using System;
using OxyPlot;
using System.Linq;
using OpenCvSharp;
using System.Text;
using static MAVLink;
using System.Windows;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using ROV_GUI_Control.Services;
using OpenCvSharp.WpfExtensions;
using ROV_GUI_Control.VisionTask;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media.Imaging;
using ROV_GUI_Control.Configuration;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ROV_GUI_Control.ViewModels
{
    public class MainViewModel : ObservableObject, INotifyPropertyChanged, IDisposable
    {
        public string AppCopyright => $"Copyright © {DateTime.Now.Year} Assiut Robotics Team. All rights reserved.";
        #region Connection
        private UdpClient UdpClient;
        private string _localIp;
        public string LocalIP
        {
            get => _localIp;
            set
            {
                if (_localIp != value)
                {
                    _localIp = value;
                    OnPropertyChanged(nameof(LocalIP));
                    ConfigManager.Current.LocalIP = value;
                    ConfigManager.Save();
                    ResetIP();
                }
            }
        }
        private string _remoteIp;
        public string RemoteIP
        {
            get => _remoteIp;
            set
            {
                if (_remoteIp != value)
                {
                    _remoteIp = value;
                    OnPropertyChanged(nameof(RemoteIP));
                    ConfigManager.Current.RemoteIP = value;
                    ConfigManager.Save();
                    ResetIP();
                }
            }
        }
        private int _localPort;
        public int LocalPort
        {
            get => _localPort;
            set
            {
                if (_localPort != value)
                {
                    _localPort = value;
                    OnPropertyChanged(nameof(LocalPort));
                    ConfigManager.Current.LocalPort = value;
                    ConfigManager.Save();
                    ResetIP();
                }
            }
        }
        private int _remotePort;
        public int RemotePort
        {
            get => _remotePort;
            set
            {
                if (_remotePort != value)
                {
                    _remotePort = value;
                    OnPropertyChanged(nameof(RemotePort));
                    ConfigManager.Current.RemotePort = value;
                    ConfigManager.Save();
                    ResetIP();
                }
            }
        }
        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
                ConfigManager.Current.UserName = value;
                ConfigManager.Save();
            }
        }
        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                ConfigManager.Current.Password = value;
                ConfigManager.Save();
            }
        }
        private int _cam1_port;
        public int Cam1_Port
        {
            get => _cam1_port;
            set
            {
                if (_cam1_port != value)
                {
                    _cam1_port = value;
                    OnPropertyChanged(nameof(Cam1_Port));
                    ConfigManager.Current.Cam1_Port = value;
                    ConfigManager.Save();
                    Feed1?.SetPort(Cam1_Port);
                }
            }
        }
        private int _cam2_port;
        public int Cam2_Port
        {
            get => _cam2_port;
            set
            {
                if (_cam2_port != value)
                {
                    _cam2_port = value;
                    OnPropertyChanged(nameof(Cam2_Port));
                    ConfigManager.Current.Cam2_Port = value;
                    ConfigManager.Save();
                    Feed2?.SetPort(Cam2_Port);
                }
            }
        }
        private int _cam3_port;
        public int Cam3_Port
        {
            get => _cam3_port;
            set
            {
                if (_cam3_port != value)
                {
                    _cam3_port = value;
                    OnPropertyChanged(nameof(Cam3_Port));
                    ConfigManager.Current.Cam3_Port = value;
                    ConfigManager.Save();
                    Feed3?.SetPort(Cam3_Port);
                }
            }
        }
        private string _connectedindtext = "";
        public string ConnectedIndText
        {
            get => _connectedindtext;
            set
            {
                _connectedindtext = value;
                OnPropertyChanged(nameof(ConnectedIndText));
            }
        }
        public ICommand ConnectCommand { get; }
        private bool _isConnect = false;
        public bool IsConnect
        {
            get => _isConnect;
            set
            {
                if (_isConnect != value)
                {
                    _isConnect = value;
                    OnPropertyChanged(nameof(IsConnect));
                }
            }
        }
        public bool IsConnectButtonBusy { get; set; }
        private async void ExecuteConnectCommand()
        {
            if (IsConnectButtonBusy) return;
            IsConnectButtonBusy = true;
            try
            {
                if (!IsConnect) {
                    bool IsReachable = await ConnectionCheckAsync(RemoteIP);
                    if (!IsReachable) {
                        MessageBox.Show($"Connect to {RemoteIP} Failed!", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    try 
                    {
                        int managerPort = 10000;
                        string response = await Task.Run(async () => {
                            try
                            {
                                byte[] data = Encoding.ASCII.GetBytes($"START_UART:{LocalIP}");
                                await UdpClient.SendAsync(data, data.Length, RemoteIP, managerPort);
                                var receiveTask = UdpClient.ReceiveAsync();
                                var delayTask = Task.Delay(1500);
                                var completedTask = await Task.WhenAny(receiveTask, delayTask);
                                if (completedTask == receiveTask)
                                {
                                    var result = await receiveTask;
                                    return Encoding.ASCII.GetString(result.Buffer);
                                }
                                else
                                {
                                    UdpClient.Close();
                                    UdpClient = new UdpClient();
                                    return "START_UART_TIMEOUT";
                                }
                            }
                            catch (ObjectDisposedException)
                            {
                                return "START_UART_TIMEOUT";
                            }
                            catch(Exception ex)
                            {
                                return $"ERROR: {ex.Message}";
                            }
                        });
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var ts = $"[{DateTime.Now:HH:mm:ss}] {response}";
                            StatusLines.Add(ts);
                            while (StatusLines.Count > MaxLines)
                                StatusLines.RemoveAt(0);
                            if (response != "START_UART_TIMEOUT") {
                                MavlinkHandler?.Connect(RemoteIP, RemotePort);
                                JOYSTICK?.Connect(RemoteIP, RemotePort);
                                UITimer?.Start();
                                ConnectedIndText = $"Connected to {RemoteIP}";
                                IsConnectedInd = true;
                                IsConnect = true;
                                ConnectedTimer?.Start();
                            }
                            else {
                                MessageBox.Show($"Raspberry Manager Replied: {response}", "Execution Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Connect Failed: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else {
                    ConnectedTimer?.Stop();
                    IsConnectedInd = false;
                    ConnectedIndText = "";
                    MavlinkHandler?.Disconnect();
                    JOYSTICK?.Disconnect();
                    UITimer?.Stop();
                    try
                    {
                        int managerPort = 10000;
                        await Task.Run(async () =>
                        {
                            try
                            {
                                string ts = "";
                                byte[] uartData = Encoding.ASCII.GetBytes("STOP_UART");
                                await UdpClient.SendAsync(uartData, uartData.Length, RemoteIP, managerPort);
                                var uartReceiveTask = UdpClient.ReceiveAsync();
                                var uartDelayTask = Task.Delay(1500);
                                var completedUartTask = await Task.WhenAny(uartReceiveTask, uartDelayTask);
                                if (completedUartTask == uartReceiveTask) {
                                    var result = await uartReceiveTask;
                                    string response = Encoding.ASCII.GetString(result.Buffer);
                                    ts = $"[{DateTime.Now:HH:mm:ss}] {response}";
                                }
                                else {
                                    ts = $"[{DateTime.Now:HH:mm:ss}] STOP_UART_TIMEOUT !";
                                }
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    StatusLines.Add(ts);
                                    while (StatusLines.Count > MaxLines)
                                        StatusLines.RemoveAt(0);
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[GCS] Error during disconnect: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Disconnect Failed: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    if (IsStream) {
                        _ = StopStreaming();
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsConnectedInd = false;
                        ConnectedIndText = "";
                        IsStream = false;
                        IsPower = false;
                        IsEnable = false;
                        IsLastEnable = false;
                        IsConnect = false;
                        _cancellationTokenSource?.Cancel();
                        IsScanning = false;
                        ResetUi();
                    });
                }
            }
            finally
            {
                IsConnectButtonBusy = false;
            }
        }
        public static async Task<bool> ConnectionCheckAsync(string remoteIP, int port = 22, int timeoutMs = 1500)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(remoteIP, port);
                var timeoutTask = Task.Delay(timeoutMs);
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                return completedTask == connectTask && client.Connected;
            }
            catch
            {
                return false;
            }
        }
        private async void Connected_Tick(object sender, EventArgs e)
        {
            if (ConnectTickRunning) return;
            ConnectTickRunning = true;

            try
            {
                bool connected = await ConnectionCheckAsync(RemoteIP);
                if (connected)
                {
                    IsConnectedInd = true;
                    ConnectedIndText = $"Connected to {RemoteIP}";
                    var hideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    hideTimer.Tick += (s, ev) =>
                    {
                        IsConnectedInd = false;
                        ConnectedIndText = "";
                        hideTimer.Stop();
                    };
                    hideTimer.Start();
                }
                else
                {
                    ConnectedTimer?.Stop();
                    MavlinkHandler?.Disconnect();
                    JOYSTICK?.Disconnect();
                    UITimer?.Stop();
                    if (IsStream)
                        _ = StopStreaming();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _cancellationTokenSource?.Cancel();
                        IsScanning = false;
                        IsConnectedInd = false;
                        ConnectedIndText = "";
                        IsStream = false;
                        IsPower = false;
                        IsEnable = false;
                        IsLastEnable = false;
                        IsConnect = false;
                        ResetUi();
                    });
                }
            }
            finally
            {
                ConnectTickRunning = false;
            }
        }
        private async void ResetIP()
        {
            MavlinkHandler?.Disconnect();
            JOYSTICK?.Disconnect();
            UITimer?.Stop();
            if (IsConnect)
            {
                IsConnect = await ConnectionCheckAsync(RemoteIP);
                if (IsConnect)
                {
                    MavlinkHandler?.Connect(RemoteIP, RemotePort);
                    JOYSTICK?.Connect(RemoteIP, RemotePort);
                    UITimer?.Start();
                }
                else
                {
                    ConnectedIndText = "";
                    IsConnectedInd = false;
                    ConnectedTimer?.Stop();
                    IsConnect = false;
                    IsPower = false;
                    IsEnable = false;
                    IsLastEnable = false;
                    IsStream = false;
                }
            }
        }
        #endregion
        #region Mavlink
        private readonly byte SystemID = 1;
        private readonly byte ComponentID = 1;
        private static readonly MavlinkParse mavlinkParser = new();
        private readonly MAVLinkHandler MavlinkHandler;
        private readonly DispatcherTimer HeartbeatTimer;
        private readonly DispatcherTimer ConnectedTimer;
        private bool ConnectTickRunning = false;
        private const int MaxLines = 200;

        private string _heartbeatText;
        public string HeartBeatText
        {
            get => _heartbeatText;
            set
            {
                _heartbeatText = value;
                OnPropertyChanged();
                
            }
        }
        private StringBuilder _statustext = new();
        public string StatusText
        {
            get => _statustext.ToString();
            set
            {
                _statustext = new StringBuilder(value);
                OnPropertyChanged(nameof(StatusText));
            }
        }
        private float _speed = 0;
        public float Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }
        private float _depth = 0;
        public float Depth
        {
            get => _depth;
            set
            {
                _depth = value;
                OnPropertyChanged(nameof(Depth));
            }
        }
        private float _comp_value = 0;
        public float CompValue
        {
            get => _comp_value;
            set
            {
                _comp_value = value;
                OnPropertyChanged(nameof(CompValue));
            }
        }
        private float _roll = 0;
        public float Roll
        {
            get { return _roll; }
            set
            {
                if (_roll != value)
                {
                    _roll = value;
                    OnPropertyChanged(nameof(Roll));
                }
            }
        }
        private float _pitch = 0;
        public float Pitch
        {
            get { return _pitch; }
            set
            {
                if (_pitch != value)
                {
                    _pitch = value;
                    OnPropertyChanged(nameof(Pitch));
                }
            }
        }
        private float _yaw = 0;
        public float Yaw
        {
            get { return _yaw; }
            set
            {
                if (_yaw != value)
                {
                    _yaw = value;
                    OnPropertyChanged(nameof(Yaw));
                }
            }
        }
        private float _water_temp = 0;
        public float Water_Temp
        {
            get { return _water_temp; }
            set
            {
                if (_water_temp != value)
                {
                    _water_temp = value;
                    OnPropertyChanged(nameof(Water_Temp));
                }
            }
        }
        private float _water_press = 0;
        public float Water_Press
        {
            get { return _water_press; }
            set
            {
                if (_water_press != value)
                {
                    _water_press = value;
                    OnPropertyChanged(nameof(Water_Press));
                }
            }
        }
        private float _tube_temp = 0;
        public float Tube_Temp
        {
            get { return _tube_temp; }
            set
            {
                if (_tube_temp != value)
                {
                    _tube_temp = value;
                    OnPropertyChanged(nameof(Tube_Temp));
                }
            }
        }
        private float _tube_press = 0;
        public float Tube_Press
        {
            get { return _tube_press; }
            set
            {
                if (_tube_press != value)
                {
                    _tube_press = value;
                    OnPropertyChanged(nameof(Tube_Press));
                }
            }
        }
     

        private readonly ConcurrentQueue<string> StatusQueue = [];
        public ObservableCollection<string> StatusLines { get; } = [];

        private bool _ismpuind = false;
        public bool IsMPUInd
        {
            get => _ismpuind;
            set
            {
                _ismpuind = value;
                OnPropertyChanged(nameof(IsMPUInd));
            }
        }
        private bool _ishmcind = false;
        public bool IsHMCInd
        {
            get => _ishmcind;
            set
            {
                _ishmcind = value;
                OnPropertyChanged(nameof(IsHMCInd));
            }
        }
        private bool _ismscind = false;
        public bool IsMSCInd
        {
            get => _ismscind;
            set
            {
                _ismscind = value;
                OnPropertyChanged(nameof(IsMSCInd));
            }
        }
        private bool _islightind = false;
        public bool IsLightInd
        {
            get => _islightind;
            set
            {
                if (!value && _islightind && IsLight)
                    IsLight = false;
                if (value && !_islightind && !IsLight)
                    IsLight = true;
                _islightind = value;
                OnPropertyChanged(nameof(IsLightInd));
            }
        }
        private bool _isgripperclosedind = false;
        public bool IsGripperClosedInd
        {
            get => _isgripperclosedind;
            set
            {
                _isgripperclosedind = value;
                OnPropertyChanged(nameof(IsGripperClosedInd));
            }
        }
        private bool _isgripperrotateind = false;
        public bool IsGripperRotateInd
        {
            get => _isgripperrotateind;
            set
            {
                _isgripperrotateind = value;
                OnPropertyChanged(nameof(IsGripperRotateInd));
            }
        }

        private int _t0_pwm;
        public int T0_PWM
        {
            get => _t0_pwm;
            set { _t0_pwm = value; OnPropertyChanged(nameof(T0_PWM)); }
        }

        private int _t1_pwm;
        public int T1_PWM
        {
            get => _t1_pwm;
            set { _t1_pwm = value; OnPropertyChanged(nameof(T1_PWM)); }
        }

        private int _t2_pwm;
        public int T2_PWM
        {
            get => _t2_pwm;
            set { _t2_pwm = value; OnPropertyChanged(nameof(T2_PWM)); }
        }

        private int _t3_pwm;
        public int T3_PWM
        {
            get => _t3_pwm;
            set { _t3_pwm = value; OnPropertyChanged(nameof(T3_PWM)); }
        }

        private int _t4_pwm;
        public int T4_PWM
        {
            get => _t4_pwm;
            set { _t4_pwm = value; OnPropertyChanged(nameof(T4_PWM)); }
        }

        private int _t5_pwm;
        public int T5_PWM
        {
            get => _t5_pwm;
            set { _t5_pwm = value; OnPropertyChanged(nameof(T5_PWM)); }
        }

        private int _t6_pwm;
        public int T6_PWM
        {
            get => _t6_pwm;
            set { _t6_pwm = value; OnPropertyChanged(nameof(T6_PWM)); }
        }
        private bool _isheartbeatind = false;
        public bool IsHeartbeatInd
        {
            get => _isheartbeatind;
            set
            {
                _isheartbeatind = value;
                OnPropertyChanged(nameof(IsHeartbeatInd));
            }
        }
        private bool _isconnectedind = false;
        public bool IsConnectedInd
        {
            get => _isconnectedind;
            set
            {
                _isconnectedind = value;
                OnPropertyChanged(nameof(IsConnectedInd));
            }
        }
        private void Heartbeat_Tick(object sender, EventArgs e)
        {
            IsHeartbeatInd = false;
            HeartbeatTimer.Stop();
        }
        private async Task SendCommand(int CMDType)
        {
            var command = new MAVLink.mavlink_command_long_t
            {
                target_system = 1,
                target_component = 1,
                confirmation = 0
            };
            switch (CMDType)
            {
                case 0: 
                    command.command = (ushort)MAV_CMD.COMPONENT_ARM_DISARM;
                    command.param1 = IsLastEnable ? 1 : 0;
                    break;
                case 1: 
                    command.command = (ushort)MAV_CMD.DO_SET_RELAY;
                    command.param1 = GripperCLS ? 1 : 0;
                    command.param2 = GripperRT ? 1 : 0;
                    command.param3 = IsLight ? 1 : 0;
                    break;
                case 2: 
                    command.command = (ushort)MAV_CMD.DO_SET_SERVO;
                    command.param1 = (int)Brightness;
                    break;
            }
            var packet = mavlinkParser.GenerateMAVLinkPacket20(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, command, false, SystemID, ComponentID);
            await MavlinkHandler.SendCommand(packet);
        }
        #endregion
        #region Stream
        private readonly CAMStream Feed1;
        private BitmapImage _image1;
        public BitmapImage Image1
        {
            get => _image1;
            set
            {
                _image1 = value;
                OnPropertyChanged(nameof(Image1));
            }
        }
        private readonly CAMStream Feed2;
        private BitmapImage _image2;
        public BitmapImage Image2
        {
            get => _image2;
            set
            {
                _image2 = value;
                OnPropertyChanged(nameof(Image2));
            }
        }
        private readonly CAMStream Feed3;
        private BitmapImage _image3;
        public BitmapImage Image3
        {
            get => _image3;
            set
            {
                _image3 = value;
                OnPropertyChanged(nameof(Image3));
            }
        }
        public bool IsStreamButtonBusy { get; set; }
        public async Task StartStreaming()
        {
            IsStreamButtonBusy = true;
            try
            {
                Index1 = PIndex1;
                Index2 = PIndex2;
                Index3 = PIndex3;
                Feed1.Start();
                Feed2.Start();
                Feed3.Start();
                int managerPort = 10000;
                await Task.Run(async () =>
                {
                    try
                    {
                        string ts = "";
                        byte[] streamData = Encoding.ASCII.GetBytes($"START_STREAM:{LocalIP}");
                        await UdpClient.SendAsync(streamData, streamData.Length, RemoteIP, managerPort);
                        var streamReceiveTask = UdpClient.ReceiveAsync();
                        var streamDelayTask = Task.Delay(1500);
                        var completedStreamTask = await Task.WhenAny(streamReceiveTask, streamDelayTask);
                        if (completedStreamTask == streamReceiveTask)
                        {
                            var result = await streamReceiveTask;
                            string response = Encoding.ASCII.GetString(result.Buffer);
                            ts = $"[{DateTime.Now:HH:mm:ss}] {response}";
                        }
                        else
                        {
                            ts = $"[{DateTime.Now:HH:mm:ss}] START_STREAM_TIMEOUT !";
                        }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusLines.Add(ts);
                            while (StatusLines.Count > MaxLines)
                                StatusLines.RemoveAt(0);
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GCS] Error during disconnect: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                IsStream = false;
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show($"Failed to start stream: {ex.Message}", "Streaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
            finally
            {
                IsStreamButtonBusy = false;
            }
        }
        public async Task StopStreaming()
        {
            IsStreamButtonBusy = true;
            try
            {
                int managerPort = 10000;
                await Task.Run(async () =>
                {
                    try
                    {
                        string ts = "";
                        byte[] streamData = Encoding.ASCII.GetBytes("STOP_STREAM");
                        await UdpClient.SendAsync(streamData, streamData.Length, RemoteIP, managerPort);
                        var streamReceiveTask = UdpClient.ReceiveAsync();
                        var streamDelayTask = Task.Delay(1500);
                        var completedStreamTask = await Task.WhenAny(streamReceiveTask, streamDelayTask);
                        if (completedStreamTask == streamReceiveTask) {
                            var result = await streamReceiveTask;
                            string response = Encoding.ASCII.GetString(result.Buffer);
                            ts = $"[{DateTime.Now:HH:mm:ss}] {response}";
                        }
                        else {
                            ts = $"[{DateTime.Now:HH:mm:ss}] STOP_STREAM_TIMEOUT !";
                        }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusLines.Add(ts);
                            while (StatusLines.Count > MaxLines)
                                StatusLines.RemoveAt(0);
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GCS] Error during disconnect: {ex.Message}");
                    }
                });
                Feed1?.Stop();
                Feed2?.Stop();
                Feed3?.Stop();
            }
            catch (Exception ex)
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show($"Failed to stop stream: {ex.Message}", "Streaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
            finally
            {
                IsStreamButtonBusy = false;
            }
        }
        public ICommand StreamCommand { get; }
        private void ExecuteStreamCommand()
        {
            if (IsStreamButtonBusy) return;
            if (!IsConnect)
            {
                MessageBox.Show("No Connection.", "Connection Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsStream = !IsStream;
            if (IsStream)
                _ = StartStreaming();
            else
                _ = StopStreaming();
        }
        private bool _isStream;
        public bool IsStream
        {
            get { return _isStream; }
            set
            {
                if (_isStream != value)
                {
                    _isStream = value;
                    OnPropertyChanged(nameof(IsStream));
                }
            }
        }
        public ObservableCollection<string> Feed_Comb { get; }
        public int PIndex1;
        public int PIndex2;
        public int PIndex3;
        private int  _index1 = 0;
        public int Index1
        {
            get => _index1;
            set
            {
                if (_index1 != value)
                {
                    _index1 = value;
                    OnPropertyChanged(nameof(Index1));
                    _ = FeedChange(1);
                }
            }
        }
        private int _index2 = 0;
        public int Index2
        {
            get => _index2;
            set
            {
                if (_index2 != value)
                {
                    _index2 = value;
                    OnPropertyChanged(nameof(Index2));
                    _ = FeedChange(2);
                }
            }
        }
        private int _index3 = 0;
        public int Index3
        {
            get => _index3;
            set
            {
                if (_index3 != value)
                {
                    _index3 = value;
                    OnPropertyChanged(nameof(Index3));
                    _ = FeedChange(3);
                }
            }
        }
        private void IndexChange(ref int a, ref int b,ref int c, ref int d,int e)
         {
            d = b;
            int temp = b;
            b = a;
            if (c > 0) // index(X) should change last after (b) change so no two change happen at the same time
                switch (e)
                {
                    case 1:
                        Index1 = temp;
                        break;
                    case 2:
                        Index2 = temp;
                        break;
                    case 3:
                        Index3 = temp;
                        break;
                    default: break;
                }
        }
        private async Task FeedChange(int a)
        {
            await Task.Run(() =>
            {
                switch (a)
                {
                    case 1:
                        if(Index1 == 0)
                        {
                            if(IsStream)
                                PIndex1 = Index1;
                        }
                        else if (Index1 == Index2 || Index1 == PIndex2)
                        {
                            Feed1.Clear();
                            Feed2.Clear();
                            int p = Feed1.GetPort();
                            Feed1.SetPort(Feed2.GetPort());
                            Feed2.SetPort(p);
                            IndexChange(ref _index1, ref PIndex1, ref _index2, ref PIndex2, 2);
                        }
                        else if (_index1 == Index3 || _index1 == PIndex3)
                        {
                            Feed1.Clear();
                            Feed3.Clear();
                            int p = Feed1.GetPort();
                            Feed1.SetPort(Feed3.GetPort());
                            Feed3.SetPort(p);
                            IndexChange(ref _index1, ref PIndex1, ref _index3, ref PIndex3, 3);
                        }
                        else
                        {
                            PIndex1 = Index1;
                        }
                        break;
                    case 2:
                        if (Index2 == 0)
                        {
                            if (IsStream)
                                PIndex2 = Index2;
                        }
                        else if (_index2 == _index1 || _index2 == PIndex1)
                        {
                            int p = Feed2.GetPort();
                            Feed1.Clear();
                            Feed2.Clear();
                            Feed2.SetPort(Feed1.GetPort());
                            Feed1.SetPort(p);
                            IndexChange(ref _index2, ref PIndex2, ref _index1, ref PIndex1, 1);
                        }
                        else if (_index2 == _index3 || _index2 == PIndex3)
                        {
                            int p = Feed2.GetPort();
                            Feed2.Clear();
                            Feed3.Clear();
                            Feed2.SetPort(Feed3.GetPort());
                            Feed3.SetPort(p);
                            IndexChange(ref _index2, ref PIndex2, ref _index3, ref PIndex3, 3);
                        }
                        else
                        {
                            PIndex2 = Index2;
                        }
                        break;
                    case 3:
                        if (Index3 == 0)
                        {
                            if (IsStream)
                                PIndex3 = Index3;
                        }
                        else if (_index3 == _index1 || _index3 == PIndex1)
                        {
                            Feed1.Clear();
                            Feed3.Clear();
                            int p = Feed3.GetPort();
                            Feed3.SetPort(Feed1.GetPort());
                            Feed1.SetPort(p);
                            IndexChange(ref _index3, ref PIndex3, ref _index1, ref PIndex1, 1);
                        }
                        else if (_index3 == _index2 || _index3 == PIndex2)
                        {
                            Feed2.Clear();
                            Feed3.Clear();
                            int p = Feed3.GetPort();
                            Feed3.SetPort(Feed2.GetPort());
                            Feed2.SetPort(p);
                            IndexChange(ref _index3, ref PIndex3, ref _index2, ref PIndex2, 2);
                        }
                        else
                        {
                            PIndex3 = Index3;
                        }
                        break;
                    default: break;
                }
            });
        }
        #endregion
        #region Plot
        private readonly Plot tubeTemp;
        private readonly Plot tubePressure;
        private readonly Plot waterTemp;
        private readonly Plot waterPressure;
        public PlotModel TubeTempModel { get; set; }
        public PlotModel TubePressureModel { get; set; }
        public PlotModel WaterTempModel { get; set; }
        public PlotModel WaterPressureModel { get; set; }
        #endregion
        #region Power on/off
        public bool IsPowerButtonBusy { get; set; }
        public ICommand PowerCommand { get; }
        private bool _isPower = false;
        public bool IsPower
        {
            get => _isPower;
            set
            {
                if (_isPower != value)
                {
                    _isPower = value;
                    OnPropertyChanged(nameof(IsPower));
                }
            }
        }
        private async void ExecutePowerCommand()
        {
            if (IsPowerButtonBusy) return;
            if (!IsConnect)
            {
                MessageBox.Show("No Connection.", "Connection Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsPowerButtonBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    IsPower = !IsPower;
                });
            }
            catch
            {
            }
            finally
            {
                IsPowerButtonBusy = false;
            }
        }
        #endregion
        #region Enable/Disable
        public bool IsEnableButtonBusy { get; set; }
        public ICommand EnableCommand { get; }

        private bool _isLastEnable = false;
        public bool IsLastEnable
        {
            get { return _isLastEnable; }
            set
            {
                if (_isLastEnable != value)
                {
                    _isLastEnable = value;
                    OnPropertyChanged(nameof(IsLastEnable));
                }
            }
        }
        private bool _isEnable = false;
        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                if (_isEnable != value)
                {
                    if(IsEnable && IsLastEnable)
                        IsLastEnable = false;
                    _isEnable = value;
                    OnPropertyChanged(nameof(IsEnable));
                }
            }
        }
        private async void ExecuteEnableCommand()
        {
            if (IsEnableButtonBusy) return;
            if (!IsConnect)
            {
                MessageBox.Show("No Connection.", "Connection Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsEnableButtonBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    /*if (!IsPower)
                    {
                        MessageBox.Show("Power Is OFF.", "Power Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }*/
                    IsLastEnable = !IsLastEnable;
                    _ = SendCommand(0);
                });
            }
            catch
            {
            }
            finally
            {
                IsEnableButtonBusy = false;
            }
        }
        #endregion
        #region Settings
        private readonly WindowService SettingsWindow;
        public ICommand SettingsCommand { get; }
        private readonly bool _isSettings = false;
        public bool IsSettings
        {
            get => _isSettings;
            set
            {
                if (_isSettings != value)
                {
                    _isConnect = value;
                    OnPropertyChanged(nameof(IsSettings));
                }
            }
        }
        private void ExecuteSettingsCommand()
        {
            var (localIP, remoteIp, username, password, localPort, remotePort, cam1_port, cam2_port, cam3_port, vision_localPort, vision_remotePort, pyfile, task1_Path, task2_Path, task3_Path, task4_Path, is_RollPID, is_PitchPID, is_DepthPID) = SettingsWindow.ShowWindow(LocalIP, RemoteIP, UserName, Password, LocalPort, RemotePort, Cam1_Port, Cam2_Port, Cam3_Port, Vision_LocalPort, Vision_RemotePort, PyFile, Task1_Path, Task2_Path, Task3_Path, Task4_Path, IsConnect, IsRollPID, IsPitchPID, IsDepthPID);
            LocalIP = localIP;
            RemoteIP = remoteIp;
            LocalPort = localPort;
            RemotePort = remotePort;
            UserName = username;
            Password = password;
            Cam1_Port = cam1_port;
            Cam2_Port = cam2_port;
            Cam3_Port = cam3_port;
            Vision_LocalPort = vision_localPort;
            Vision_RemotePort = vision_remotePort;
            PyFile = pyfile;
            Task1_Path = task1_Path;
            Task2_Path = task2_Path;
            Task3_Path = task3_Path;
            Task4_Path = task4_Path;
        }
        #endregion
        #region Light on/off
        public bool IsLightButtonBusy { get; set; }
        public ICommand LightCommand { get; }
        public ICommand BrightnessCommand { get; }

        private bool _isLight = false;
        public bool IsLight
        {
            get { return _isLight; }
            set
            {
                if (_isLight != value)
                {
                    _isLight = value;
                    OnPropertyChanged(nameof(IsLight));
                }
            }
        }
        private int _brightness;
        public int Brightness
        {
            get => _brightness;
            set
            {
                if (_brightness != value)
                {
                    _brightness = value;
                    OnPropertyChanged(nameof(Brightness));
                }
            }
        }
        private async void ExecuteLightCommand()
        {
            if (IsLightButtonBusy) return;
            if (!IsConnect)
            {
                MessageBox.Show("No Connection.", "Connection Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsLightButtonBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    IsLight = !IsLight;
                    if (IsLight)
                    {
                        _ = SendCommand(2);
                        _ = SendCommand(1);
                    }
                    else
                    {
                        _ = SendCommand(1);
                    }
                });
            }
            catch
            {
            }
            finally
            {
                IsLightButtonBusy = false;
            }
        }
        private void ExecuteBrightnessCommand()
        {
            if (IsLightInd)
                _ = SendCommand(2);
        }
        #endregion
        #region Joystick
        private readonly JOYStick JOYSTICK;
        private int _hmove;
        public int H_move
        {
            get => _hmove;
            set
            {
                _hmove = value;
                OnPropertyChanged(nameof(H_move));
            }
        }
        private int _vmove;
        public int V_move
        {
            get => _vmove;
            set
            {
                _vmove = value;
                OnPropertyChanged(nameof(V_move));
            }
        }
        private bool _gripperCLS;
        public bool GripperCLS
        {
            get { return _gripperCLS; }
            set
            {
                if (_gripperCLS != value)
                {
                    _gripperCLS = value;
                    OnPropertyChanged(nameof(GripperCLS));
                    if (IsEnable)
                        _ = SendCommand(1);
                }
            }
        }
        private bool _gripperRTT;
        public bool GripperRT
        {
            get { return _gripperRTT; }
            set
            {
                if (_gripperRTT != value)
                {
                    _gripperRTT = value;
                    OnPropertyChanged(nameof(GripperRT));
                    if(IsEnable)
                        _ = SendCommand(1);
                }
            }
        }
        private bool _addmark = false;
        public bool AddMark
        {
            get => _addmark;
            set
            {
                _addmark = value;
                OnPropertyChanged(nameof(AddMark));
            }
        }
        #endregion
        #region 3D
        #endregion
        #region PID
        private bool _isRollPID = false;
        public bool IsRollPID
        {
            get => _isRollPID;
            set
            {
                if (_isRollPID != value)
                {
                    _isRollPID = value;
                    OnPropertyChanged(nameof(IsRollPID));
                }
            }
        }
        private bool _isPitchPID = false;
        public bool IsPitchPID
        {
            get => _isPitchPID;
            set
            {
                if (_isPitchPID != value)
                {
                    _isPitchPID = value;
                    OnPropertyChanged(nameof(IsPitchPID));
                }
            }
        }
        private bool _isDepthPID = false;
        public bool IsDepthPID
        {
            get => _isDepthPID;
            set
            {
                if (_isDepthPID != value)
                {
                    _isDepthPID = value;
                    OnPropertyChanged(nameof(IsDepthPID));
                }
            }
        }
        #endregion
        #region VisionTasks
        private CancellationTokenSource _cancellationTokenSource;
        private bool IsScanning = false;
        private ObservableCollection<Detection> _detectedQrCodes;
        public ObservableCollection<Detection> DetectedQrCodes
        {
            get => _detectedQrCodes;
            set
            {
                _detectedQrCodes = value;
            }
        }

        private int _vision_LocalPort;
        public int Vision_LocalPort
        {
            get => _vision_LocalPort;
            set
            {
                if (_vision_LocalPort != value)
                {
                    _vision_LocalPort = value;
                    OnPropertyChanged(nameof(Vision_LocalPort));
                    ConfigManager.Current.Vision_LocalPort = value;
                    ConfigManager.Save();
                }
            }
        }
        private int _vision_RemotePort;
        public int Vision_RemotePort
        {
            get => _vision_RemotePort;
            set
            {
                if (_vision_RemotePort != value)
                {
                    _vision_RemotePort = value;
                    OnPropertyChanged(nameof(Vision_RemotePort));
                    ConfigManager.Current.Vision_RemotePort = value;
                    ConfigManager.Save();
                }
            }
        }
        private string _pyFile;
        public string PyFile
        {
            get => _pyFile;
            set { 
                if (_pyFile != value) {
                    _pyFile = value; 
                    OnPropertyChanged(nameof(PyFile));
                    ConfigManager.Current.PyFile = value;
                    ConfigManager.Save();
                } 
            }
        }
        private string _task1_Path;
        public string Task1_Path
        {
            get => _task1_Path;
            set { 
                if (_task1_Path != value) {
                    _task1_Path = value; 
                    OnPropertyChanged(nameof(Task1_Path));
                    ConfigManager.Current.Task1_Path = value;
                    ConfigManager.Save();
                } 
            }
        }
        private string _task2_Path;
        public string Task2_Path
        {
            get => _task2_Path;
            set { 
                if (_task2_Path != value) {
                    _task2_Path = value; 
                    OnPropertyChanged(nameof(Task2_Path));
                    ConfigManager.Current.Task2_Path = value;
                    ConfigManager.Save();
                } 
            }
        }
        private string _task3_Path;
        public string Task3_Path
        {
            get => _task3_Path;
            set { 
                if (_task3_Path != value) { 
                    _task3_Path = value; 
                    OnPropertyChanged(nameof(Task3_Path));
                    ConfigManager.Current.Task3_Path = value;
                    ConfigManager.Save();
                } 
            }
        }
        private string _task4_Path;
        public string Task4_Path
        {
            get => _task4_Path;
            set { 
                if (_task4_Path != value) { 
                    _task4_Path = value; 
                    OnPropertyChanged(nameof(Task4_Path));
                    ConfigManager.Current.Task4_Path = value;
                    ConfigManager.Save();
                } 
            }
        }
        public VisionTask VisionTask { get; }
        //public ObservableCollection<Detection> Detections => VisionTask.Detections;

        private Score _latestScore;
        public Score LatestScore
        {
            get => _latestScore;
            set { _latestScore = value; OnPropertyChanged(nameof(LatestScore)); }
        }

        private ICommand _task_Command;
        public ICommand Task_Command =>
            _task_Command ??= new RelayCommand<string>(async param => await ExecuteTask_Command(int.Parse(param)));
        private ICommand _visionStopCommand;
        public ICommand VisionStopCommand =>
            _visionStopCommand ??= new RelayCommand(async () => await ExecuteVisionStopCommand());
        private async Task ExecuteTask_Command(int param)
        {
            if (IsScanning) return;
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;
            try
            {
                if (param == 1)
                {
                    IsScanning = true;
                    while (!token.IsCancellationRequested)
                    {
                        if (Image1 == null || Image1 is not BitmapSource currentBitmap)
                        {
                            await Task.Delay(100, token);
                            AddStatusLine("NO IMAGE !");
                            continue;
                        }
                        string result = string.Empty;
                        Point2f[] qrPoints = null;
                        using (Mat frame = BitmapSourceConverter.ToMat(currentBitmap))
                        using (Mat processingMat = frame.Clone())
                        {
                            (result, qrPoints) = await Task.Run(() =>
                            {
                                using QRCodeDetector qrDetector = new();
                                string decodedText = qrDetector.DetectAndDecode(processingMat, out Point2f[] points, null);

                                return (decodedText, points);
                            }, token);
                        }
                        if (!string.IsNullOrEmpty(result) && qrPoints != null && qrPoints.Length > 0)
                        {
                            double minX = qrPoints.Min(p => p.X);
                            double minY = qrPoints.Min(p => p.Y);
                            double maxX = qrPoints.Max(p => p.X);
                            double maxY = qrPoints.Max(p => p.Y);
                            double width = maxX - minX;
                            double height = maxY - minY;
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                DetectedQrCodes.Clear();
                                DetectedQrCodes.Add(new Detection
                                {
                                    Value = result,
                                    X = minX,
                                    Y = minY,
                                    Width = width,
                                    Height = height
                                });
                            });
                            AddStatusLine($"QR code content: {result}.");
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (DetectedQrCodes.Count > 0)
                                    DetectedQrCodes.Clear();
                            });
                        }
                        await Task.Delay(200, token);
                    }
                }
                else
                {
                    AddStatusLine("No Tasks.");
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                AddStatusLine($"Error: {ex.Message}");
            }
            finally
            {
                IsScanning = false;
            }
        }
        private  Task ExecuteVisionStopCommand()
        {
            try
            {
                if (IsScanning)
                {
                    _cancellationTokenSource?.Cancel();
                    IsScanning = false;
                    AddStatusLine("Task1 OFF.");
                }
            }
            catch (Exception ex)
            {
                AddStatusLine($"Stop Error: {ex.Message}");
            }
            return Task.CompletedTask;
        }
        private void AddStatusLine(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusLines.Add(message);
                while (StatusLines.Count > MaxLines)
                {
                    StatusLines.RemoveAt(0);
                }
            });
        }
        #endregion

        private readonly object Lock = new();
        private mavlink_heartbeat_t LatestHeartbeat;
        private mavlink_sys_status_t LatestSysStatus;
        private mavlink_vfr_hud_t LatestVfrHud;
        private mavlink_attitude_t LatestAttitude;
        private mavlink_scaled_pressure_t LatestWaterEnv;
        private readonly DispatcherTimer UITimer;
        /*
        private void OnVisionTaskPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VisionTask.Detections))
            {
                // Schedule asynchronously on UI thread, don’t block background thread:
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _detections.Clear();
                    foreach (var d in VisionTask.Detections)
                        _detections.Add(d);
                }));
            }
            else if (e.PropertyName == nameof(VisionTask.LatestScore))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LatestScore = VisionTask.LatestScore;
                }));
            }
        }
        
        private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Image1))
            {
                VisionTask.Image = Image1;
            }
            else if (e.PropertyName == nameof(PyFile))
            {
                VisionTask.PyFile = PyFile;
            }
        }
        */
        public MainViewModel()
        {
            #region Config
            UdpClient = new UdpClient();
            LocalIP = ConfigManager.Current.LocalIP;
            RemoteIP = ConfigManager.Current.RemoteIP;
            LocalPort = ConfigManager.Current.LocalPort;
            RemotePort = ConfigManager.Current.RemotePort;
            UserName = ConfigManager.Current.UserName;
            Password = ConfigManager.Current.Password;
            Cam1_Port = ConfigManager.Current.Cam1_Port;
            Cam2_Port = ConfigManager.Current.Cam2_Port;
            Cam3_Port = ConfigManager.Current.Cam3_Port;
            Vision_LocalPort = ConfigManager.Current.Vision_LocalPort;
            Vision_RemotePort = ConfigManager.Current.Vision_RemotePort;
            PyFile = ConfigManager.Current.PyFile;
            Task1_Path = ConfigManager.Current.Task1_Path;
            Task2_Path = ConfigManager.Current.Task2_Path;
            Task3_Path = ConfigManager.Current.Task3_Path;
            Task4_Path = ConfigManager.Current.Task4_Path;
            IsConnect = false;
            IsConnectButtonBusy = false;
            IsPower = false;
            IsPowerButtonBusy = false;
            IsEnable = false;
            IsEnableButtonBusy = false;
            IsStream = false;
            IsStreamButtonBusy = false;
            IsLight = false;
            IsLightButtonBusy = false;
            GripperCLS = false;
            GripperRT = false;
            SettingsWindow = new WindowService();
            Feed1 = new CAMStream(Cam1_Port);
            Feed2 = new CAMStream(Cam2_Port);
            Feed3 = new CAMStream(Cam3_Port);
            Image1 = Feed1?.Image;
            Image2 = Feed2?.Image;
            Image3 = Feed3?.Image;
            PIndex1 = 1;
            PIndex2 = 2;
            PIndex3 = 3;
            Feed_Comb =
            [
                "None",
                "Camera 1",
                "Camera 2",
                "Camera 3"
            ];
            JOYSTICK = new(RemoteIP, RemotePort);
            MavlinkHandler = new(RemoteIP, RemotePort, LocalPort);
            UITimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            UITimer.Tick += (s, e) => UpdateUi();
            HeartbeatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            HeartbeatTimer.Tick += Heartbeat_Tick;
            ConnectedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            ConnectedTimer.Tick += Connected_Tick;
            ConnectCommand = new RelayCommand(_ => ExecuteConnectCommand());
            PowerCommand = new RelayCommand(_ => ExecutePowerCommand());
            EnableCommand = new RelayCommand(_ => ExecuteEnableCommand());
            SettingsCommand = new RelayCommand(_ => ExecuteSettingsCommand());
            StreamCommand = new RelayCommand(_ => ExecuteStreamCommand());
            LightCommand = new RelayCommand(_ => ExecuteLightCommand());
            BrightnessCommand = new RelayCommand(_ => ExecuteBrightnessCommand());
            #endregion
            #region Vision
            DetectedQrCodes = [];
            VisionTask = new(LocalIP, Vision_RemotePort, Vision_LocalPort)
            {
                PyFile = PyFile
            };
            VisionTask.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(VisionTask.LatestScore))
                    LatestScore = VisionTask.LatestScore;
            };
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Image1))
                    VisionTask.Image = Image1;
                else if (e.PropertyName == nameof(PyFile))
                    VisionTask.PyFile = PyFile;
            };
            #endregion
            #region Stream
            
            Feed1.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Image")
                {
                    Image1 = Feed1.Image;
                }
            };
            Feed1.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Port")
                {
                    Cam1_Port = Feed1.Port;
                }
            };
            Feed2.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Image")
                {
                    Image2 = Feed2.Image;
                }
            };
            Feed2.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Port")
                {
                    Cam2_Port = Feed2.Port;
                }
            };
            Feed3.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Image")
                {
                    Image3 = Feed3.Image;
                }
            };
            Feed3.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Port")
                {
                    Cam3_Port = Feed3.Port;
                }
            };
            #endregion
            #region plot
            TubeTempModel = new PlotModel { Title = "Tube Temp", DefaultFontSize = 9, TitleFontSize = 9, TitleColor = OxyColors.White };
            TubePressureModel = new PlotModel { Title = "Tube Pressure", DefaultFontSize = 9, TitleFontSize = 9, TitleColor = OxyColor.Parse("#FFFFFFFF") };
            WaterTempModel = new PlotModel { Title = "Water Temp", DefaultFontSize = 9, TitleFontSize = 9, TitleColor = OxyColor.Parse("#FFFFFFFF") };
            WaterPressureModel = new PlotModel { Title = "Water Pressure", DefaultFontSize = 9, TitleFontSize = 9, TitleColor = OxyColor.Parse("#FFFFFFFF") };
            tubeTemp = new(TubeTempModel, "Temp (°C)");
            tubePressure = new(TubePressureModel, "Pressure (Pa)");
            waterTemp = new(WaterTempModel, "Temp (°C)");
            waterPressure = new(WaterPressureModel, "Pressure (Pa)");
            #endregion
            #region Joystick
            JOYSTICK.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "H_move")
                {
                    H_move = JOYSTICK.H_move;
                }
            };
            JOYSTICK.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "V_move")
                {
                    V_move = JOYSTICK.V_move;
                }
            };
            JOYSTICK.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "AddMark")
                {
                    //AddMark = JOYSTICK.AddMark;
                }
            };
            JOYSTICK.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "GripperCLS")
                {
                    GripperCLS = JOYSTICK.GripperCLS;
                }
            };
            JOYSTICK.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "GripperRT")
                {
                    GripperRT = JOYSTICK.GripperRT;
                }
            };
            #endregion 
            #region Mavlink
            MavlinkHandler.UpdateHearbeat += hb =>
            {
                lock (Lock)
                {
                    LatestHeartbeat = hb;
                }
                IsHeartbeatInd = true;
                HeartbeatTimer.Stop();
                HeartbeatTimer.Start();
            };
            MavlinkHandler.UpdateStatus += status =>
            {
                lock (Lock)
                {
                    StatusQueue.Enqueue(status);
                }
            };
            
            MavlinkHandler.UpdateSystemStatus += sysStatus =>
            {
                lock (Lock)
                {
                    LatestSysStatus = sysStatus;
                }
            };

            MavlinkHandler.UpdateVFR_HUD += vfr =>
            {
                lock (Lock)
                {
                    LatestVfrHud = vfr;
                }
            };
            MavlinkHandler.UpdateAttitude += attitude =>
            {
                lock (Lock)
                {
                    LatestAttitude = attitude;
                }
            };
            MavlinkHandler.UpdateWaterEnv += waterEnv =>
            {
                lock (Lock)
                {
                    LatestWaterEnv = waterEnv;
                }
            };
            //MavlinkHandler.UpdateTubeEnv += UpdateTubeEnv;
            MavlinkHandler.UpdatePID += updatepid =>
            {
                lock (Lock)
                {
                    UpdatePID(updatepid);
                }
            };
            #endregion
            #region 3D
            #endregion
        }
        private void UpdatePID(mavlink_named_value_float_t msg)
        {
            uint p = (uint)msg.value;
            SettingsWindow.settingsWVM.IsRollPID = (p & 0x01) != 0;
            SettingsWindow.settingsWVM.IsPitchPID = (p & 0x02) != 0;
            SettingsWindow.settingsWVM.IsDepthPID = (p & 0x04) != 0;
            IsRollPID = (p & 0x01) != 0;
            IsPitchPID = (p & 0x02) != 0;
            IsDepthPID = (p & 0x04) != 0;
        }
        private void UpdateUi()
        {
            mavlink_heartbeat_t hb;
            mavlink_sys_status_t ss;
            mavlink_vfr_hud_t vfr;
            mavlink_attitude_t att;
            mavlink_scaled_pressure_t waterEnv;
            string[] newStatuses;
            lock (Lock)
            {
                hb = LatestHeartbeat;
                newStatuses = [.. StatusQueue];
                ss = LatestSysStatus;
                att = LatestAttitude;
                vfr = LatestVfrHud;
                waterEnv = LatestWaterEnv;
            }
            if(hb.type != 0)
            {
                HeartBeatText =
                   $"\n  Type: {hb.type}\n" +
                   $"\n  Autopilot: {hb.autopilot}\n" +
                   $"\n  Mode: {hb.base_mode}\n" +
                   $"\n  Status: {hb.system_status}\n" +
                   $"\n  MAVLink Version: {hb.mavlink_version}";
            }
            Speed = (float)Math.Round(vfr.groundspeed, 2);
            Depth = vfr.alt;
            CompValue = vfr.heading;
            Roll = (float)Math.Round(att.roll, 2);
            Pitch = (float)Math.Round(att.pitch, 2);
            Yaw = (float)Math.Round(att.yaw, 2);
            Water_Temp = waterEnv.temperature;
            Water_Press = waterEnv.press_abs;
            while (StatusQueue.TryDequeue(out var raw))
            {
                var ts = $"[{DateTime.Now:HH:mm:ss}] {raw}";
                StatusLines.Add(ts);
                while (StatusLines.Count > MaxLines)
                    StatusLines.RemoveAt(0);
            }
            uint present = ss.onboard_control_sensors_present;
            IsMPUInd = (present & 0x01) != 0;
            IsHMCInd = (present & 0x02) != 0;
            IsMSCInd = (present & 0x04) != 0;
            IsLightInd = (present & 0x08) != 0;
            IsGripperClosedInd = (present & 0x10) != 0;
            IsGripperRotateInd = (present & 0x20) != 0;
            IsEnable = (present & 0x40) != 0;
            T0_PWM = ss.load;
            T1_PWM = ss.drop_rate_comm;
            T2_PWM = ss.errors_comm;
            T3_PWM = ss.errors_count1;
            T4_PWM = ss.errors_count2;
            T5_PWM = ss.errors_count3;
            T6_PWM = ss.errors_count4;
        }
        private void ResetUi()
        {
            Water_Temp = 0;
            Tube_Temp = 0;
            Water_Press = 0;
            Tube_Press = 0;
            Depth = 0;
            Speed = 0;
            CompValue = 0;
            Roll = 0;
            Pitch = 0;
            Yaw = 0;
            //StatusLines.Clear();
            IsMPUInd = false;
            IsHMCInd = false;
            IsMSCInd = false;
            IsLightInd = false;
            IsGripperClosedInd = false;
            IsGripperRotateInd = false;
            T0_PWM = 0;
            T1_PWM = 0;
            T2_PWM = 0;
            T3_PWM = 0;
            T4_PWM = 0;
            T5_PWM = 0;
            T6_PWM = 0;
            LatestHeartbeat = new mavlink_heartbeat_t();
            LatestSysStatus = new mavlink_sys_status_t();
            LatestAttitude = new mavlink_attitude_t();
            LatestVfrHud = new mavlink_vfr_hud_t();
            LatestWaterEnv = new mavlink_scaled_pressure_t();
            HeartBeatText = "";
        }
        public new event PropertyChangedEventHandler PropertyChanged;
        protected new void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public void Dispose()
        {
            Feed1?.Dispose();
            Feed2?.Dispose();
            Feed3?.Dispose();
            JOYSTICK?.Dispose();
            MavlinkHandler?.Dispose();
            VisionTask?.Dispose();
            SettingsWindow.Dispose();
        }
    }
}