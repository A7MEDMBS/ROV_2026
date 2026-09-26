using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Windows;
using System.Text.Json;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using ROV_GUI_Control.VisionTask;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.ViewModels
{
    public class VisionTask : INotifyPropertyChanged, IDisposable
    {
        private UdpClient _udpSend;
        private UdpClient _udpReceive;
        private CancellationTokenSource _pythonCts;
        private CancellationTokenSource _ctsSend;
        private CancellationTokenSource _ctsReceive;
        private Task _pythonTask;
        private Task _sendTask, _receiveTask;

        public string RemoteIP { get; set; }
        public int LocalPort { get; set; }
        public int RemotePort { get; set; }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get => _image;
            set { if (_image != value) { _image = value; OnPropertyChanged(); } }
        }

        private string _pyFile;
        public string PyFile
        {
            get => _pyFile;
            set { if (_pyFile != value) { _pyFile = value; OnPropertyChanged(); } }
        }

        public class VisionPacket
        {
            public List<Detection> Detections { get; set; } = new();
            public Score Score { get; set; } = new();
        }

        public ObservableCollection<Detection> Detections { get; } = new();

        private Score _latestScore;
        public Score LatestScore
        {
            get => _latestScore;
            set
            {
                if (!Equals(_latestScore, value))
                {
                    _latestScore = value;
                    OnPropertyChanged();
                }
            }
        }
        public VisionTask(string remoteIp, int remotePort, int localPort)
        {
            RemoteIP = remoteIp;
            RemotePort = remotePort;
            LocalPort = localPort;
        }
        public async Task Start(string scriptPath)
        {
            try
            {
                 await Connect().ConfigureAwait(false);
                _pythonCts = new CancellationTokenSource();
                _pythonTask = Task.Run(() => RunPythonScript(scriptPath, _pythonCts.Token));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VisionTask Start error: {ex}");
            }
        }
        public async Task Stop()
        {
            try
            {
                await Disconnect().ConfigureAwait(false);
                _pythonCts?.Cancel();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VisionTask Stop error: {ex}");
            }
        }
        private async Task RunPythonScript(string script, CancellationToken cancellationToken)
        {
            var psi = new ProcessStartInfo
            {
                FileName = PyFile,
                Arguments = $"\"{script}\"",
                WorkingDirectory = Path.GetDirectoryName(script),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            try
            {
                process.Start();

                var outputTask = Task.Run(async () =>
                {
                    while (!process.HasExited && !cancellationToken.IsCancellationRequested)
                    {
                        var line = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false);
                        if (line != null)
                            Debug.WriteLine($"[Python] {line}");
                    }
                }, cancellationToken);

                var errorTask = Task.Run(async () =>
                {
                    while (!process.HasExited && !cancellationToken.IsCancellationRequested)
                    {
                        var line = await process.StandardError.ReadLineAsync().ConfigureAwait(false);
                        if (line != null)
                            Debug.WriteLine($"[Python Error] {line}");
                    }
                }, cancellationToken);

                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Cancelled
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Python script error: {ex}");
            }
            finally
            {
                if (!process.HasExited)
                {
                    try { process.Kill(); } catch { }
                }
            }
        }

        public async Task Connect()
        {
            _udpSend = new UdpClient();
            _udpReceive = new UdpClient(LocalPort);

            _ctsSend = new CancellationTokenSource();
            _ctsReceive = new CancellationTokenSource();

            _sendTask = Task.Run(() => SendLoop(_ctsSend.Token));
            _receiveTask = Task.Run(() => ReceiveLoop(_ctsReceive.Token));

            await Task.Delay(100).ConfigureAwait(false);
        }

        public async Task Disconnect()
        {
            _ctsSend?.Cancel();
            _ctsReceive?.Cancel();
            try
            {
                if (_sendTask != null) await _sendTask.ConfigureAwait(false);
                if (_receiveTask != null) await _receiveTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }

            _udpSend?.Close();
            _udpReceive?.Close();
            _udpSend?.Dispose();
            _udpReceive?.Dispose();
            _udpSend = null;
            _udpReceive = null;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Detections.Clear();
                LatestScore = null;
            });
        }

        private async Task SendLoop(CancellationToken token)
        {
            var delay = TimeSpan.FromMilliseconds(33);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (Image == null)
                    {
                        await Task.Delay(delay, token).ConfigureAwait(false);
                        continue;
                    }

                    var jpegBytes = await Task.Run(() =>
                    {
                        using var ms = new MemoryStream();
                        var encoder = new JpegBitmapEncoder { QualityLevel = 75 };
                        encoder.Frames.Add(BitmapFrame.Create(Image));
                        encoder.Save(ms);
                        return ms.ToArray();
                    }, token).ConfigureAwait(false);

                    await _udpSend.SendAsync(jpegBytes, jpegBytes.Length,
                        new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort)).ConfigureAwait(false);

                    await Task.Delay(delay, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Send error: {ex}");
                }
            }
        }

        private DateTime _lastUIUpdate = DateTime.MinValue;

        private async Task ReceiveLoop(CancellationToken token)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpReceive.ReceiveAsync().ConfigureAwait(false);
                    string json = Encoding.UTF8.GetString(result.Buffer);

                    var packet = await Task.Run(() =>
                        JsonSerializer.Deserialize<VisionPacket>(json, options), token).ConfigureAwait(false);

                    //if (packet != null)
                        //Unpacking(packet);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Receive error: {ex}");
                    await Task.Delay(100, token).ConfigureAwait(false);
                }
            }
        }

        private void Unpacking(VisionPacket packet)
        {
            // throttle UI updates to max 10 per second
            if (DateTime.UtcNow - _lastUIUpdate < TimeSpan.FromMilliseconds(100))
                return;

            _lastUIUpdate = DateTime.UtcNow;

            // schedule UI update asynchronously:
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (packet.Detections != null)
                    {
                        // update existing ObservableCollection instead of replacing
                        Detections.Clear();
                        foreach (var d in packet.Detections)
                            Detections.Add(d);
                    }

                    if (packet.Score != null)
                    {
                        LatestScore = packet.Score;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UI update error: {ex.Message}");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Dispose()
        {
            _pythonCts?.Cancel();
            _ = Disconnect();
        }
    }
}
