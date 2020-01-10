using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace FreeJoyConfigurator
{
    

    public class AxesCurvesVM : BindableBase
    {
        const double FullScaleX = 750;
        const double FullScaleY = 510;

        #region Fields
        public DeviceConfig Config { get; set; }
        

        private ObservableCollection<Curve> _curves;

        public ObservableCollection<Curve> Curves
        {
            get
            {
                return _curves;
            }
            set
            {
                SetProperty(ref _curves, value);
            }
        }
        #endregion

        #region Constructor
        public AxesCurvesVM(DeviceConfig config)
        {          
            Config = config;

            _curves = new ObservableCollection<Curve>();

            for (int i = 0; i < Config.AxisConfig.Count; i++)
            {
                Curve tmp = new Curve(FullScaleX, FullScaleY, i+1);

                for (int j = 0; j < Config.AxisConfig[i].CurveShape.Count; j++)
                {
                    tmp.Points.Add(new Point((int)(Config.AxisConfig[i].CurveShape[j].X * FullScaleX / 9),
                                        (int)((FullScaleY / 2) + (FullScaleY / 2) * Config.AxisConfig[i].CurveShape[j].Y / 100)));
                    
                }
                foreach (var item in tmp.Points)
                {
                    tmp.Sliders.Add(item);
                }

                for (int j = tmp.Points.Count - 1; j >= 0; j--)
                {
                    double delta = (tmp.Points[1].X - tmp.Points[0].X);
                    double delta_ref = delta/3;

                    if (j == tmp.Points.Count - 1)
                    {
                        double koeff = FindRefKoeff(tmp.Points[j - 1], tmp.Points[j], delta);
                        Point left = GetLeftRefPoint(koeff, delta_ref, tmp.Points[j]);
                        tmp.Points.Insert(j, left);
                    }
                    else if (j == 0)
                    {
                        double koeff = FindRefKoeff(tmp.Points[j], tmp.Points[j+1], delta);
                        Point right = GetRightRefPoint(koeff, delta_ref, tmp.Points[j]);
                        tmp.Points.Insert(j+1, right);
                    }
                    else
                    {                      
                        double koeff = FindRefKoeff(tmp.Points[j - 1], tmp.Points[j], tmp.Points[j + 1], delta);
                        Point right = GetRightRefPoint(koeff, delta_ref, tmp.Points[j]);
                        Point left = GetLeftRefPoint(koeff, delta_ref, tmp.Points[j]);

                        tmp.Points.Insert(j + 1, right);
                        tmp.Points.Insert(j, left);
                    }
                }

                _curves.Add(tmp);
                _curves[i].Sliders.CollectionChanged += Sliders_CollectionChanged;
            }
        }
        #endregion


        #region Functions
        public void Update(DeviceConfig config)
        {
            Config = config;

            for (int i = 0; i < Config.AxisConfig.Count; i++)
            {
                Curve tmp = new Curve(FullScaleX, FullScaleY, i+1);

                for (int j = 0; j < Config.AxisConfig[i].CurveShape.Count; j++)
                {
                    tmp.Points.Add(new Point((int)(Config.AxisConfig[i].CurveShape[j].X * FullScaleX / 9),
                                        (int)((FullScaleY / 2) + (FullScaleY / 2) * Config.AxisConfig[i].CurveShape[j].Y / 100)));

                }
                foreach (var item in tmp.Points)
                {
                    tmp.Sliders.Add(item);
                }

                for (int j = tmp.Points.Count - 1; j >= 0; j--)
                {
                    double delta = (tmp.Points[1].X - tmp.Points[0].X);
                    double delta_ref = delta / 3;

                    if (j == tmp.Points.Count - 1)
                    {
                        double koeff = FindRefKoeff(tmp.Points[j - 1], tmp.Points[j], delta);
                        Point left = GetLeftRefPoint(koeff, delta_ref, tmp.Points[j]);
                        tmp.Points.Insert(j, left);
                    }
                    else if (j == 0)
                    {
                        double koeff = FindRefKoeff(tmp.Points[j], tmp.Points[j + 1], delta);
                        Point right = GetRightRefPoint(koeff, delta_ref, tmp.Points[j]);
                        tmp.Points.Insert(j + 1, right);
                    }
                    else
                    {
                        double koeff = FindRefKoeff(tmp.Points[j - 1], tmp.Points[j], tmp.Points[j + 1], delta);
                        Point right = GetRightRefPoint(koeff, delta_ref, tmp.Points[j]);
                        Point left = GetLeftRefPoint(koeff, delta_ref, tmp.Points[j]);

                        tmp.Points.Insert(j + 1, right);
                        tmp.Points.Insert(j, left);
                    }
                }

                _curves[i] = tmp;
                _curves[i].Sliders.CollectionChanged += Sliders_CollectionChanged;
                
            }
        }

        private void Sliders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // update model
            DeviceConfig conf = Config;
            for (int i = 0; i < conf.AxisConfig.Count; i++)
            {
                for (int j = 0; j < conf.AxisConfig[i].CurveShape.Count; j++)
                {
                    Point tmp = new Point(j, (int)((_curves[i].Sliders[j].Y - (FullScaleY / 2)) * 100 / (FullScaleY / 2)));

                    conf.AxisConfig[i].CurveShape[j] = tmp;
                }
            }
            Config = conf;

            // update bindings
            for (int i = 0; i < Config.AxisConfig.Count; i++)
            {
                //Curve tmp = new Curve(FullScaleX, FullScaleY);

                for (int j = 0; j < Config.AxisConfig[i].CurveShape.Count; j++)
                {
                    _curves[i].Points[j*3] = new Point((int)(Config.AxisConfig[i].CurveShape[j].X * FullScaleX / 9),
                                        (int)((FullScaleY / 2) + (FullScaleY / 2) * Config.AxisConfig[i].CurveShape[j].Y / 100));
                }
                for (int j = 0; j<_curves[i].Points.Count; j+=3)
                {
                    double delta = (_curves[i].Points[3].X - _curves[i].Points[0].X);
                    double delta_ref = delta / 3;

                    if (j == _curves[i].Points.Count - 1)
                    {
                        double koeff = FindRefKoeff(_curves[i].Points[j - 3], _curves[i].Points[j], delta);
                        Point left = GetLeftRefPoint(koeff, delta_ref, _curves[i].Points[j]);
                        _curves[i].Points[j-1] = left;
                    }
                    else if (j == 0)
                    {
                        double koeff = FindRefKoeff(_curves[i].Points[j], _curves[i].Points[j + 3], delta);
                        Point right = GetRightRefPoint(koeff, delta_ref, _curves[i].Points[j]);
                        _curves[i].Points[j+1] = right;
                    }
                    else
                    {
                        double koeff = FindRefKoeff(_curves[i].Points[j - 3], _curves[i].Points[j], _curves[i].Points[j + 3], delta);
                        Point right = GetRightRefPoint(koeff, delta_ref, _curves[i].Points[j]);
                        Point left = GetLeftRefPoint(koeff, delta_ref, _curves[i].Points[j]);

                        _curves[i].Points[j + 1] = right;
                        _curves[i].Points[j - 1] = left;
                    }
                }
                //_curves[i] = tmp;
            }
        }


        double FindRefKoeff (Point point1, Point point2, Point point3, double delta)
        {
            double koeff = 0;

            if ((point3.Y - point1.Y) == 0) koeff = 0;
            else
            {
                koeff = (Math.Sqrt((Math.Pow((point2.Y - point1.Y), 2) + Math.Pow(delta, 2)) * (Math.Pow((point2.Y - point3.Y), 2) + Math.Pow(delta, 2))) -
                        Math.Pow(delta, 2) - (point2.Y - point1.Y) * (point2.Y - point3.Y)) / ((point3.Y - point1.Y) * delta);
            }
            return koeff;
        }
        double FindRefKoeff(Point point1, Point point2, double delta)
        {
            return (point2.Y - point1.Y) / delta; ;
        }


        Point GetLeftRefPoint (double koeff, double delta, Point point)
        {
            return new Point(point.X - delta, point.Y - koeff * delta);
        }

        Point GetRightRefPoint(double koeff, double delta, Point point)
        {
            return new Point(point.X + delta, point.Y + koeff * delta);
        }
        #endregion

    }

    public class Curve : BindableBase
    {
        public DelegateCommand SetLinearCommand { get; }
        public DelegateCommand SetExponent1Command { get; }
        public DelegateCommand SetExponent2Command { get; }
        public DelegateCommand SetShapeCommand { get; }

        public double MaxX { get; private set; }
        public double MaxY { get; private set; }
        public int Number { get; private set; }

        private ObservableCollection<Point> _points;
        private ObservableCollection<Point> _sliders;

        public bool IsSymmetric { get; set; }

        public ObservableCollection<Point> Points
        {
            get
            {
                return _points;
            }
            set
            {                
                SetProperty(ref _points, value);               
            }
        }

        public ObservableCollection<Point> Sliders
        {
            get
            {
                return _sliders;
            }
            set
            {
                SetProperty(ref _sliders, value);
            }
        }

        public Curve(double maxX, double maxY, int number)
        {
            this.MaxX = maxX;
            this.MaxY = maxY;
            Number = number;
            _points = new ObservableCollection<Point>();
            _sliders = new ObservableCollection<Point>();

            SetLinearCommand = new DelegateCommand(() => SetPointsLinear());
            SetExponent1Command = new DelegateCommand(() => SetPointsExponent1());
            SetExponent2Command = new DelegateCommand(() => SetPointsExponent2());
            SetShapeCommand = new DelegateCommand(() => SetPointsShape());
        }

        void SetPointsLinear()
        {
            for (int i=0; i<Sliders.Count; i++)
            {
                Sliders[i] = new Point(i, i * MaxY / (Sliders.Count - 1));
            }
        }

        void SetPointsExponent1()
        {
            for (int i = 0; i < Sliders.Count; i++)
            {
                Sliders[i] = new Point(i, Math.Exp(i * Math.Log(MaxY + 1) / (Sliders.Count - 1)) - 1);
            }
        }

        void SetPointsExponent2()
        {
            for (int i = 0; i < Sliders.Count; i++)
            {
                Sliders[(Sliders.Count - 1) -i] = new Point((Sliders.Count - 1)-i, MaxY - Math.Exp(i * Math.Log(MaxY + 1) / (Sliders.Count - 1)) - 1);
            }
        }

        void SetPointsShape()
        {
            Sliders[0] = new Point(0, 0);
            Sliders[1] = new Point(1, 0.25 * MaxY);
            Sliders[2] = new Point(2, 0.40 * MaxY);
            Sliders[3] = new Point(3, 0.47 * MaxY);
            Sliders[4] = new Point(4, 0.49 * MaxY);
            Sliders[5] = new Point(5, 0.51 * MaxY);
            Sliders[6] = new Point(6, 0.53 * MaxY);
            Sliders[7] = new Point(7, 0.60 * MaxY);
            Sliders[8] = new Point(8, 0.75 * MaxY);
            Sliders[9] = new Point(9, MaxY);
        }
    }

    public class PointValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Point val = (Point)value;
            return val.Y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Point(Double.Parse((string)parameter), (double)value);
        }
    }

}
