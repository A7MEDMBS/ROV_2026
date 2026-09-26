using System;
using System.Windows;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.View
{
    /// <summary>
    /// Interaction logic for Movement.xaml
    /// </summary>
    public partial class Movement : UserControl, INotifyPropertyChanged, IDisposable
    {
        public static readonly DependencyProperty H_moveProperty = DependencyProperty.Register(nameof(H_move), typeof(int), typeof(Movement),
            new PropertyMetadata(0, OnHDirValuePropertyChanged));
        public int H_move
        {
            get { return (int)GetValue(H_moveProperty); }
            set { SetValue(H_moveProperty, value); }
        }
        private static void OnHDirValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Movement)d;
            control.HMoveBlink((int)e.NewValue);
        }
        
        public static readonly DependencyProperty V_moveProperty = DependencyProperty.Register(nameof(V_move), typeof(int), typeof(Movement),
            new PropertyMetadata(0, OnVDirValuePropertyChanged));
        public int V_move
        {
            get { return (int)GetValue(V_moveProperty); }
            set { SetValue(V_moveProperty, value); }
        }
        private static void OnVDirValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Movement)d;
            control.VMoveBlink((int)e.NewValue);
        }
        private readonly System.Timers.Timer HTimer;
        private readonly System.Timers.Timer VTimer;

        private Path _hpoly;
        public Path HPOLY
        {
            get => _hpoly;
            set
            {
                _hpoly = value;
                OnPropertyChanged(nameof(HPOLY));
            }
        }
        private Path _vpoly;
        public Path VPOLY
        {
            get => _vpoly;
            set
            {
                _vpoly = value;
                OnPropertyChanged(nameof(VPOLY));
            }
        }
        private bool _ishblink = false;
        public bool IsHBlink
        {
            get => _ishblink;
            set
            {
                _ishblink = value;
                OnPropertyChanged(nameof(IsHBlink));
            }
        }
        private bool _isvblink = false;
        public bool IsVBlink
        {
            get => _isvblink;
            set
            {
                _isvblink = value;
                OnPropertyChanged(nameof(IsVBlink));
            }
        }
        public Movement()
        {
            InitializeComponent();
            HTimer = new System.Timers.Timer(200);
            HTimer.Elapsed += HTimer_Tick;
            HTimer.AutoReset = true;
            VTimer = new System.Timers.Timer(200);
            VTimer.Elapsed += VTimer_Tick;
            VTimer.AutoReset = true;
            HiddenAll();
            Up.Visibility = Visibility.Hidden;
            Down.Visibility = Visibility.Hidden;
            Ushadow.Visibility = Visibility.Hidden;
            Dshadow.Visibility = Visibility.Hidden;
        }
        private void HiddenAll()
        {
            Forward.Visibility = Visibility.Hidden;
            Backward.Visibility = Visibility.Hidden;
            Right.Visibility = Visibility.Hidden;
            Left.Visibility = Visibility.Hidden;
            ForwardR.Visibility = Visibility.Hidden;
            ForwardL.Visibility = Visibility.Hidden;
            BackwardR.Visibility = Visibility.Hidden;
            BackwardL.Visibility = Visibility.Hidden;
            RotateR.Visibility = Visibility.Hidden;
            RotateL.Visibility = Visibility.Hidden;
            RRshadow.Visibility = Visibility.Hidden;
            RLshadow.Visibility = Visibility.Hidden;
        }
        private void HTimer_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (HPOLY != null)
                {
                    HPOLY.Visibility = IsHBlink ? Visibility.Visible : Visibility.Hidden;
                    IsHBlink = !IsHBlink;
                }
            });
        }
        private void VTimer_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (VPOLY != null)
                {
                    VPOLY.Visibility = IsVBlink ? Visibility.Visible : Visibility.Hidden;
                    IsVBlink = !IsVBlink;
                }
            });
        }
       
        private void HMoveBlink(int i)
        {
            switch(i)
            {
                case 0:
                    HBlink(Forward);
                    break;
                case 1:
                    HBlink(Backward);
                    break;
                case 2:
                    HBlink(Right);
                    break;
                case 3:
                    HBlink(Left);
                    break;
                case 4:
                    HBlink(ForwardR);
                    break;
                case 5:
                    HBlink(ForwardL);
                    break;
                case 6:
                    HBlink(BackwardR);
                    break;
                case 7:
                    HBlink(BackwardL);
                    break;
                case 8:
                    HBlink(RotateR);
                    break;
                case 9:
                    HBlink(RotateL);
                    break;
                default:
                    HTimer.Stop();
                    HiddenAll();
                    break;
            }
        }
        private void VMoveBlink(int i)
        {
            switch (i)
            {
                case 10:
                    VBlink(Up);
                    break;
                case 11:
                    VBlink(Down);
                    break;
                default:
                    VTimer.Stop();
                    Up.Visibility = Visibility.Hidden;
                    Down.Visibility = Visibility.Hidden;
                    Ushadow.Visibility = Visibility.Hidden;
                    Dshadow.Visibility = Visibility.Hidden;
                    break;
            }
        }
        private void HBlink(Path p)
        {
            HTimer.Stop();
            HiddenAll();
            HPOLY = p;
            if (HPOLY != null)
            {
                if (HPOLY == RotateR)
                {
                    RRshadow.Visibility = Visibility.Visible;
                }
                if (HPOLY == RotateL)
                {
                    RLshadow.Visibility = Visibility.Visible;
                }
                IsHBlink = true;
                if (HPOLY != null)
                {
                    HPOLY.Visibility = Visibility.Visible;
                }
                HTimer.Start();
            }
        }

        private void VBlink(Path p)
        {
            VTimer.Stop();
            Up.Visibility = Visibility.Hidden;
            Down.Visibility = Visibility.Hidden;
            Ushadow.Visibility = Visibility.Hidden;
            Dshadow.Visibility = Visibility.Hidden;
            VPOLY = p;
            if (VPOLY != null)
            {
                if (VPOLY == Up)
                {
                    Ushadow.Visibility = Visibility.Visible;
                }
                if (VPOLY == Down)
                {
                    Dshadow.Visibility = Visibility.Visible;
                }
                IsVBlink = true;
                if (VPOLY != null)
                {
                    VPOLY.Visibility = Visibility.Visible;
                }
                VTimer.Start();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public void Dispose()
        {
            HTimer?.Stop();
            VTimer.Stop();
            HTimer?.Dispose();
            VTimer.Dispose();
        }
    }
}
