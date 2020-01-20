using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HidLibrary;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using System.Threading;

namespace FreeJoyConfigurator
{
    public class Joystick : BindableBase
    {
        private DeviceConfig _config;

        public DeviceConfig Config
        {
            get
            {
                return _config;
            }
            set
            {
                SetProperty(ref _config, value);
            }
        }
        public ObservableCollection<Axis> Axes { get; private set; }
        public ObservableCollection<Button> Buttons { get; private set; }
        public ObservableCollection<Pov> Povs { get; private set; }

        public Joystick(DeviceConfig config)
        {
            _config = config;
            Axes = new ObservableCollection<Axis>();
            for (int i = 0; i < 8; i++)
            {
                Axes.Add(new Axis(i+1, Config.AxisConfig[i]));
                if(config.PinConfig[i] == PinType.AxisAnalog)
                {
                    Axes[i].IsEnabled = true;
                }
                else Axes[i].IsEnabled = false;
            }
            Buttons = new ObservableCollection<Button>();
            for (int i = 0; i < 128; i++)
            {
                Buttons.Add(new Button(i+1));
            }
            Povs = new ObservableCollection<Pov>();
            for (int i=0; i<4; i++)
            {
                Povs.Add(new Pov(0xFF, i));
            }

            Hid.PacketReceived += PacketReceivedEventHandler;
        }

        public void PacketReceivedEventHandler(HidReport report)
        {
            var joystick = this;
            HidReport hr = report;

            if ((ReportID)hr.ReportId == (ReportID.JOY_REPORT))
            {
                ReportConverter.ReportToJoystick(ref joystick, hr);
                RaisePropertyChanged(nameof(joystick));
            }          
        }

    }

    public class Button : BindableBase
    {
        private ButtonType _type;
        private ObservableCollection<ButtonType> _allowedTypes;
        private bool _state;
        public int Number { get; private set; }

        public ButtonType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        public ObservableCollection<ButtonType> AllowedTypes
        {
            get { return _allowedTypes; }
            set { SetProperty(ref _allowedTypes, value); }
        }

        public bool State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public Button(int number)
        {
            Number = number;
            _type = ButtonType.BtnNormal;
            _allowedTypes = new ObservableCollection<ButtonType>()
            {       ButtonType.BtnInverted,
                    ButtonType.BtnNormal,
                    ButtonType.BtnToggle,
                    ButtonType.ToggleSw,
                    ButtonType.ToggleSwOff,
                    ButtonType.ToggleSwOn,
                    ButtonType.Pov1Down,
                    ButtonType.Pov1Left,
                    ButtonType.Pov1Right,
                    ButtonType.Pov1Up,
                    ButtonType.Pov2Down,
                    ButtonType.Pov2Left,
                    ButtonType.Pov2Right,
                    ButtonType.Pov2Up,
                    ButtonType.Pov3Down,
                    ButtonType.Pov3Left,
                    ButtonType.Pov3Right,
                    ButtonType.Pov3Up,
                    ButtonType.Pov4Down,
                    ButtonType.Pov4Left,
                    ButtonType.Pov4Right,
                    ButtonType.Pov4Up,
                    ButtonType.Encoder_A,
                    ButtonType.Encoder_B
            };

            _state = false;
        }

        public Button(bool state, int number)
        {
            Number = number;
            _type = ButtonType.BtnNormal;
            _allowedTypes = new ObservableCollection<ButtonType>()
            {       ButtonType.BtnInverted,
                    ButtonType.BtnNormal,
                    ButtonType.BtnToggle,
                    ButtonType.ToggleSw,
                    ButtonType.ToggleSwOff,
                    ButtonType.ToggleSwOn,
                    ButtonType.Pov1Down,
                    ButtonType.Pov1Left,
                    ButtonType.Pov1Right,
                    ButtonType.Pov1Up,
                    ButtonType.Pov2Down,
                    ButtonType.Pov2Left,
                    ButtonType.Pov2Right,
                    ButtonType.Pov2Up,
                    ButtonType.Pov3Down,
                    ButtonType.Pov3Left,
                    ButtonType.Pov3Right,
                    ButtonType.Pov3Up,
                    ButtonType.Pov4Down,
                    ButtonType.Pov4Left,
                    ButtonType.Pov4Right,
                    ButtonType.Pov4Up,
                    ButtonType.Encoder_A,
                    ButtonType.Encoder_B
            };
            _state = state;
        }
        public Button(bool state, ButtonType type, int number)
        {
            Number = number;
            _type = type;
            _allowedTypes = new ObservableCollection<ButtonType>()
            {       ButtonType.BtnInverted,
                    ButtonType.BtnNormal,
                    ButtonType.BtnToggle,
                    ButtonType.ToggleSw,
                    ButtonType.ToggleSwOff,
                    ButtonType.ToggleSwOn,
                    ButtonType.Pov1Down,
                    ButtonType.Pov1Left,
                    ButtonType.Pov1Right,
                    ButtonType.Pov1Up,
                    ButtonType.Pov2Down,
                    ButtonType.Pov2Left,
                    ButtonType.Pov2Right,
                    ButtonType.Pov2Up,
                    ButtonType.Pov3Down,
                    ButtonType.Pov3Left,
                    ButtonType.Pov3Right,
                    ButtonType.Pov3Up,
                    ButtonType.Pov4Down,
                    ButtonType.Pov4Left,
                    ButtonType.Pov4Right,
                    ButtonType.Pov4Up,
                    ButtonType.Encoder_A,
                    ButtonType.Encoder_B
            };
            _state = state;
        }

        public Button(bool state, ButtonType type, ObservableCollection<ButtonType> allowedTypes, int number)
        {
            Number = number;
            _type = type;
            _allowedTypes = allowedTypes;
            _state = state;
        }
    }

    public class Axis : BindableBase
    {
        private ushort _value;
        private ushort _rawValue;
        private bool _isEnabled;
        private AxisConfig _axisConfig;
        private bool _isCalibrating;
        private Task _calibrationTask;
        private CancellationTokenSource ts;
        private CancellationToken ct;

        public string CalibrationString
        {
            get
            {
                if (_isCalibrating) return "Stop calibration";
                else return "Start calibration";
            }
        }

        public DelegateCommand CalibrateCommand { get; }

        public int Number { get; private set; }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public ushort Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
        public ushort RawValue
        {
            get { return _rawValue; }
            set { SetProperty(ref _rawValue, value); }
        }

        public AxisConfig AxisConfig
        {
            get { return _axisConfig; }
            set { SetProperty(ref _axisConfig, value); }
        }

        public Axis(int number, AxisConfig axisConfig)
        {
            _axisConfig = axisConfig;
            _isCalibrating = false;
            Number = number;
            _value = 0;
            _rawValue = 0;

            CalibrateCommand = new DelegateCommand(() => Calibrate());
        }


        public void Calibrate()
        {
            _isCalibrating = !_isCalibrating;
            RaisePropertyChanged(nameof(CalibrationString));

            if (_isCalibrating)
            {
                // start task
                ts = new CancellationTokenSource();
                ct = ts.Token;
                _calibrationTask = Task.Factory.StartNew(() =>
                {
                    AxisConfig.CalibMax = RawValue;
                    AxisConfig.CalibMin = RawValue;
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            AxisConfig.IsCalibCenterUnlocked = true;
                            AxisConfig.CalibCenter = RawValue;
                            break;
                        }
                        CalibrationTask();
                        //Thread.Sleep(10);
                    }
                });

            }
            else
            {
                // stop task
                ts.Cancel();
            }
        }

        private void CalibrationTask()
        {    
                    if (AxisConfig.CalibMax < RawValue)
                    {
                        AxisConfig.CalibMax = RawValue;
                    }
                    if (AxisConfig.CalibMin > RawValue)
                    {
                        AxisConfig.CalibMin = RawValue;
                    }     
        }

    }

    public class Pov : BindableBase
    {
            private byte _state;
            public int Number { get; private set; }

            public byte State
            {
                get { return _state; }
                set { SetProperty(ref _state, value); }
            }

            public Pov(byte state, int number)
            {
                Number = number;
                _state = state;
            }
        
    }


}
