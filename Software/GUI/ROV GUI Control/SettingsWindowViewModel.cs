using System;
using System.Net;
using System.Linq;
using System.Windows;
using static MAVLink;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ROV_GUI_Control.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ROV_GUI_Control
{
    public class SettingsWindowViewModel : ObservableObject, IDisposable
    {
        private readonly byte SystemID = 1;
        private readonly byte ComponentID = 1;
        private static readonly MavlinkParse mavlinkParser = new();
        private readonly UdpClient UDPClient; 
        
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
                    OkCommand?.NotifyCanExecuteChanged();
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
                    OkCommand?.NotifyCanExecuteChanged();
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
                    OkCommand?.NotifyCanExecuteChanged();
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
                    OkCommand?.NotifyCanExecuteChanged();
                }
            }
        }
        private string _username;
        public string UserName
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(UserName));
                    OkCommand?.NotifyCanExecuteChanged();
                }
            }
        }
        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                    OkCommand?.NotifyCanExecuteChanged();
                }
            }
        }
        private int _cam1_Port;
        public int Cam1_Port
        {
            get => _cam1_Port;
            set
            {
                _cam1_Port = value;
                OnPropertyChanged(nameof(Cam1_Port));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
       
        private int _cam2_Port;
        public int Cam2_Port
        {
            get => _cam2_Port;
            set
            {
                _cam2_Port = value;
                OnPropertyChanged(nameof(Cam2_Port));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
        
        private int _cam3_Port;
        public int Cam3_Port
        {
            get => _cam3_Port;
            set
            {
                _cam3_Port = value;
                OnPropertyChanged(nameof(Cam3_Port));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
        private int _vision_localPort;
        public int Vision_LocalPort
        {
            get => _vision_localPort;
            set
            {
                _vision_localPort = value;
                OnPropertyChanged(nameof(Vision_LocalPort));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
        private int _vision_remotePort;
        public int Vision_RemotePort
        {
            get => _vision_remotePort;
            set
            {
                _vision_remotePort = value;
                OnPropertyChanged(nameof(Vision_RemotePort));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
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
        public bool IsSetButtonBusy { get; set; }
        private string _pyfile;
        public string PyFile
        {
            get => _pyfile;
            set
            {
                if (_pyfile != value)
                {
                    _pyfile = value;
                    OnPropertyChanged(nameof(PyFile));
                    OkCommand?.NotifyCanExecuteChanged();
                }
            }
        }
        private string _task1_Path;
        public string Task1_Path
        {
            get => _task1_Path;
            set
            {
                if (_task1_Path != value)
                {
                    _task1_Path = value;
                    OnPropertyChanged(nameof(Task1_Path));
                    OkCommand?.NotifyCanExecuteChanged();
                }
            }
        }
        private string _task2_Path;
        public string Task2_Path
        {
            get => _task2_Path;
            set
            {
                if (_task2_Path != value)
                {
                    _task2_Path = value;
                    OnPropertyChanged(nameof(Task2_Path));
                    OkCommand?.NotifyCanExecuteChanged();
                }
            }
        }
        private string _task3_Path;
        public string Task3_Path
        {
            get => _task3_Path;
            set
            {
                if (_task3_Path != value)
                {
                    _task3_Path = value;
                    OnPropertyChanged(nameof(Task3_Path));
                    OkCommand?.NotifyCanExecuteChanged();
                }
            }
        }
        private string _task4_Path;
        public string Task4_Path
        {
            get => _task4_Path;
            set
            {
                if (_task4_Path != value)
                {
                    _task4_Path = value;
                    OnPropertyChanged(nameof(Task4_Path));
                    OkCommand?.NotifyCanExecuteChanged();
                }
            }
        }
        private PIDConfig _rollPID;
        public PIDConfig RollPID
        {
            get => _rollPID;
            set
            {
                _rollPID = value;
                OnPropertyChanged(nameof(RollPID));             
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
        private PIDConfig _pitchPID;
        public PIDConfig PitchPID
        {
            get => _pitchPID;
            set
            {
                _pitchPID = value;
                OnPropertyChanged(nameof(PitchPID));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
        private PIDConfig _depthPID;
        public PIDConfig DepthPID
        {
            get => _depthPID;
            set
            {
                _depthPID = value;
                OnPropertyChanged(nameof(DepthPID));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
        private float _atmosphericPressure;
        public float AtmosphericPressure
        {
            get => _atmosphericPressure;
            set
            {
                _atmosphericPressure = value;
                OnPropertyChanged(nameof(AtmosphericPressure));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
        private float _rollOffset = 0 ;
        public float RollOFFSET
        {
            get => _rollOffset;
            set
            {
                _rollOffset = value;
                OnPropertyChanged(nameof(RollOFFSET));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
        private float _pitchOffset = 0;
        public float PitchOFFSET
        {
            get => _pitchOffset;
            set
            {
                _pitchOffset = value;
                OnPropertyChanged(nameof(PitchOFFSET));
                OkCommand?.NotifyCanExecuteChanged();
            }
        }
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
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand<string> SetCommand { get; }
        public RelayCommand<string> BrowseCommand { get; }
        public Action<ModalResult> CloseAction { get; set; }
        public async void ExecuteSetCommand(int type, int ind)
        {
            if (IsSetButtonBusy) return;
            if (!IsConnect)
            {
                MessageBox.Show("No Connection.", "Connection Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsSetButtonBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    var command = new MAVLink.mavlink_command_long_t
                    {
                        target_system = 1,
                        target_component = 1,
                        confirmation = (byte)type,
                        command = (ushort)MAV_CMD.DO_SET_PARAMETER,
                        param1 = ind
                    };
                    if (type == 1)
                    {
                        Console.WriteLine(command.confirmation);
                        switch (ind)
                        {
                            case 0:
                                command.param2 = RollPID.Kp;
                                command.param3 = RollPID.Ki;
                                command.param4 = RollPID.Kd;
                                command.param5 = RollPID.Alpha;
                                command.param6 = RollPID.AntiWindup;
                                break;
                            case 1:
                                command.param2 = PitchPID.Kp;
                                command.param3 = PitchPID.Ki;
                                command.param4 = PitchPID.Kd;
                                command.param5 = PitchPID.Alpha;
                                command.param6 = PitchPID.AntiWindup;
                                break;
                            case 2:
                                command.param2 = DepthPID.Kp;
                                command.param3 = DepthPID.Ki;
                                command.param4 = DepthPID.Kd;
                                command.param5 = DepthPID.Alpha;
                                command.param6 = DepthPID.AntiWindup;
                                break;
                            case 3:
                                command.param2 = AtmosphericPressure;
                                break;
                            case 4:
                                command.param2 = RollOFFSET;
                                break;
                            case 5:
                                command.param2 = PitchOFFSET;
                                break;
                        }
                    }
                    var packet = mavlinkParser.GenerateMAVLinkPacket20(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, command, false, SystemID, ComponentID);
                    UDPClient.Send(packet, packet.Length, new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort));
                });
            }
            catch
            {
            }
            finally
            {
                IsSetButtonBusy = false;
            }
        }
        private void BrowseFile(int path)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a file",
                Filter = "All files (*.*)|*.*" 
            };
            if (dialog.ShowDialog() == true)
            {
                switch(path)
                {
                    case 0:
                        PyFile = dialog.FileName;
                        break;
                    case 1:
                        Task1_Path = dialog.FileName;
                        break;
                    case 2:
                        Task2_Path = dialog.FileName;
                        break;
                    case 3:
                        Task3_Path = dialog.FileName;
                        break;
                    case 4:
                        Task4_Path = dialog.FileName;
                        break;
                }
                
            }
        }
        public ModalResult Check_ports()
        {
            ConfigManager.Current.RollPID = RollPID;
            ConfigManager.Current.PitchPID = PitchPID;
            ConfigManager.Current.DepthPID = DepthPID;
            ConfigManager.Current.AtmosphericPressure = AtmosphericPressure;
            ConfigManager.Current.RollOFFSET = RollOFFSET;
            ConfigManager.Current.PitchOFFSET = PitchOFFSET;
            ConfigManager.Save();
            var ports = new[] { LocalPort, RemotePort, Cam1_Port, Cam2_Port, Cam3_Port, Vision_LocalPort, Vision_RemotePort};
            return (ports.Distinct().Count() == ports.Length) ? ModalResult.Ok: ModalResult.Warning;
        }
        public SettingsWindowViewModel()
        {
            RollPID = new PIDConfig
            {
                Kp = ConfigManager.Current.RollPID.Kp,
                Ki = ConfigManager.Current.RollPID.Ki,
                Kd = ConfigManager.Current.RollPID.Kd,
                Alpha = ConfigManager.Current.RollPID.Alpha,
                AntiWindup = ConfigManager.Current.RollPID.AntiWindup
            };
            PitchPID = new PIDConfig
            {
                Kp = ConfigManager.Current.PitchPID.Kp,
                Ki = ConfigManager.Current.PitchPID.Ki,
                Kd = ConfigManager.Current.PitchPID.Kd,
                Alpha = ConfigManager.Current.PitchPID.Alpha,
                AntiWindup = ConfigManager.Current.PitchPID.AntiWindup
            };
            DepthPID = new PIDConfig
            {
                Kp = ConfigManager.Current.DepthPID.Kp,
                Ki = ConfigManager.Current.DepthPID.Ki,
                Kd = ConfigManager.Current.DepthPID.Kd,
                Alpha = ConfigManager.Current.DepthPID.Alpha,
                AntiWindup = ConfigManager.Current.DepthPID.AntiWindup
            };
            AtmosphericPressure = ConfigManager.Current.AtmosphericPressure;
            RollOFFSET = ConfigManager.Current.RollOFFSET;
            PitchOFFSET = ConfigManager.Current.PitchOFFSET;
            IsSetButtonBusy = false;
            UDPClient = new UdpClient();
            OkCommand = new RelayCommand(
                () => CloseAction?.Invoke(Check_ports())
            );
            CancelCommand = new RelayCommand(() => CloseAction?.Invoke(ModalResult.Cancel));
            SetCommand = new RelayCommand<string>(param =>
            {
                var parts = param.Split(',');
                ExecuteSetCommand(int.Parse(parts[0]), int.Parse(parts[1]));
            });
            BrowseCommand = new RelayCommand<string>(param => BrowseFile(int.Parse(param)));
        }
        public void Dispose()
        {
            UDPClient?.Close();
            UDPClient?.Dispose();
        }
    }
}
