using System;
using System.Net;
using static MAVLink;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace ROV_GUI_Control.ViewModels
{
    public class MAVLinkHandler : IDisposable
    {
        private  UdpClient UDPClient;
        private  IPEndPoint RemoteEP;
        private static readonly MavlinkParse mavlinkParser = new();
        private bool UdpClientDisposed = false;
        private static string RemoteIP { get; set; }
        private static int RemotePort { get; set; }
        private static int LocalPort { get; set; }
        public event Action<string> UpdateStatus;
        public event Action<mavlink_vfr_hud_t> UpdateVFR_HUD;
        public event Action<mavlink_attitude_t> UpdateAttitude;
        public event Action<mavlink_heartbeat_t> UpdateHearbeat;
        public event Action<mavlink_sys_status_t> UpdateSystemStatus;
        public event Action<mavlink_scaled_pressure_t> UpdateWaterEnv;
        public event Action<mavlink_scaled_pressure2_t> UpdateTubeEnv;
        public event Action<mavlink_named_value_float_t> UpdatePID;
        public MAVLinkHandler(string remoteIp, int remotePort, int localPort = 14550)
        {
            RemoteIP = remoteIp;
            RemotePort = remotePort;
            LocalPort = localPort;
        }
        public void Connect(string remoteIp, int remotePort, int localPort = 14550)
        {
            try
            {
                RemoteIP = remoteIp;
                RemotePort = remotePort;
                LocalPort = localPort;

                UdpClientDisposed = false;
                if (UDPClient != null)
                {
                    UDPClient.Close();
                    UDPClient.Dispose();
                }
                UDPClient = new UdpClient(LocalPort);
                RemoteEP = new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort);
                UDPClient.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception )
            {
            }
        }
        public void Disconnect()
        {
            UdpClientDisposed = true; // Tell the callback to stop RE-STARTING

            try
            {
                UDPClient?.Close(); // This will trigger the final ReceiveCallback
                UDPClient?.Dispose();
            }
            finally
            {
                UDPClient = null;
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (UdpClientDisposed || UDPClient == null) return;
            try
            {
                IPEndPoint remoteEP = new(IPAddress.Any, 0);
                byte[] receivedBytes = UDPClient?.EndReceive(ar, ref remoteEP);

                if (receivedBytes != null && receivedBytes.Length > 0)
                {
                    foreach (byte b in receivedBytes)
                    {
                        var packet = mavlinkParser.ReadByte(b);

                        if (packet != null)
                        {
                            Task.Run(() => MessageProcessing(packet));
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // This is expected when the socket closes; ignore it.
                return;
            }
            catch (Exception)
            {
            }
            finally
            {
                // 2. Double-check the flag before restarting
                try
                {
                    if (!UdpClientDisposed && UDPClient != null)
                    {
                        UDPClient.BeginReceive(ReceiveCallback, null);
                    }
                }
                catch (ObjectDisposedException) { /* Socket closed during finally */ }
                catch (Exception) { /* Handle or log other restart errors */ }
            }
        }
        private Task MessageProcessing(MAVLinkMessage message)
        {
            if (message != null)
            {
                try
                {
                    switch (message.msgid)
                    {
                        case (uint)MAVLINK_MSG_ID.HEARTBEAT:
                            var heartbeat = message.ToStructure<mavlink_heartbeat_t>();
                            UpdateHearbeat?.Invoke(heartbeat);
                            break;
                        case (uint)MAVLINK_MSG_ID.STATUSTEXT:
                            mavlink_statustext_t statusText = message.ToStructure<mavlink_statustext_t>();
                            string receivedMessage = System.Text.Encoding.ASCII.GetString(statusText.text).TrimEnd('\0');
                            UpdateStatus?.Invoke(receivedMessage);
                            break;
                        case (uint)MAVLINK_MSG_ID.SYS_STATUS:
                            var sysStatus = message.ToStructure<mavlink_sys_status_t>();
                            UpdateSystemStatus?.Invoke(sysStatus);
                            break;
                        case (uint)MAVLINK_MSG_ID.VFR_HUD:
                            var vfrHud = message.ToStructure<mavlink_vfr_hud_t>();
                            UpdateVFR_HUD?.Invoke(vfrHud);
                            break;
                        case (uint)MAVLINK_MSG_ID.ATTITUDE:
                            var attitude = message.ToStructure<mavlink_attitude_t>();
                            UpdateAttitude?.Invoke(attitude);
                            break;
                        case (uint)MAVLINK_MSG_ID.SCALED_PRESSURE:
                            var pres_temp_w = message.ToStructure<mavlink_scaled_pressure_t>();
                            UpdateWaterEnv?.Invoke(pres_temp_w);
                            break;
                        case (uint)MAVLINK_MSG_ID.SCALED_PRESSURE2:
                            var pres_temp_t = message.ToStructure<mavlink_scaled_pressure2_t>();
                            UpdateTubeEnv?.Invoke(pres_temp_t);
                            break;
                        case (uint)MAVLINK_MSG_ID.NAMED_VALUE_FLOAT:
                            var pid = message.ToStructure<mavlink_named_value_float_t>();
                            UpdatePID?.Invoke(pid);
                            break;
                        default:
                            Console.WriteLine($"Unknown Packet ID: {message.msgid}");
                            break;
                    }
                }
                catch 
                {
                }
            }
            return Task.CompletedTask;
        }
        public async Task SendCommand(byte[] packet)
        {
            if (packet == null || packet.Length == 0)
                return;

            if (UDPClient == null || UdpClientDisposed || RemoteEP == null)
                return;
            try
            {
                int bytesSent = await UDPClient.SendAsync(packet, packet.Length, RemoteEP);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (SocketException )
            {
            }
            catch (Exception )
            {
                
            }
        }
        public void Dispose()
        {
            UDPClient?.Close();
            UDPClient?.Dispose();
            UdpClientDisposed = true;
        }
    }
}
