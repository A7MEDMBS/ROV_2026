//using System;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using HelixToolkit.Wpf;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.ComponentModel;
//using System.Windows.Controls;
//using System.Collections.Generic;
//using System.Windows.Media.Media3D;
//using System.Windows.Media.Animation;
//using System.Runtime.CompilerServices;
//using System.Numerics;
//using System.Windows.Media.Imaging;
//using System.Security.Cryptography;

//namespace ROV_GUI_Control.View
//{
//    /// <summary>
//    /// Interaction logic for ROV3DModel.xaml
//    /// </summary>
//    public partial class ROV3DModel : UserControl, INotifyPropertyChanged, IDisposable
//    {
//        public static readonly DependencyProperty H_DirValueProperty = DependencyProperty.Register("H_move", typeof(int), typeof(ROV3DModel),
//            new PropertyMetadata(0, OnHDirValuePropertyChanged));
//        public int H_move
//        {
//            get { return (int)GetValue(H_DirValueProperty); }
//            set { SetValue(H_DirValueProperty, value); }
//        }
//        private static void OnHDirValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            ROV3DModel f = d as ROV3DModel;
//            f.OnHDirValueChanged(e);
//        }
//        public virtual void OnHDirValueChanged(DependencyPropertyChangedEventArgs e)
//        {
//            if (H_move != -1)
//                H_MoveTimer.Start();
//            else
//                H_MoveTimer.Stop();
//        }

//        public static readonly DependencyProperty V_DirValueProperty = DependencyProperty.Register("V_move", typeof(int), typeof(ROV3DModel),
//            new PropertyMetadata(0, OnVDirValuePropertyChanged));
//        public int V_move
//        {
//            get { return (int)GetValue(V_DirValueProperty); }
//            set { SetValue(V_DirValueProperty, value); }
//        }
//        private static void OnVDirValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            ROV3DModel f = d as ROV3DModel;
//            f.OnVDirValueChanged(e);
//        }
//        public virtual void OnVDirValueChanged(DependencyPropertyChangedEventArgs e)
//        {
//            if (V_move != -1)
//                V_MoveTimer.Start();
//            else
//                V_MoveTimer.Stop();
//        }

//        public static readonly DependencyProperty MarkValueProperty = DependencyProperty.Register("AddMark", typeof(bool), typeof(ROV3DModel),
//            new PropertyMetadata(false, OnMarkValuePropertyChanged));
//        public bool AddMark
//        {
//            get { return (bool)GetValue(MarkValueProperty); }
//            set { SetValue(MarkValueProperty, value); }
//        }
//        private static void OnMarkValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            ROV3DModel f = d as ROV3DModel;
//            f.OnMarkValueChanged(e);
//        }
//        public virtual void OnMarkValueChanged(DependencyPropertyChangedEventArgs e)
//        {
//            Addmark(new Point3D(X_Position, Y_Position, 0));
//        }
        
        
//        private readonly System.Timers.Timer H_MoveTimer;
//        private readonly System.Timers.Timer V_MoveTimer;
//        Point3D center;
//        readonly Color[] partColors = [Colors.Blue, Colors.WhiteSmoke, Colors.Red];
//        int i = 1;
//        Model3D model;
//        public ICommand Add { get; }
//        public ICommand Delete { get; }
//        public ICommand Run { get; }
//        public ICommand MoveTo { get; }

//        private float _width = 10;
//        public float Pool_width
//        {
//            get => _width;
//            set
//            {
//                if (_width != value)
//                {
//                    _width = value;
//                    OnPropertyChanged(nameof(Pool_width));
//                }
//            }
//        }
//        private float _lengtht = 20;
//        public float Pool_length
//        {
//            get => _lengtht;
//            set
//            {
//                if (_lengtht != value)
//                {
//                    _lengtht = value;
//                    OnPropertyChanged(nameof(Pool_length));
//                }
//            }
//        }
//        private float _height = 6;
//        public float Pool_height
//        {
//            get => _height;
//            set
//            {
//                if (_height != value)
//                {
//                    _height = value;
//                    OnPropertyChanged(nameof(Pool_height));
//                }
//            }
//        }
//        private float _grid = 0.5f;
//        public float GridSize
//        {
//            get => _grid;
//            set
//            {
//                if (_grid != value)
//                {
//                    _grid = value;
//                    OnPropertyChanged(nameof(GridSize));
//                }
//            }
//        }
//        private double _yaw = 0;
//        public double Yaw
//        {
//            get => _yaw;
//            set
//            {
//                if (_yaw != value)
//                {
//                    _yaw = value;
//                    OnPropertyChanged(nameof(Yaw));
//                }
//            }
//        }
        
//        private double path_x = 0;
//        public double Path_X
//        {
//            get { return path_x; }
//            set
//            {
//                if (path_x != value)
//                {
//                    path_x = value;
//                    OnPropertyChanged(nameof(Path_X));
//                }
//            }
//        }
//        private double path_y = 0;
//        public double Path_Y
//        {
//            get { return path_y; }
//            set
//            {
//                if (path_y != value)
//                {
//                    path_y = value;
//                    OnPropertyChanged(nameof(Path_Y));
//                }
//            }
//        }
//        private double path_z = 0;
//        public double Path_Z
//        {
//            get { return path_z; }
//            set
//            {
//                if (path_z != value)
//                {
//                    path_z = value;
//                    OnPropertyChanged(nameof(Path_Z));
//                }
//            }
//        }
//        private double path_speed = 0;
//        public double Path_Speed
//        {
//            get { return path_speed; }
//            set
//            {
//                if (path_speed != value)
//                {
//                    path_speed = value;
//                    OnPropertyChanged(nameof(Path_Speed));
//                }
//            }
//        }

//        private double moveTo_x = 0;
//        public double MoveTo_X
//        {
//            get { return moveTo_x; }
//            set
//            {
//                if (moveTo_x != value)
//                {
//                    moveTo_x = value;
//                    OnPropertyChanged(nameof(MoveTo_X));
//                }
//            }
//        }
//        private double moveTo_y = 0;
//        public double MoveTo_Y
//        {
//            get { return moveTo_y; }
//            set
//            {
//                if (moveTo_y != value)
//                {
//                    moveTo_y = value;
//                    OnPropertyChanged(nameof(MoveTo_Y));
//                }
//            }
//        }
//        private double moveTo_z = 0;
//        public double MoveTo_Z
//        {
//            get { return moveTo_z; }
//            set
//            {
//                if (moveTo_z != value)
//                {
//                    moveTo_z = value;
//                    OnPropertyChanged(nameof(MoveTo_Z));
//                }
//            }
//        }
//        private double moveTo_speed = 0;
//        public double MoveTo_Speed
//        {
//            get { return moveTo_speed; }
//            set
//            {
//                if (moveTo_speed != value)
//                {
//                    moveTo_speed = value;
//                    OnPropertyChanged(nameof(MoveTo_Speed));
//                }
//            }
//        }

//        private double x_position;
//        public double X_Position
//        {
//            get { return x_position; }
//            set
//            {
//                x_position = value;
//                OnPropertyChanged(nameof(X_Position));
//            }
//        }
//        private double y_position;
//        public double Y_Position
//        {
//            get { return y_position; }
//            set
//            {
//                y_position = value;
//                OnPropertyChanged(nameof(Y_Position));
//            }
//        }
//        private double z_position;
//        public double Z_Position
//        {
//            get { return z_position; }
//            set
//            {
//                z_position = value;
//                OnPropertyChanged(nameof(Z_Position));
//            }
//        }
//        private Point3D _position;
//        public Point3D Position
//        {
//            get { return _position; }
//            set
//            {
//                if (_position != value)
//                {
//                    _position = value;
//                    OnPropertyChanged(nameof(Position));
//                }
//            }
//        }
//        private double _rovdiameter = 0.31;
//        public double ROV_Diameter
//        {
//            get { return _rovdiameter; }
//            set
//            {
//                if (_rovdiameter != value)
//                {
//                    _rovdiameter = value;
//                    OnPropertyChanged(nameof(ROV_Diameter));
//                }
//            }
//        }
//        private StringBuilder _sampletext = new();
//        public string SampleText
//        {
//            get => _sampletext.ToString();
//            set
//            {
//                _sampletext = new StringBuilder(value);
//                OnPropertyChanged(nameof(SampleText));
//            }
//        }
//        private ModelVisual3D CatchedSample;
//        private int LineCount = 0;
//        private Point3D PrevSample = new(0, 0 ,0);
//        private readonly double X_bound;
//        private readonly double Y_bound;
//        private readonly double Z_bound;
//        private readonly Queue<Point3D> TrackPoints = new();
//        private readonly HashSet<string> Points = [];
//        private readonly Queue<Point3D> SamplesPoints = new();
//        private readonly List<LinesVisual3D> lineList = [];
//        private bool AutoMoving = false;
//        private bool V_Moving = false;
//        Point3D PrevPoint;

//        public ROV3DModel()
//        {
//            InitializeComponent();
//            PrevPoint = new(0, 0, 0);
//            X_bound = (Pool_width - ROV_Diameter) *1000;
//            Y_bound = (Pool_length - ROV_Diameter) * 1000;
//            Z_bound = (Pool_height - 0.3) * 1000;
//            Add = new RelayCommand(_ => AddPoint(new Point3D(Path_X, Path_Y, Path_Z)));
//            Delete = new RelayCommand(_ => DeletePoint());
//            Run = new RelayCommand(_ => StartAutoPath());
//            MoveTo = new RelayCommand(_ => MoveToPoint());
//            H_MoveTimer = new System.Timers.Timer(50);
//            H_MoveTimer.Elapsed += H_ManualMove_Tick;
//            H_MoveTimer.AutoReset = true;
//            V_MoveTimer = new System.Timers.Timer(50);
//            V_MoveTimer.Elapsed += V_ManualMove_Tick;
//            V_MoveTimer.AutoReset = true;
//            AddPool();
//            AddROV("ROV2025.obj");
//            GetCenter();
//            Task11(new Point3D(1750, 750, 1));
//            Task12(new Point3D(1750, 3750, 1));
//            Task13(new Point3D(1750, 6750, 1));
//            Task21(new Point3D(13.35, 10500, 3000), new Vector3D(0, 0, 1), 750, 1000, new Uri("pack://application:,,,/Media/Task21.jpg", UriKind.Absolute));
//            Task23(new Point3D(0, 13500, 0), new Vector3D(0, 0, 0), 0);
//            Task31(new Point3D(6809, 16500, 1.5));
//            Task32(new Point3D(6809, 12500, 1.5));
//            CatchedSample = null;
//        }
//        #region view
//        private void AddPool()
//        {
//            float width = Pool_width * 1000;
//            float length = Pool_length * 1000;
//            float height = Pool_height * 1000;
//            CreateTile(new Point3D(5000, 10000, 0), new Vector3D(0, 0, 0), 10000, 20000, 10, 20, new Uri("pack://application:,,,/poolTile.jpg", UriKind.Absolute));
//            CreateTile(new Point3D(0, 10000, 2850), new Vector3D(0, 1, 0), 5700, 20000, 4, 14.035, new Uri("pack://application:,,,/poolTile3.jpg", UriKind.Absolute));
//            CreateTile(new Point3D(5000, 20000, 2850), new Vector3D(1, 0, 0), 10000, 5700, 7.0175, 4, new Uri("pack://application:,,,/poolTile3.jpg", UriKind.Absolute));
            
//            var builder = new MeshBuilder();
//           /* var path = new List<Point3D>
//            {
//                new Point3D(1000, 1000,0),
//                new Point3D(1000, 1000,1000)
//            };
//            double[] dia = { 500, 200, 500 };
//            double[] value = { 0, 0 };
            
//            builder.AddTube(path, value, dia, 46, false);*/
//            //builder.AddTorus(50, 500, 20,15);
//            var model = new GeometryModel3D
//            {
//                Geometry = builder.ToMesh(),
//                BackMaterial = MaterialHelper.CreateMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#47D3E4"))),
//                Material = MaterialHelper.CreateMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#47D3E4")))
//            };
//            ROV3DView.Children.Add(new ModelVisual3D { Content = model });
//            var modelGroup = new Model3DGroup();
//            modelGroup.Children.Add(CreateBox(new Point3D(-1000, 0, 5700), 1001, 21000, 300, Colors.LightGray));
//            modelGroup.Children.Add(CreateBox(new Point3D(-1000, 19999, 5700), 11000, 1000, 300, Colors.LightGray));
//            ModelVisual3D visual = new() { Content = modelGroup };
//            ROV3DView.Children.Add(visual);
//        }
//        private GeometryModel3D CreateBox(Point3D origin, double width, double height, double depth, Color color)
//        {
//            MeshGeometry3D mesh = new();
//            Point3D p0 = origin;
//            Point3D p1 = new(origin.X + width, origin.Y, origin.Z);
//            Point3D p2 = new(origin.X + width, origin.Y, origin.Z + depth);
//            Point3D p3 = new(origin.X, origin.Y, origin.Z + depth);
//            Point3D p4 = new(origin.X, origin.Y + height, origin.Z);
//            Point3D p5 = new(origin.X + width, origin.Y + height, origin.Z);
//            Point3D p6 = new(origin.X + width, origin.Y + height, origin.Z + depth);
//            Point3D p7 = new(origin.X, origin.Y + height, origin.Z + depth);
//            Point3D[] pts = [p0, p1, p2, p3, p4, p5, p6, p7];
//            void AddFace(int a, int b, int c, int d)
//            {
//                mesh.Positions.Add(pts[a]);
//                mesh.Positions.Add(pts[b]);
//                mesh.Positions.Add(pts[c]);
//                mesh.Positions.Add(pts[d]);
//                int i = mesh.Positions.Count - 4;
//                mesh.TriangleIndices.Add(i);
//                mesh.TriangleIndices.Add(i + 1);
//                mesh.TriangleIndices.Add(i + 2);
//                mesh.TriangleIndices.Add(i);
//                mesh.TriangleIndices.Add(i + 2);
//                mesh.TriangleIndices.Add(i + 3);
//            }
//            AddFace(0, 1, 2, 3);
//            AddFace(0, 4, 5, 1);
//            AddFace(3, 2, 6, 7);
//            AddFace(0, 3, 7, 4);
//            AddFace(1, 5, 6, 2);
//            var geometrymodel3d = new GeometryModel3D
//            {
//                Geometry = mesh,
//                Material = new DiffuseMaterial(new SolidColorBrush(color)),
//                BackMaterial = new DiffuseMaterial(new SolidColorBrush(color))
//            };
//            return geometrymodel3d;
//        }
//        public void CreateTile(Point3D center, Vector3D dir, double width, double length, double tileWidth, double tileLength, Uri textureUri)
//        {
//            double halfW = width / 2.0;
//            double halfL = length / 2.0;

//            // rectangle vertices
//            var mesh = new MeshGeometry3D();
//            mesh.Positions.Add(new Point3D(-halfW, -halfL, 0)); // 0
//            mesh.Positions.Add(new Point3D(+halfW, -halfL, 0)); // 1
//            mesh.Positions.Add(new Point3D(+halfW, +halfL, 0)); // 2
//            mesh.Positions.Add(new Point3D(-halfW, +halfL, 0)); // 3

//            // one rectangle (two triangles)
//            mesh.TriangleIndices.Add(0);
//            mesh.TriangleIndices.Add(1);
//            mesh.TriangleIndices.Add(2);
//            mesh.TriangleIndices.Add(2);
//            mesh.TriangleIndices.Add(3);
//            mesh.TriangleIndices.Add(0);

//            // texture coordinates – repeat by tile count
//            mesh.TextureCoordinates.Add(new Point(0, 0));
//            mesh.TextureCoordinates.Add(new Point(tileWidth, 0));
//            mesh.TextureCoordinates.Add(new Point(tileWidth, tileLength));
//            mesh.TextureCoordinates.Add(new Point(0, tileLength));


//            var uri = textureUri; 
//            var bmp = new BitmapImage();
//            bmp.BeginInit();
//            bmp.UriSource = uri;
//            bmp.CacheOption = BitmapCacheOption.OnLoad;
//            bmp.EndInit();
//            bmp.Freeze();
        
//            var image = bmp;
//            //image.Freeze();
//            var brush = new ImageBrush(image)
//            {
//                ViewportUnits = BrushMappingMode.Absolute,
//                TileMode = TileMode.Tile
//            };
//            brush.Freeze();

//            var material = new DiffuseMaterial(brush);

//            var model = new GeometryModel3D
//            {
//                Geometry = mesh,
//                Material = material,
//                BackMaterial = material
//            };
//            var visual = new ModelVisual3D { Content = model };
//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(dir, 90)));
//            transformGroup.Children.Add(new TranslateTransform3D(center.X, center.Y, center.Z));
//            visual.Transform = transformGroup;
//            ROV3DView.Children.Add(visual);
//        }
//        private void AddROV(string modelPath)
//        {
//            var importer = new ModelImporter
//            {
//                DefaultMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Blue))
//            };
//            model = importer.Load(modelPath);
//            var bounds = model.Bounds;
//            center = new Point3D(
//                (bounds.X + bounds.SizeX) / 2,
//                (bounds.Y + bounds.SizeY) / 2,
//                (bounds.Z + bounds.SizeZ) / 2);
//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new TranslateTransform3D(-center.X, -center.Y, -center.Z));
//            transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(new Vector3D(0, 0, 1), -90)));
//            transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(new Vector3D(0, 1, 0), -90)));
//            transformGroup.Children.Add(new TranslateTransform3D(center.Z, center.Y, center.X));
//            model.Transform = transformGroup;
//            transformGroup.Children.Add(new TranslateTransform3D(95, -12, -53));
//            model.Transform = transformGroup;
//            ROV3DView.Children.Add(new ModelVisual3D { Content = model });
//            var modelGroup = model as Model3DGroup;
//            foreach (var child in modelGroup.Children)
//            {
//                if (child is GeometryModel3D geometry)
//                {
//                    geometry.Material = new DiffuseMaterial(new SolidColorBrush(partColors[i % 2]));
//                    i++;
//                }
//            }
//            /*Matrix3D matrix = model.Transform.Value;
//            double roll = Math.Atan2(matrix.M31, matrix.M33);
//            double pitch = Math.Asin(-matrix.M32);
//            double yaw = Math.Atan2(matrix.M12, matrix.M22);
//            yaw = yaw * 180 / Math.PI;
//            pitch = pitch * 180 / Math.PI;
//            roll = roll * 180 / Math.PI;*/
//        }
//        private void AddPoint(Point3D point)
//        {
//            if (TrackPoints.Count == 0)
//            {
//                TrackPoints.Enqueue(point);
//            }
//            else
//            {
//                string key = GetPointsKey(PrevPoint, point);
//                if (Points.Contains(key))
//                    return;
//                TrackPoints.Enqueue(point);
//                var line = new LinesVisual3D
//                {
//                    Color = Colors.Red,
//                    Thickness = 5,
//                    Points = [PrevPoint, point]
//                };
//                Points.Add(key);
//                lineList.Add(line);
//                ROV3DView.Children.Add(line);
//            }
//            PrevPoint = point;
//        }
//        private void DeletePoint()
//        {
//            if (TrackPoints.Count != 0)
//            {
//                TrackPoints.Dequeue();
//                if (TrackPoints.Count != 0)
//                    PrevPoint = TrackPoints.Last();
//                else
//                    PrevPoint = new(0, 0, 0);
//                if (lineList.Count == 0)
//                    return;
//                var lastLine = lineList[lineList.Count - 1];
//                ROV3DView.Children.Remove(lastLine);
//                lineList.RemoveAt(lineList.Count - 1);
//                var points = lastLine.Points.ToArray();
//                string key = GetPointsKey(points[0], points[1]);
//                Points.Remove(key);
//            }
//        }
//        private string GetPointsKey(Point3D a, Point3D b)
//        {
//            var ordered = new[] { a, b }.OrderBy(p => p.X)
//                                        .ThenBy(p => p.Y)
//                                        .ThenBy(p => p.Z)
//                                        .ToArray();
//            return $"{ordered[0]}|{ordered[1]}";
//        }
//        private void Addmark(Point3D point)
//        {
//            if (point == PrevSample) return;

//            var tube = new TubeVisual3D
//            {
//                Path =
//                [
//                     new Point3D(0, 0, 0),
//                     new Point3D(0, 0, 1000),
//                ],
//                Diameter = 30,
//                Fill = new SolidColorBrush(Colors.Red),
//                IsPathClosed = true,
//                ThetaDiv = 60
//            };
//            var mesh = new MeshGeometry3D();
//            mesh.Positions.Add(new Point3D(0, 0, 700));
//            mesh.Positions.Add(new Point3D(-300, -300, 850));
//            mesh.Positions.Add(new Point3D(0, 0, 1000));
//            mesh.TriangleIndices = [0, 1, 2];
//            var material = new DiffuseMaterial(new SolidColorBrush(Colors.LawnGreen));
//            var backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LawnGreen));
//            var model = new GeometryModel3D
//            {
//                Geometry = mesh,
//                Material = material,
//                BackMaterial = backMaterial
//            };
//            var visual = new ModelVisual3D { Content = model };
//            visual.Children.Add(tube);
//            visual.Transform = new TranslateTransform3D(point.X, point.Y, 0);
//            //ROV3DView.Children.Add(visual);

//            var timestamp = $"[{DateTime.Now:HH:mm:ss}] ";
//            var newMessage = timestamp + $"Sample identified at {Position.X}, {Position.Y}";
//            if (LineCount > 0)
//                _sampletext.AppendLine();
//            _sampletext.Append(newMessage);
//            LineCount++;
//            SampleText = SampleText.ToString();
//            PrevSample = point;
//        }
//        private ModelVisual3D AlgaeType0(Point3D point, Vector3D vector, double angle)
//        {
            
//            var mesh = new MeshBuilder(false, false);
//            var pasth1 = new List<Point3D> { 
//                new(0, 0, 0),
//                new (0, 0, 60),
//            };
//            var pasth2 = new List<Point3D> {
//                new (100, 260, 0),
//                new (100, 260, 110.5),
//            };
//            var pasth3 = new List<Point3D> {
//                new (0, 260, 110.5),
//                new (200, 260, 110.5),
//            };
//            double[] values = { 26.7, 26.7 };
//            mesh.AddTube(pasth1, values, values, 60, false);
//            //mesh.AddTube(pasth2, values, values, 60, false);
//            //mesh.AddTube(pasth3, values, values, 60, false);
//            //mesh.AddSphere(new (100, 260, 0), 13.35, 60, 60);
//            mesh.AddTorus(18, 8.7, 60, 60);
//            var modelGroup = new Model3DGroup();
//            var model = new GeometryModel3D
//            {
//                Geometry = mesh.ToMesh(),
//                Material = new DiffuseMaterial(new SolidColorBrush(Colors.LawnGreen)),
//                BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LawnGreen))
//            };
//            var visual = new ModelVisual3D { Content = model };
//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(vector, angle)));
//            transformGroup.Children.Add(new TranslateTransform3D(point.X, point.Y, point.Z));
//            visual.Transform = transformGroup;
//            return visual;
//        }
//        private ModelVisual3D AlgaeType1(Point3D point, Vector3D vector, double angle)
//        {
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new(100, 0, 0), new(100, 260, 0), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(100, 260, 0), new(100, 260, 110.5), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(0, 260, 110.5), new(200, 260, 110.5), 26.7, Colors.Green));
//            visual.Children.Add(Torus(new(100, 1, 0), new(1, 0, 0), 13.35, Colors.Green));
//            visual.Children.Add(Torus(new(1, 260, 110.5), new(0, 1, 0), 13.35, Colors.Green));
//            visual.Children.Add(Torus(new(199, 260, 110.5), new(0, 1, 0), 13.35, Colors.Green));
//            visual.Children.Add(Sphere(new(100, 260, 0), 13.35, Colors.Green));
//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(vector, angle)));
//            transformGroup.Children.Add(new TranslateTransform3D(point.X, point.Y, point.Z));
//            visual.Transform = transformGroup;
//            return visual;
//        }
//        private ModelVisual3D AlgaeType2(Point3D point, Vector3D vector, double angle)
//        {
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new(45, 321, 100), new(91, 321, 100), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(91, 321, 100), new(91, 0, 100), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(91, 0, 100), new(0, 0, 100), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(0, 0, 100), new(0, 100, 100), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(0, 100, 0), new(0, 100, 200), 26.7, Colors.Green));
//            visual.Children.Add(Torus(new(0, 100, 1), new(0, 0, 1), 13.35, Colors.Green));
//            visual.Children.Add(Torus(new(0, 100, 199), new(0, 0, 1), 13.35, Colors.Green));
//            visual.Children.Add(Torus(new(46, 321, 100), new(0, 1, 0), 13.35, Colors.Green));
//            visual.Children.Add(Sphere(new(91, 321, 100), 13.35, Colors.Green));
//            visual.Children.Add(Sphere(new(91, 0, 100), 13.35, Colors.Green));
//            visual.Children.Add(Sphere(new(0, 0, 100), 13.35, Colors.Green));
//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(vector, angle)));
//            transformGroup.Children.Add(new TranslateTransform3D(point.X, point.Y, point.Z-14));
//            visual.Transform = transformGroup;
//            return visual;
//        }
//        private ModelVisual3D AlgaeType3(Point3D point, Vector3D vector, double angle)
//        {
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new(100, 0, 321), new(100, 45, 321), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(100, 45, 321), new(100, 45, 0), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(100, 45, 0), new(100, 91, 0), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(100, 91, 0), new(100, 91, 110), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(200, 91, 110), new(0, 91, 110), 26.7, Colors.Green));
//            visual.Children.Add(PVC(new(0, 91, 110), new(0, 91, 0), 26.7, Colors.Green));
//            visual.Children.Add(Torus(new(100, 1, 321), new(1, 0, 0), 13.35, Colors.Green));
//            visual.Children.Add(Torus(new(199, 91, 110), new(0, 1, 0), 13.35, Colors.Green));
//            visual.Children.Add(Torus(new(0, 91, 1), new(0, 0, 1), 13.35, Colors.Green));
//            visual.Children.Add(Sphere(new(100, 45, 321), 13.25, Colors.Green));
//            visual.Children.Add(Sphere(new(100, 45, 0), 13.35, Colors.Green));
//            visual.Children.Add(Sphere(new(100, 91, 0), 13.35, Colors.Green));
//            visual.Children.Add(Sphere(new(0, 91, 110), 13.35, Colors.Green));
//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(vector, angle)));
//            transformGroup.Children.Add(new TranslateTransform3D(point.X, point.Y, point.Z));
//            visual.Transform = transformGroup;
//            return visual; ;
//        }
//        private void Task11(Point3D point)
//        {
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new Point3D(0, 0, 14), new Point3D(0, 1500, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, 1500, 14), new Point3D(1500, 1500, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1500, 1500, 14), new Point3D(1500, 0, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1500, 0, 14), new Point3D(0, 0, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 0, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 1500, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1500, 1500, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1500, 0, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(AlgaeType1(new Point3D(250, 750, 14), new Vector3D(0, 0, 0), 0));
//            visual.Children.Add(AlgaeType2(new Point3D(750, 750, 14), new Vector3D(0, 0, 0), 0));
//            visual.Children.Add(AlgaeType3(new Point3D(1250, 750, 14), new Vector3D(0, 0, 0), 0));
//            visual.Transform = new TranslateTransform3D(point.X, point.Y, point.Z);
//            ROV3DView.Children.Add(visual);
//        }
//        private void Task12(Point3D point)
//        {
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new Point3D(0, 0, 14), new Point3D(0, 1500, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, 1500, 14), new Point3D(1500, 1500, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1500, 1500, 14), new Point3D(1500, 0, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1500, 0, 14), new Point3D(0, 0, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 0, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 1500, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1500, 1500, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1500, 0, 14), 13.25, Colors.WhiteSmoke));

//            visual.Children.Add(PVC0(new Point3D(500, 500, 0), new Point3D(500, 500, 200), 50, Colors.WhiteSmoke));

//            visual.Children.Add(PVC0(new Point3D(1000, 500, 0), new Point3D(1000, 500, 95), 50, Colors.WhiteSmoke));
//            visual.Children.Add(PVC0(new Point3D(1000, 500, 95), new Point3D(1000, 500, 105), 50, Colors.Black));
//            visual.Children.Add(PVC0(new Point3D(1000, 500, 105), new Point3D(1000, 500, 200), 50, Colors.WhiteSmoke));

//            visual.Children.Add(PVC0(new Point3D(1000, 1000, 0), new Point3D(1000, 1000, 85), 50, Colors.WhiteSmoke));
//            visual.Children.Add(PVC0(new Point3D(1000, 1000, 85), new Point3D(1000, 1000, 95), 50, Colors.Black));
//            visual.Children.Add(PVC0(new Point3D(1000, 1000, 95), new Point3D(1000, 1000, 105), 50, Colors.WhiteSmoke));
//            visual.Children.Add(PVC0(new Point3D(1000, 1000, 105), new Point3D(1000, 1000, 115), 50, Colors.Black));
//            visual.Children.Add(PVC0(new Point3D(1000, 1000, 115), new Point3D(1000, 1000, 200), 50, Colors.WhiteSmoke));

//            visual.Children.Add(PVC0(new Point3D(500, 1000, 0), new Point3D(500, 1000, 75), 50, Colors.WhiteSmoke));
//            visual.Children.Add(PVC0(new Point3D(500, 1000, 75), new Point3D(500, 1000, 85), 50, Colors.Black));
//            visual.Children.Add(PVC0(new Point3D(500, 1000, 85), new Point3D(500, 1000, 95), 50, Colors.WhiteSmoke));
//            visual.Children.Add(PVC0(new Point3D(500, 1000, 95), new Point3D(500, 1000, 105), 50, Colors.Black));
//            visual.Children.Add(PVC0(new Point3D(500, 1000, 105), new Point3D(500, 1000, 115), 50, Colors.WhiteSmoke));
//            visual.Children.Add(PVC0(new Point3D(500, 1000, 115), new Point3D(500, 1000, 125), 50, Colors.Black));
//            visual.Children.Add(PVC0(new Point3D(500, 1000, 125), new Point3D(500, 1000, 200), 50, Colors.WhiteSmoke));
            
//            visual.Transform = new TranslateTransform3D(point.X, point.Y, point.Z);
//            ROV3DView.Children.Add(visual);
//        }
//        private void Task13(Point3D point)
//        {
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new Point3D(0, 0, 14), new Point3D(0, 1500, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, 1500, 14), new Point3D(1500, 1500, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1500, 1500, 14), new Point3D(1500, 0, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1500, 0, 14), new Point3D(0, 0, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 0, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 1500, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1500, 1500, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1500, 0, 14), 13.25, Colors.WhiteSmoke));
//            var rec = new RectangleVisual3D
//            {
//                Width = 210,
//                Length = 297,
//                Material = MaterialHelper.CreateMaterial(Colors.Black),
//                BackMaterial = MaterialHelper.CreateMaterial(Colors.Black),
//                DivLength = 60,
//                DivWidth = 60,
//                LengthDirection = new Vector3D(0, 1, 0)

//            };
//            rec.Transform = new TranslateTransform3D(750, 750, 7);
//            visual.Children.Add(rec);
//            var importer = new ModelImporter
//            {
//                DefaultMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.WhiteSmoke))
//            };
//            var task = importer.Load("Task13.obj");
//            var group = new Transform3DGroup();
//            group.Children.Add(new ScaleTransform3D(100, 100, 100));
//            group.Children.Add(new TranslateTransform3D(750, 750, 5));
//            task.Transform = group;
//            visual.Children.Add(new ModelVisual3D { Content = task });
//            visual.Transform = new TranslateTransform3D(point.X, point.Y, point.Z);
//            ROV3DView.Children.Add(visual);
//        }
//        private void Task21(Point3D point, Vector3D vector, double width, double length, Uri textureUri)
//        {
//            double halfW = width / 2.0;
//            double halfL = length / 2.0;
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new Point3D(0, point.Y - halfL, point.Z - halfW), new Point3D(50, point.Y - halfL, point.Z - halfW), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, point.Y - halfL, point.Z + halfW), new Point3D(50, point.Y - halfL, point.Z + halfW), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, point.Y + halfL, point.Z + halfW), new Point3D(50, point.Y + halfL, point.Z + halfW), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, point.Y + halfL, point.Z - halfW), new Point3D(50, point.Y + halfL, point.Z - halfW), 26.7, Colors.WhiteSmoke));


//            visual.Children.Add(PVC(new Point3D(50, point.Y - halfL, point.Z - halfW), new Point3D(50, point.Y - halfL, point.Z + halfW), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(50, point.Y - halfL, point.Z + halfW), new Point3D(50, point.Y + halfL, point.Z + halfW), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(50, point.Y + halfL, point.Z + halfW), new Point3D(50, point.Y + halfL, point.Z - halfW), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(50, point.Y + halfL, point.Z - halfW), new Point3D(50, point.Y - halfL, point.Z - halfW), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(50, point.Y - halfL, point.Z - halfW), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(50, point.Y - halfL, point.Z + halfW), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(50, point.Y + halfL, point.Z + halfW), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(50, point.Y + halfL, point.Z - halfW), 13.25, Colors.WhiteSmoke));
//            var mesh = new MeshGeometry3D();
//            mesh.Positions.Add(new Point3D(-halfL, 0, -halfW));
//            mesh.Positions.Add(new Point3D(+halfL, 0, -halfW));
//            mesh.Positions.Add(new Point3D(+halfL, 0, +halfW));
//            mesh.Positions.Add(new Point3D(-halfL, 0, +halfW));
//            mesh.TriangleIndices.Add(0);
//            mesh.TriangleIndices.Add(1);
//            mesh.TriangleIndices.Add(2);
//            mesh.TriangleIndices.Add(2);
//            mesh.TriangleIndices.Add(3);
//            mesh.TriangleIndices.Add(0);
//            mesh.TextureCoordinates.Add(new Point(0, 0));
//            mesh.TextureCoordinates.Add(new Point(1, 0));
//            mesh.TextureCoordinates.Add(new Point(1, 1));
//            mesh.TextureCoordinates.Add(new Point(0, 1));
//            var uri = textureUri;
//            var bmp = new BitmapImage();
//            bmp.BeginInit();
//            bmp.UriSource = uri;
//            bmp.CacheOption = BitmapCacheOption.OnLoad;
//            bmp.EndInit();
//            bmp.Freeze();
//            var image = bmp;
//            var brush = new ImageBrush(image)
//            {
//                ViewportUnits = BrushMappingMode.Absolute,
//                TileMode = TileMode.Tile
//            };
//            brush.Freeze();
//            var material = new DiffuseMaterial(brush);
//            var model = new GeometryModel3D
//            {
//                Geometry = mesh,
//                Material = material,
//                BackMaterial = material
//            };
//            var Image = new ModelVisual3D { Content = model };
//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(vector, 90)));
//            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0,1,0), 180)));
//            transformGroup.Children.Add(new TranslateTransform3D(50, point.Y, point.Z));
//            Image.Transform = transformGroup;
//            ROV3DView.Children.Add(visual);
//            ROV3DView.Children.Add(Image);
//        }
//        private void Task23(Point3D point, Vector3D vector, double angle)
//        {
//            var visual = new ModelVisual3D();
//            for(int i = 3250; i>=700; i -= 150) 
//            {
//                if (i != 2500 && i != 1600)
//                    visual.Children.Add(PVC(new Point3D(250, 0, i), new Point3D(250, 6000, i), 26.7, Colors.WhiteSmoke));
//                else
//                {
//                    if (i == 2500)
//                    {
//                        visual.Children.Add(PVC(new Point3D(250, 0, i), new Point3D(250, 900, i), 26.7, Colors.WhiteSmoke));
//                        visual.Children.Add(PVC(new Point3D(250, 1200, i), new Point3D(250, 6000, i), 26.7, Colors.WhiteSmoke));
//                    }
//                    else
//                    {
//                        visual.Children.Add(PVC(new Point3D(250, 0, i), new Point3D(250, 2400, i), 26.7, Colors.WhiteSmoke));
//                        visual.Children.Add(PVC(new Point3D(250, 2700, i), new Point3D(250, 6000, i), 26.7, Colors.WhiteSmoke));
//                    }
//                }
//            }
//            for (int i = 150; i <= 5850; i += 150)
//            {
//                if(i != 1050 && i != 2550)
//                    visual.Children.Add(PVC(new Point3D(250, i, 3250), new Point3D(250, i, 700), 26.7, Colors.WhiteSmoke));
//                else
//                {
//                    if (i == 1050)
//                    {
//                        visual.Children.Add(PVC(new Point3D(250, i, 3250), new Point3D(250, i, 2650), 26.7, Colors.WhiteSmoke));
//                        visual.Children.Add(PVC(new Point3D(250, i, 2350), new Point3D(250, i, 700), 26.7, Colors.WhiteSmoke));
//                    }
//                    else
//                    {
//                        visual.Children.Add(PVC(new Point3D(250, i, 3250), new Point3D(250, i, 1750), 26.7, Colors.WhiteSmoke));
//                        visual.Children.Add(PVC(new Point3D(250, i, 1450), new Point3D(250, i, 700), 26.7, Colors.WhiteSmoke));
//                    }
//                }
//            }
//            for (int i = 2590; i >= 2410; i -= 60)
//            {
//                visual.Children.Add(PVC(new Point3D(250, 900, i), new Point3D(250, 1200, i), 15, Colors.WhiteSmoke));
//            }
//            for (int i = 960; i <= 1140; i += 60)
//            {
//                visual.Children.Add(PVC(new Point3D(250, i, 2650), new Point3D(250, i, 2350), 15, Colors.WhiteSmoke));
//            }

//            visual.Children.Add(PVC(new Point3D(250, 0, 3250), new Point3D(250, 0, 300), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(250, 6000, 3250), new Point3D(250, 6000, 300), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(250, 0, 300), new Point3D(250, 6000, 300), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(13.4, 0, 13.4), new Point3D(250, 0, 300), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(13.4, 6000, 13.4), new Point3D(250, 6000, 300), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(250, 0, 300), new Point3D(486.6, 0, 13.4), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(250, 6000, 300), new Point3D(486.6, 6000, 13.4), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(13.4, 0, 13.4), new Point3D(486.6, 0, 13.4), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(13.4, 6000, 13.4), new Point3D(486.6, 6000, 13.4), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(13.4, 0, 13.4), new Point3D(13.4, 6000, 13.4), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(486.6, 0, 13.4), new Point3D(486.6, 6000, 13.4), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(250, 0, 3250), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(250, 6000, 3250), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(13.4, 0, 13.4), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(486.6, 0, 13.4), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(13.4, 6000, 13.4), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(486.6, 6000, 13.4), 13.25, Colors.WhiteSmoke));
//            //path
//            visual.Children.Add(PVC(new Point3D(276.7, 675, 1525), new Point3D(276.7, 675, 2875), 26.7, Colors.Red));
//            visual.Children.Add(PVC(new Point3D(276.7, 675, 2875), new Point3D(276.7, 2175, 2875), 26.7, Colors.Red));
//            visual.Children.Add(PVC(new Point3D(276.7, 2175, 2875), new Point3D(276.7, 2175, 1225), 26.7, Colors.Red));
//            visual.Children.Add(PVC(new Point3D(276.7, 2175, 1225), new Point3D(276.7, 3825, 1225), 26.7, Colors.Red));
//            visual.Children.Add(PVC(new Point3D(276.7, 3825, 1225), new Point3D(276.7, 3825, 2875), 26.7, Colors.Red));
//            visual.Children.Add(PVC(new Point3D(276.7, 3825, 2875), new Point3D(276.7, 5325, 2875), 26.7, Colors.Red));
//            visual.Children.Add(PVC(new Point3D(276.7, 5325, 2875), new Point3D(276.7, 5325, 1525), 26.7, Colors.Red));
//            visual.Children.Add(Sphere(new(276.7, 675, 2875), 13.25, Colors.Red));
//            visual.Children.Add(Sphere(new(276.7, 2175, 2875), 13.25, Colors.Red));
//            visual.Children.Add(Sphere(new(276.7, 2175, 1225), 13.25, Colors.Red));
//            visual.Children.Add(Sphere(new(276.7, 3825, 1225), 13.25, Colors.Red));
//            visual.Children.Add(Sphere(new(276.7, 3825, 2875), 13.25, Colors.Red));
//            visual.Children.Add(Sphere(new(276.7, 5325, 2875), 13.25, Colors.Red));

//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(vector, angle)));
//            transformGroup.Children.Add(new TranslateTransform3D(point.X, point.Y, point.Z));
//            visual.Transform = transformGroup;
//            ROV3DView.Children.Add(visual);
//        }
//        private void Task31(Point3D point)
//        {
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new Point3D(-809, -900, 14), new Point3D(-809, 2100, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(-809, 2100, 14), new Point3D(2191, 2100, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(2191, 2100, 14), new Point3D(2191, -900, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(2191, -900, 14), new Point3D(-809, -900, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(-809, -900, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(-809, 2100, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(2191, 2100, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(2191, -900, 14), 13.25, Colors.WhiteSmoke));

//            visual.Children.Add(PVC(new Point3D(0, 0, 13.35), new Point3D(0, 0, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, 0, 77.65), new Point3D(1382, 0, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1382, 0, 77.65), new Point3D(1382, 0, 13.35), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1382, 0, 13.35), new Point3D(0, 0, 13.35), 26.7, Colors.WhiteSmoke));

//            visual.Children.Add(PVC(new Point3D(0, 1200, 13.35), new Point3D(0, 1200, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, 1200, 77.65), new Point3D(1382, 1200, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1382, 1200, 77.65), new Point3D(1382, 1200, 13.35), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1382, 1200, 13.35), new Point3D(0, 1200, 13.35), 26.7, Colors.WhiteSmoke));

//            visual.Children.Add(PVC(new Point3D(91, 0, 13.35), new Point3D(91, 1200, 13.35), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(91, 0, 77.65), new Point3D(91, 1200, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1291, 0, 13.35), new Point3D(1291, 1200, 13.35), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1291, 0, 77.65), new Point3D(1291, 1200, 77.65), 26.7, Colors.WhiteSmoke));

//            visual.Children.Add(PVC(new Point3D(61, 0, 77.65), new Point3D(121, 0, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(91, 0, 77.65), new Point3D(91, 30, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(61, 0, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(121, 0, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(91, 30, 77.65), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(61, 0, 13.35), new Point3D(121, 0, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(91, 0, 13.35), new Point3D(91, 30, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(61, 0, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(121, 0, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(91, 30, 13.35), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(1261, 0, 77.65), new Point3D(1321, 0, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(1291, 0, 77.65), new Point3D(1291, 30, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1261, 0, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1321, 0, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1291, 30, 77.65), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(1261, 0, 13.35), new Point3D(1321, 0, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(1291, 0, 13.35), new Point3D(1291, 30, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1261, 0, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1321, 0, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1291, 30, 13.35), new(0, 1, 0), 14.85, Colors.AntiqueWhite));
//            ///
//            visual.Children.Add(PVC(new Point3D(61, 1200, 77.65), new Point3D(121, 1200, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(91, 1170, 77.65), new Point3D(91, 1200, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(61, 1200, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(121, 1200, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(91, 1170, 77.65), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(61, 1200, 13.35), new Point3D(121, 1200, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(91, 1170, 13.35), new Point3D(91, 1200, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(61, 1200, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(121, 1200, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(91, 1170, 13.35), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(1261, 1200, 77.65), new Point3D(1321, 1200, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(1291, 1170, 77.65), new Point3D(1291, 1200, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1261, 1200, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1321, 1200, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1291, 1170, 77.65), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(1261, 1200, 13.35), new Point3D(1321, 1200, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(1291, 1170, 13.35), new Point3D(1291, 1200, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1261, 1200, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1321, 1200, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1291, 1170, 13.35), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(Sphere(new(0, 0, 13.35), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 0, 77.65), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1382, 0, 13.35), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1382, 0, 77.65), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 1200, 13.35), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 1200, 77.65), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1382, 1200, 13.35), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1382, 1200, 77.65), 13.25, Colors.WhiteSmoke));


//            visual.Children.Add(PVC(new Point3D(599.5, 508.5, -1.5), new Point3D(599.5, 508.5, 121.5), 60, Colors.White));
//            visual.Children.Add(PVC(new Point3D(599.5, 508.5, -1.5), new Point3D(599.5, 508.5, 121.5), 60, Colors.White));
//            visual.Children.Add(PVC(new Point3D(599.5, 508.5, -1.5), new Point3D(599.5, 508.5, 151.5), 26.7, Colors.White));
            
//            visual.Children.Add(PVC(new Point3D(963.5, 326.5, -1.5), new Point3D(963.5, 326.5, 121.5), 60, Colors.White));
//            visual.Children.Add(PVC(new Point3D(963.5, 326.5, -1.5), new Point3D(963.5, 326.5, 151.5), 26.7, Colors.White));

//            var importer = new ModelImporter
//            {
//                DefaultMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.White))
//            };
//            var task = importer.Load("Task2.obj");
//            var group = new Transform3DGroup();
//            group.Children.Add(new ScaleTransform3D(100, 100, 20));
//            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 180)));
//            group.Children.Add(new TranslateTransform3D(691, 600, 77.65));
//            task.Transform = group;
//            visual.Children.Add(new ModelVisual3D { Content = task });
//            visual.Transform = new TranslateTransform3D(point.X, point.Y, point.Z);
//            ROV3DView.Children.Add(visual);
//        }
//        private void Task32(Point3D point)
//        {
//            var visual = new ModelVisual3D();
//            visual.Children.Add(PVC(new Point3D(-809, -900, 14), new Point3D(-809, 2100, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(-809, 2100, 14), new Point3D(2191, 2100, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(2191, 2100, 14), new Point3D(2191, -900, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(2191, -900, 14), new Point3D(-809, -900, 14), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(-809, -900, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(-809, 2100, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(2191, 2100, 14), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(2191, -900, 14), 13.25, Colors.WhiteSmoke));

//            visual.Children.Add(PVC(new Point3D(0, 0, 13.35), new Point3D(0, 0, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, 0, 77.65), new Point3D(1382, 0, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1382, 0, 77.65), new Point3D(1382, 0, 13.35), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1382, 0, 13.35), new Point3D(0, 0, 13.35), 26.7, Colors.WhiteSmoke));

//            visual.Children.Add(PVC(new Point3D(0, 1200, 13.35), new Point3D(0, 1200, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(0, 1200, 77.65), new Point3D(1382, 1200, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1382, 1200, 77.65), new Point3D(1382, 1200, 13.35), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1382, 1200, 13.35), new Point3D(0, 1200, 13.35), 26.7, Colors.WhiteSmoke));

//            visual.Children.Add(PVC(new Point3D(91, 0, 13.35), new Point3D(91, 1200, 13.35), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(91, 0, 77.65), new Point3D(91, 1200, 77.65), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1291, 0, 13.35), new Point3D(1291, 1200, 13.35), 26.7, Colors.WhiteSmoke));
//            visual.Children.Add(PVC(new Point3D(1291, 0, 77.65), new Point3D(1291, 1200, 77.65), 26.7, Colors.WhiteSmoke));

//            visual.Children.Add(PVC(new Point3D(61, 0, 77.65), new Point3D(121, 0, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(91, 0, 77.65), new Point3D(91, 30, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(61, 0, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(121, 0, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(91, 30, 77.65), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(61, 0, 13.35), new Point3D(121, 0, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(91, 0, 13.35), new Point3D(91, 30, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(61, 0, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(121, 0, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(91, 30, 13.35), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(1261, 0, 77.65), new Point3D(1321, 0, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(1291, 0, 77.65), new Point3D(1291, 30, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1261, 0, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1321, 0, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1291, 30, 77.65), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(1261, 0, 13.35), new Point3D(1321, 0, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(1291, 0, 13.35), new Point3D(1291, 30, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1261, 0, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1321, 0, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1291, 30, 13.35), new(0, 1, 0), 14.85, Colors.AntiqueWhite));
//            ///
//            visual.Children.Add(PVC(new Point3D(61, 1200, 77.65), new Point3D(121, 1200, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(91, 1170, 77.65), new Point3D(91, 1200, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(61, 1200, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(121, 1200, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(91, 1170, 77.65), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(61, 1200, 13.35), new Point3D(121, 1200, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(91, 1170, 13.35), new Point3D(91, 1200, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(61, 1200, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(121, 1200, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(91, 1170, 13.35), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(1261, 1200, 77.65), new Point3D(1321, 1200, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(1291, 1170, 77.65), new Point3D(1291, 1200, 77.65), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1261, 1200, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1321, 1200, 77.65), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1291, 1170, 77.65), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(PVC(new Point3D(1261, 1200, 13.35), new Point3D(1321, 1200, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(PVC(new Point3D(1291, 1170, 13.35), new Point3D(1291, 1200, 13.35), 29.7, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1261, 1200, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1321, 1200, 13.35), new(1, 0, 0), 14.85, Colors.AntiqueWhite));
//            visual.Children.Add(Cap(new(1291, 1170, 13.35), new(0, 1, 0), 14.85, Colors.AntiqueWhite));

//            visual.Children.Add(Sphere(new(0, 0, 13.35), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 0, 77.65), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1382, 0, 13.35), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1382, 0, 77.65), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 1200, 13.35), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(0, 1200, 77.65), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1382, 1200, 13.35), 13.25, Colors.WhiteSmoke));
//            visual.Children.Add(Sphere(new(1382, 1200, 77.65), 13.25, Colors.WhiteSmoke));

            
//            visual.Children.Add(Cable(new Point3D(221, -250, 3), new Point3D(1191, -250, 3), 6, Colors.Black));
//            visual.Children.Add(PVC(new Point3D(191, -250, -1.5), new Point3D(191, -250, 121.5), 60, Colors.White));
//            visual.Children.Add(PVC(new Point3D(1191, -250, -1.5), new Point3D(1191, -250, 151.5), 26.7, Colors.White));

//            var importer = new ModelImporter
//            {
//                DefaultMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.WhiteSmoke))
//            };
//            var task = importer.Load("Task2.obj");
//            var group = new Transform3DGroup();
//            group.Children.Add(new ScaleTransform3D(100, 100, 20));
//            group.Children.Add(new TranslateTransform3D(691, 600, 77.65));
//            task.Transform = group;
//            visual.Children.Add(new ModelVisual3D { Content = task });
//            visual.Transform = new TranslateTransform3D(point.X, point.Y, point.Z);
//            ROV3DView.Children.Add(visual);
//        }
//        public TubeVisual3D PVC(Point3D p1, Point3D p2, double dia, Color col)
//        {
//            var tube = new TubeVisual3D
//            {
//                Path = [p1, p2],
//                Diameter = dia,
//                Fill = new SolidColorBrush(col),
//                Material = MaterialHelper.CreateMaterial(col),
//                BackMaterial = MaterialHelper.CreateMaterial(col),
//                ThetaDiv = 60,
//            };
//            return tube;
//        }
//        public TubeVisual3D Cable(Point3D p1, Point3D p2, double dia, Color col)
//        {
//            double len = (p2 - p1).Length;
//            var line = new Point3DCollection();
//            for (int i = 0; i < len; i += 10)
//            {
//                double x = i;
//                double y = -Math.Cos(i * Math.PI / 180) * 10;
//                line.Add(new Point3D(x, y, 0));
//            }
//            var tube = new TubeVisual3D
//            {
//                Path = line,
//                Fill = new SolidColorBrush(col),
//                Material = MaterialHelper.CreateMaterial(col),
//                BackMaterial = MaterialHelper.CreateMaterial(col),
//                Diameter = dia
//            };
//            tube.Transform = new TranslateTransform3D(p1.X, p1.Y, p1.Z);
//            return tube;
//        }
//        public TubeVisual3D PVC0(Point3D p1, Point3D p2, double dia, Color col)
//        {
//            var tube = new TubeVisual3D
//            {
//                Path = [p1, p2],
//                Diameter = dia,
//                Fill = new SolidColorBrush(col),
//                Material = MaterialHelper.CreateMaterial(col),
//                BackMaterial = MaterialHelper.CreateMaterial(Colors.WhiteSmoke),
//                ThetaDiv = 60,
//            };
//            return tube;
//        }
//        private ModelVisual3D Cap(Point3D center, Vector3D normal, double radius, Color color)
//        {
//            var builder = new MeshBuilder();
//            builder.AddCone(center, normal, radius, radius, 1, true, true, 60);
//            var mesh = builder.ToMesh();
//            var model = new GeometryModel3D
//            {
//                Geometry = mesh,
//                Material = MaterialHelper.CreateMaterial(color),
//                BackMaterial = MaterialHelper.CreateMaterial(color)
//            };
//            var modelvisual = new ModelVisual3D { Content = model };
//            return modelvisual;
//        }
//        private ModelVisual3D Torus(Point3D center, Vector3D normal, double radius, Color color)
//        {
//            var torus = new TorusVisual3D
//            {
//                TubeDiameter = 8.7,
//                TorusDiameter = radius*2 -8.7,
//                BackMaterial = MaterialHelper.CreateMaterial(new SolidColorBrush(color)),
//                Material = MaterialHelper.CreateMaterial(new SolidColorBrush(color)),
//                ThetaDiv = 60,
//                PhiDiv = 60
//            };
//            Transform3DGroup transformGroup = new();
//            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(normal, 90)));
//            transformGroup.Children.Add(new TranslateTransform3D(center.X, center.Y, center.Z));
//            torus.Transform = transformGroup;
//            return torus;
//        }
  
//        private SphereVisual3D Sphere(Point3D p, double rad, Color col)
//        {
//            var sphere = new SphereVisual3D
//            {
//                Center = p,
//                Radius = rad,
//                Fill = new SolidColorBrush(col),
//                Material = MaterialHelper.CreateMaterial(col),
//                BackMaterial = MaterialHelper.CreateMaterial(col),
//                ThetaDiv = 60

//            };
          
//            return sphere;
//        }
//        #endregion
//        public void MoveToPoint()
//        {
//            AutoMoving = true;
//            Move(new Point3D(MoveTo_X, MoveTo_Y, MoveTo_Z), MoveTo_Speed, GetCenter);
//            AutoMoving = false;
//        }
//        public void StartAutoPath()
//        {
//            if (!AutoMoving)
//                AutoMove();
//        }
//        private void AutoMove()
//        {
//            if (TrackPoints.Count == 0)
//            {
//                AutoMoving = false;
//                return;
//            }
//            AutoMoving = true;
//            var nextPoint = TrackPoints.Dequeue();
//            Move(nextPoint, 1, AutoMove);
//        }
//        private void H_ManualMove_Tick(object sender, EventArgs e)
//        {
//            Application.Current.Dispatcher.Invoke(() =>
//            {
//                ManualMove(H_move);
//            });
//        }
//        private void V_ManualMove_Tick(object sender, EventArgs e)
//        {
//            Application.Current.Dispatcher.Invoke(() =>
//            {
//                ManualMove(V_move);
//            });
//        }
//        private void ManualMove(int dir)
//        {
//            GetCenter();
//            var transformGroup = model.Transform as Transform3DGroup ?? new Transform3DGroup();           
//            double X = 0;
//            double Y = 0;
//            switch (dir)
//            {
//                case 0:
//                    X = Math.Cos((90 + Yaw) * Math.PI / 180.0) * 50;
//                    Y = Math.Sin((90 + Yaw) * Math.PI / 180.0) * 50;
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.02, GetCenter);
//                    break;
//                case 1:
//                    X = Math.Cos((-90 + Yaw) * Math.PI / 180.0) * 50;
//                    Y = Math.Sin((-90 + Yaw) * Math.PI / 180.0) * 50;
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.02, GetCenter);
//                    break;
//                case 2:
//                    X = Math.Cos((Yaw) * Math.PI / 180.0) * 50;
//                    Y = Math.Sin((Yaw) * Math.PI / 180.0) * 50;
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.02, GetCenter);
//                    break;
//                case 3:
//                    X = -Math.Cos((Yaw) * Math.PI / 180.0) * 50;
//                    Y = -Math.Sin((Yaw) * Math.PI / 180.0) * 50;
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.02, GetCenter);
//                    break;
//                case 4:
//                    X = Math.Cos((45 + Yaw) * Math.PI / 180.0) * 50;
//                    Y = Math.Sin((45 + Yaw) * Math.PI / 180.0) * 50;
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.02, GetCenter);
//                    break;
//                case 5:
//                    X = -Math.Cos((45 - Yaw) * Math.PI / 180.0) * 50;
//                    Y = Math.Sin((45 - Yaw) * Math.PI / 180.0) * 50;
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.02, GetCenter);
//                    break;
//                case 6:
//                    X = Math.Cos((45 - Yaw) * Math.PI / 180.0) * 50;
//                    Y = -Math.Sin((45 - Yaw) * Math.PI / 180.0) * 50;
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.02, GetCenter);
//                    break;
//                case 7:
//                    X = -Math.Cos((45 + Yaw) * Math.PI / 180.0) * 50;
//                    Y = -Math.Sin((45 + Yaw) * Math.PI / 180.0) * 50;
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position), 0.02, GetCenter);
//                    break;
//                case 8:
//                    transformGroup.Children.Add(new RotateTransform3D(
//                         new AxisAngleRotation3D(new Vector3D(0, 0, 1), -10), new Point3D(X_Position, Y_Position, Z_Position)));
//                    if (Yaw <= -179)
//                        Yaw = 170;
//                    else
//                        Yaw -= 10;
//                    break;
//                case 9:
//                    transformGroup.Children.Add(new RotateTransform3D(
//                        new AxisAngleRotation3D(new Vector3D(0, 0, 1), +10), new Point3D(X_Position, Y_Position, Z_Position)));
//                    if (Yaw >= 179)
//                        Yaw = -170;
//                    else
//                        Yaw += 10;
//                    break;
//                case 10:
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position + 40), 0.02, GetCenter);
//                    break;
//                case 11:
//                    Move(new Point3D(X_Position + X, Y_Position + Y, Z_Position - 40), 0.02, GetCenter);
//                    break;
//                default:
//                    break;
//            }
//        }
//        private void Move(Point3D targetPosition, double durationSeconds, Action onComplete)
//        {
            
//            GetCenter();
//            var transformGroup = model.Transform as Transform3DGroup ?? new Transform3DGroup();
//            model.Transform = transformGroup;
//            if (AutoMoving)
//                RotateROV(targetPosition, new Point3D(X_Position, Y_Position, Z_Position));
//            Vector3D movement = targetPosition - new Point3D(X_Position, Y_Position, Z_Position);
//            if ((targetPosition.X) > X_bound || (targetPosition.X) < 310)
//            {
//                movement.X = movement.X > 0 ? X_bound - X_Position : 310 - X_Position;
//            }
//            if ((targetPosition.Y) > Y_bound || (targetPosition.Y) < 310)
//            {
//                movement.Y = movement.Y > 0 ? Y_bound - Y_Position : 310 - Y_Position;
//            }
//            if ((targetPosition.Z) > Z_bound || (targetPosition.Z) < 182)
//            {
//                if (!V_Moving)
//                {
//                    V_Moving = true;
//                    movement.Z = movement.Z > 0 ? Z_bound - Z_Position : 182 - Z_Position;
//                }
//                else
//                    movement.Z = 0;
//            }
//            if (movement.LengthSquared >= 0)
//            {
//                var translateTransform = new TranslateTransform3D();
//                transformGroup.Children.Add(translateTransform);
//                var animationX = new DoubleAnimation(
//                    0, movement.X ,
//                    TimeSpan.FromSeconds(durationSeconds));
//                var animationY = new DoubleAnimation(
//                    0, movement.Y ,
//                    TimeSpan.FromSeconds(durationSeconds));
//                var animationZ = new DoubleAnimation(
//                    0, movement.Z,
//                    TimeSpan.FromSeconds(durationSeconds));

//                int completedCount = 0;
//                EventHandler onAnimationCompleted = null;
                
//                onAnimationCompleted = (s, e) =>
//                {
//                    completedCount++;
//                    if (completedCount == 3) 
//                    {
//                        if (V_Moving)
//                            V_Moving = false;
//                        onComplete?.Invoke();
//                    }
//                };
//                animationX.Completed += onAnimationCompleted;
//                animationY.Completed += onAnimationCompleted;
//                animationZ.Completed += onAnimationCompleted;
//                translateTransform.BeginAnimation(TranslateTransform3D.OffsetXProperty, animationX);
//                translateTransform.BeginAnimation(TranslateTransform3D.OffsetYProperty, animationY);
//                translateTransform.BeginAnimation(TranslateTransform3D.OffsetZProperty, animationZ);
//            }
//        }
//        private void RotateROV(Point3D targetPosition, Point3D currentPosition)
//        {var transformGroup = model.Transform as Transform3DGroup ?? new Transform3DGroup();
//            model.Transform = transformGroup;
//            double angle;
//            Point3D center = new(X_Position, Y_Position, Z_Position);
//            if (targetPosition.X < 310 && x_position <= 310)
//            {
//                transformGroup.Children.Add(new RotateTransform3D(
//                    new AxisAngleRotation3D(new Vector3D(0, 0, 1), -Yaw), center));
//                angle = 90;// targetPosition.X < 310 ? 90 : -90;
//                transformGroup.Children.Add(new RotateTransform3D(
//                    new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle), center));
//                Yaw = angle;
//                return;
//            }
//            Vector3D movement = targetPosition - currentPosition;
//            if (targetPosition.Y > (currentPosition.Y ))
//            {
//                transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(new Vector3D(0, 0, 1), -Yaw), center));
//                angle = Math.Atan2(movement.X, movement.Y) * (180 / Math.PI);
//                angle = (targetPosition.X > currentPosition.X) ? -angle : angle;
//                Yaw = angle;
//            }
//            else if (targetPosition.Y < (currentPosition.Y ))
//            {
//                transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(new Vector3D(0, 0, 1), -180 - Yaw), center));
//                angle = Math.Atan2(Math.Abs(movement.X), Math.Abs(movement.Y)) * (180 / Math.PI);
//                angle = (targetPosition.X > currentPosition.X) ? angle : -angle;
//                Yaw = (currentPosition.X > targetPosition.X) ? 180 + angle : -180 + angle;
//            }
//            else
//            {
//                transformGroup.Children.Add(new RotateTransform3D(
//                new AxisAngleRotation3D(new Vector3D(0, 0, 1), -Yaw), center));
//                angle = (targetPosition.X > currentPosition.X) ? (Math.Abs(Yaw) <= 90) ? -90 : 90 : (Math.Abs(Yaw) <= 90) ? 90 : -90;
//                Yaw = angle;
//            }
//            transformGroup.Children.Add(new RotateTransform3D(
//               new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle), center));
//        }
//        private void GetCenter()
//        {
//            var bounds = model.Bounds;
//            X_Position = bounds.X + bounds.SizeX / 2;
//            Y_Position = bounds.Y + bounds.SizeY / 2;
//            Z_Position = bounds.Z + bounds.SizeZ / 2;
//            var pos = Position;
//            pos.X = Math.Round(X_Position, 2);
//            pos.Y = Math.Round(Y_Position, 2);
//            pos.Z = Math.Round(Z_Position, 2);
//            Position = pos;
//        }
//        ModelVisual3D FindTubeAtPosition(Point3D position, double tolerance = 0.01)
//        {
//            foreach (var child in ROV3DView.Children.OfType<ModelVisual3D>())
//            {
//                if (child.Transform is Transform3DGroup group)
//                {
//                    foreach (var t in group.Children)
//                    {
//                        if (t is TranslateTransform3D translate)
//                        {
//                            if (Math.Abs(translate.OffsetX - position.X) < tolerance &&
//                                Math.Abs(translate.OffsetY - position.Y) < tolerance &&
//                                Math.Abs(translate.OffsetZ - position.Z) < tolerance)
//                            {
//                                return child;
//                            }
//                        }
//                    }
//                }
//            }
//            return null;
//        }
        
//        public event PropertyChangedEventHandler PropertyChanged;
//        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//        public void Dispose()
//        {
//            H_MoveTimer?.Stop();
//            H_MoveTimer?.Dispose();
//        }
//    }
//}