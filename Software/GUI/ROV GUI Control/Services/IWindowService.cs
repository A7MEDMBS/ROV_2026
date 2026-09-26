namespace ROV_GUI_Control
{
    public interface IWindowService
    {
        (string, string, string, string, int, int, int, int, int, int, int, string, string, string, string, string, bool, bool, bool) ShowWindow(string localIP, string remoteIp, string username, string password, int localPort, int remotePort, int cam1_port, int cam2_port, int cam3_port, int vision_localPort, int vision_remotePort, string pyfile, string task1_Path, string task2_Path, string task3_Path, string task4_Path, bool is_connect, bool is_RollPID, bool is_PitchPID, bool is_DepthPID);
    }
}
