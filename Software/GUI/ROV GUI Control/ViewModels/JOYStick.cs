using System;
using System.Net;
using static MAVLink;
using System.Threading;
using System.Net.Sockets;
using SharpDX.DirectInput;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.ViewModels
{
    public class JOYStick : INotifyPropertyChanged, IDisposable
    {
        private readonly byte systemId = 1;
        private readonly byte componentId = 1;
        private readonly MavlinkParse mavlinkParser = new();
        private UdpClient _udpClient;
        private IPEndPoint _remoteEP;
        private readonly SemaphoreSlim _networkLock = new(1, 1);

        private DirectInput _directInput;
        private Joystick _joystick;

        private readonly CancellationTokenSource _loopCTS;
        private readonly int _updateIntervalMs = 30;

        private float _nextX, _nextY, _nextZ, _nextRz;
        private bool _isMoving;

        #region Notify Properties
        private int _vmove = -1;
        public int V_move { get => _vmove; set { _vmove = value; OnPropertyChanged(); } }

        private int _hmove = -1;
        public int H_move { get => _hmove; set { _hmove = value; OnPropertyChanged(); } }

        private bool _gripperCLS;
        public bool GripperCLS { get => _gripperCLS; set { if (_gripperCLS != value) { _gripperCLS = value; OnPropertyChanged(); } } }

        private bool _gripperRT;
        public bool GripperRT { get => _gripperRT; set { if (_gripperRT != value) { _gripperRT = value; OnPropertyChanged(); } } }
        #endregion
        private readonly int[,,] BasicMov;

        public JOYStick(string remoteIp, int remotePort)
        {
            BasicMov = new int[3, 3, 3]
            {
                {
                    { 5, 5, 5 },
                    { 3, 3, 3 },
                    { 7, 7, 7 }
                },
                {
                    { 0, 0, 0 },
                    { 9,-1, 8 },
                    { 1, 1, 1 }
                },
                {
                    { 4, 4, 4 },
                    { 2, 2, 2 },
                    { 6, 6, 6 }
                }
            };
            _loopCTS = new CancellationTokenSource();
            Connect(remoteIp, remotePort);
            _ = Task.Run(() => DiscoveryLoop(_loopCTS.Token));
        }

        public void Connect(string remoteIp, int remotePort)
        {
            _networkLock.Wait();
            try
            {
                _udpClient?.Close();
                _udpClient?.Dispose();
                _udpClient = new UdpClient();
                _remoteEP = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
            }
            catch (Exception) {}
            finally { _networkLock.Release(); }
        }
        public async Task Disconnect()
        {
            await _networkLock.WaitAsync();
            try
            {
                if (_udpClient != null)
                {
                    _udpClient.Close();
                    _udpClient.Dispose();
                    _udpClient = null;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _networkLock.Release();
            }
        }
        private async Task DiscoveryLoop(CancellationToken token)
        {
            _directInput = new DirectInput();
            while (!token.IsCancellationRequested)
            {
                var devices = _directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly);
                if (devices.Count > 0)
                {
                    _joystick = new Joystick(_directInput, devices[0].InstanceGuid);
                    _joystick.Acquire();
                    await CommunicationLoop(token);
                }
                await Task.Delay(1000, token);
            }
        }
        private async Task CommunicationLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    PollJoystick();
                    await SendCurrentStateAsync();
                }
                catch (Exception) { break; }

                await Task.Delay(_updateIntervalMs, token);
            }
        }
        private void PollJoystick()
        {
            if (_joystick == null) return;
            _joystick.Poll();
            var state = _joystick.GetCurrentState();

            float x = Normalize(state.X);
            float y = Normalize(state.Y);
            float z = Normalize(state.Z);
            float rz = Normalize(state.RotationZ);

            if (Math.Abs(x) > 0.05f || Math.Abs(y) > 0.05f || Math.Abs(z) > 0.05f || Math.Abs(rz) > 0.05f)
            {
                _nextX = x; _nextY = y; _nextZ = z; _nextRz = rz;
                _isMoving = true;
            }
            else if (_isMoving)
            {
                H_move = -1;
                V_move = -1;
                _nextX = 0; _nextY = 0; _nextZ = 0; _nextRz = 0;
                _isMoving = false;
            }
            GripperCLS = state.Buttons[5];
            GripperRT = state.Buttons[4];
            x = (Math.Abs(x) < 0.05) ? 0 : x;
            y = (Math.Abs(y) < 0.05) ? 0 : y;
            z = (Math.Abs(z) < 0.05) ? 0 : z;
            rz = (Math.Abs(rz) < 0.05) ? 0 : rz;
            int xn = (int)RoundAwayFromZero(x) + 1;
            int yn = (int)RoundAwayFromZero(y) + 1;
            int zn = (int)RoundAwayFromZero(z) + 1;
            H_move = BasicMov[xn, yn, zn];
            if (Math.Abs(rz) > 0.01)
                V_move = (rz > 0) ? 11 : 10;
            else
                V_move = -1;
        }
        static int RoundAwayFromZero(float num)
        {
            if (num == 0)
                return 0;
            return (int)(num > 0 ? Math.Ceiling(num) : Math.Floor(num));
        }
        private async Task SendCurrentStateAsync()
        {
            var msg = new mavlink_set_position_target_local_ned_t
            {
                vx = - _nextX,
                vy = - _nextY,
                vz = _nextRz,
                yaw = (Math.Abs(_nextX) > 0.05f || (Math.Abs(_nextY) > 0.05f)) ? 0 : _nextZ
            };
            byte[] packet = mavlinkParser.GenerateMAVLinkPacket20(MAVLINK_MSG_ID.SET_POSITION_TARGET_LOCAL_NED, msg, false, systemId, componentId);
            await _networkLock.WaitAsync();
            try
            {
                if (_udpClient != null)
                    await _udpClient.SendAsync(packet, packet.Length, _remoteEP);
            }
            catch (ObjectDisposedException) { /* Clean shutdown */ }
            catch (Exception) {}
            finally { _networkLock.Release(); }
        }
        private float Normalize(int value) => (value / 65535.0f) * 2 - 1;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Dispose()
        {
            _loopCTS?.Cancel();
            _networkLock.Wait();
            _udpClient?.Close();
            _joystick?.Unacquire();
            _joystick?.Dispose();
            _directInput?.Dispose();
            _networkLock.Release();
        }
    }
}