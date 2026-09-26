using System;
using System.Windows;

namespace ROV_GUI_Control.Services
{
    public class WindowService : IWindowService, IDisposable
    {
        public SettingsWindowViewModel settingsWVM;
        public Settings settings;
        public (string, string, string, string, int, int, int, int, int, int, int, string, string, string, string, string, bool, bool, bool) ShowWindow(string localIP, string remoteIp, string username, string password, int localPort, int remotePort, int cam1_port, int cam2_port, int cam3_port, int vision_localPort, int vision_remotePort, string pyfile, string task1_Path, string task2_Path, string task3_Path, string task4_Path, bool is_connect, bool is_RollPID, bool is_PitchPID, bool is_DepthPID)
        {
            settingsWVM = new SettingsWindowViewModel()
            {
                LocalIP = localIP,
                RemoteIP = remoteIp,
                UserName = username,
                Password = password,
                LocalPort = localPort,
                RemotePort = remotePort,
                Cam1_Port = cam1_port,
                Cam2_Port = cam2_port,
                Cam3_Port = cam3_port,
                Vision_LocalPort = vision_localPort,
                Vision_RemotePort = vision_remotePort,
                IsConnect = is_connect,
                PyFile = pyfile,
                Task1_Path = task1_Path,
                Task2_Path = task2_Path,
                Task3_Path = task3_Path,
                Task4_Path = task4_Path,
                IsRollPID = is_RollPID,
                IsPitchPID = is_PitchPID,
                IsDepthPID = is_DepthPID
            };
            settings = new Settings
            {
                DataContext = settingsWVM
            };
            settingsWVM.ExecuteSetCommand(2, 3);
            ModalResult result = ModalResult.Cancel;
            settingsWVM.CloseAction = modalResult =>
            {
                result = modalResult;
                if (modalResult == ModalResult.Warning)
                    MessageBox.Show("Used Port !");
                else
                    settings.DialogResult = true;
            };
            bool? closed = settings.ShowDialog();
            if (result == ModalResult.Ok)
                return (settingsWVM.LocalIP, settingsWVM.RemoteIP, settingsWVM.UserName, settingsWVM.Password, settingsWVM.LocalPort, settingsWVM.RemotePort, settingsWVM.Cam1_Port, settingsWVM.Cam2_Port, settingsWVM.Cam3_Port, settingsWVM.Vision_LocalPort, settingsWVM.Vision_RemotePort, settingsWVM.PyFile, settingsWVM.Task1_Path, settingsWVM.Task2_Path, settingsWVM.Task3_Path, settingsWVM.Task4_Path, settingsWVM.IsRollPID, settingsWVM.IsPitchPID, settingsWVM.IsDepthPID);
            else
                return (localIP, remoteIp, username, password, localPort, remotePort, cam1_port, cam2_port, cam3_port, vision_localPort, vision_remotePort, pyfile, task1_Path, task2_Path, task3_Path, task4_Path, is_RollPID, is_PitchPID, is_DepthPID);
        }
        public void Dispose()
        {
            settingsWVM?.Dispose();
        }
    }
}
