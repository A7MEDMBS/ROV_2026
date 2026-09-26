using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.VisionTask
{
    public class Detection : INotifyPropertyChanged
    {
        public string Value { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        /*private int _x;
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

        private int _y;
        public int y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(y)));
                }
            }
        }

        private int _w;
        public int w
        {
            get => _w;
            set
            {
                if (_w != value)
                {
                    _w = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(w)));
                }
            }
        }

        private int _h;
        public int h
        {
            get => _h;
            set
            {
                if (_h != value)
                {
                    _h = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(h)));
                }
            }
        }*/
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}