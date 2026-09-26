using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.VisionTask
{
    public class Score : INotifyPropertyChanged
    {
        private int _c;
        public int c
        {
            get => _c;
            set
            {
                if (_c != value)
                {
                    _c = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(c)));
                }
            }
        }
        private int _r;
        public int r
        {
            get => _r;
            set
            {
                if (_r != value)
                {
                    _r = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(r)));
                }
            }
        }
        private int _t;
        public int t
        {
            get => _t;
            set
            {
                if (_t != value)
                {
                    _t = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(t)));
                }
            }
        }
        private int _x;
        public int x
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(x)));
                }
            }
        }
        private int _s;
        public int s
        {
            get => _s;
            set
            {
                if (_s != value)
                {
                    _s = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(s)));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
