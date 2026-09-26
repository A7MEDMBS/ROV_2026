using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.Configuration
{
    public class AppConfig
    {
        public string LocalIP { get; set; } = "192.168.1.9";
        public string RemoteIP { get; set; } = "192.168.1.6";
        public string UserName { get; set; } = "rov";
        public string Password { get; set; } = "rov2025";
        public int LocalPort { get; set; } = 14550;
        public int RemotePort { get; set; } = 14500;
        public int Cam1_Port { get; set; } = 5000;
        public int Cam2_Port { get; set; } = 6000;
        public int Cam3_Port { get; set; } = 7000;
        public int Vision_LocalPort { get; set; } = 15000;
        public int Vision_RemotePort { get; set; } = 16000;
        public string PyFile { get; set; }  = "C:\\python312\\python3.exe";
        public string Task1_Path { get; set; }  = "C:\\python312\\Task_1.py";
        public string Task2_Path { get; set; }  = "C:\\python312\\Task_1.py";
        public string Task3_Path { get; set; }  = "C:\\python312\\Task_1.py";
        public string Task4_Path { get; set; }  = "C:\\python312\\Task_1.py";
        public PIDConfig RollPID { get; set; } = new PIDConfig();
        public PIDConfig PitchPID { get; set; } = new PIDConfig();
        public PIDConfig DepthPID { get; set; } = new PIDConfig();
        public float AtmosphericPressure { get; set; } = 1016.0f;
        public float RollOFFSET { get; set; } = 0.0f;
        public float PitchOFFSET { get; set; } = 0.0f;
    }
    public class PIDConfig : INotifyPropertyChanged
    {
        private float _kp;
        public float Kp
        {
            get => _kp;
            set { _kp = value; OnPropertyChanged(); }
        }

        private float _ki;
        public float Ki
        {
            get => _ki;
            set { _ki = value; OnPropertyChanged(); }
        }

        private float _kd;
        public float Kd
        {
            get => _kd;
            set { _kd = value; OnPropertyChanged(); }
        }

        private float _alpha;
        public float Alpha
        {
            get => _alpha;
            set { _alpha = value; OnPropertyChanged(); }
        }

        private float _antiWindup;
        public float AntiWindup
        {
            get => _antiWindup;
            set { _antiWindup = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
