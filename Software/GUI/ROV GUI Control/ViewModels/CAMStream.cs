using System;
using System.IO;
using System.Windows;
using System.Threading;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ROV_GUI_Control.ViewModels
{
    public class CAMStream(int port) : INotifyPropertyChanged, IDisposable
    {
        private int _port = port;
        private UdpClient _udpReceiver;
        private bool _isStreaming = false;
        private readonly object _lockObj = new();
        private CancellationTokenSource _cancellation;
        public int Port
        {
            get => _port;
            set
            {
                if (_port != value)
                {
                    _port = value;
                    OnPropertyChanged(nameof(Port));
                    if (_isStreaming)
                        _ = RestartAsync();
                }
            }
        }
        private BitmapImage _image = OFFLine();
        public BitmapImage Image
        {
            get => _image;
            private set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        private static BitmapImage OFFLine() => CreateFrozenImage("pack://application:,,,/Media/OFFLine.png");
        private static BitmapImage NoSignal() => CreateFrozenImage("pack://application:,,,/Media/NoSignal.png");
        private static BitmapImage CreateFrozenImage(string packUri)
        {
            var uri = new Uri(packUri, UriKind.Absolute);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        public void Start()
        {
            lock (_lockObj)
            {
                if (_isStreaming) return;
                _isStreaming = true;
                _cancellation = new CancellationTokenSource();
                Task.Run(() => ReceiveLoopAsync(_cancellation.Token));
            }
        }
        public void Stop()
        {
            lock (_lockObj)
            {
                _isStreaming = false;
                CleanupSocket();
                Image = OFFLine();
            }
        }
        private async Task RestartAsync()
        {
            lock (_lockObj)
            {
                CleanupSocket();
                if (_isStreaming)
                {
                    _cancellation = new CancellationTokenSource();
                    Task.Run(() => ReceiveLoopAsync(_cancellation.Token));
                }
            }
            await Task.CompletedTask;
        }
        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                _udpReceiver = new UdpClient(Port);
                _udpReceiver.Client.ReceiveBufferSize = 65535;
                while (!token.IsCancellationRequested)
                {
                    var receiveTask = _udpReceiver.ReceiveAsync();
                    var timeoutTask = Task.Delay(2000, token);
                    var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
                    if (completedTask == timeoutTask) {
                        if (!token.IsCancellationRequested) {
                            _ = Application.Current.Dispatcher.BeginInvoke(new Action(() => Image = NoSignal()));
                        }
                        continue;
                    }
                    var result = await receiveTask;
                    if (result.Buffer == null || result.Buffer.Length == 0) continue;
                    BitmapImage bitmapImage = null;
                    try
                    {
                        using var ms = new MemoryStream(result.Buffer);
                        bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                    }
                    catch
                    {
                        continue;
                    }
                    if (bitmapImage != null)
                    {
                        _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (!token.IsCancellationRequested)
                            {
                                Image = bitmapImage;
                            }
                        }));
                    }
                }
            }
            catch (Exception)
            {
                // معالجة الأخطاء بصمت عند إغلاق الـ Socket
            }
            finally
            {
                CleanupSocket();
            }
        }
        private void CleanupSocket()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
            try
            {
                _udpReceiver?.Close();
                _udpReceiver?.Dispose();
            }
            catch { }
            _udpReceiver = null;
        }
        public void Clear()
        {
            CleanupSocket();
        }
        public void SetPort(int port)
        {
            Port = port;
        }
        public int GetPort()
        {
            return Port;
        }
        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}