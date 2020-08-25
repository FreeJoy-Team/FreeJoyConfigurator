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
        public ObservableCollection<Button> LogicalButtons { get; private set; }
        public ObservableCollection<Pov> Povs { get; private set; }
        public ObservableCollection<Button> PhysicalButtons { get; private set; }
        public ObservableCollection<Button> ShiftButtons { get; private set; }

        public Joystick(DeviceConfig config)
        {
            _config = config;
            Axes = new ObservableCollection<Axis>();
            for (int i = 0; i < 8; i++)
            {
                Axes.Add(new Axis(i+1, Config.AxisConfig[i]));
            }
            LogicalButtons = new ObservableCollection<Button>();
            for (int i = 0; i < 128; i++)
            {
                LogicalButtons.Add(new Button(i+1));
            }
            Povs = new ObservableCollection<Pov>();
            for (int i=0; i<4; i++)
            {
                Povs.Add(new Pov(0xFF, i));
            }
            PhysicalButtons = new ObservableCollection<Button>();
            for (int i = 0; i < 128; i++)
            {
                PhysicalButtons.Add(new Button(i + 1));
            }
            ShiftButtons = new ObservableCollection<Button>();
            for (int i = 0; i < 5; i++)
            {
                ShiftButtons.Add(new Button(i + 1));
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
        private bool _state;
        private ButtonConfig _config;
        private ObservableCollection<ButtonType> _allowedTypes;
        private ButtonSourceType _sourceType;
        private int _maxPhysicalNumber;

        public int Number { get; private set; }

        public int MaxPhysicalNumber
        {
            get { return _maxPhysicalNumber; }
            set
            {
                SetProperty(ref _maxPhysicalNumber, value);
            }
        }

        public bool State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public ButtonConfig Config
        {
            get { return _config; }
            set { SetProperty(ref _config, value); }
        }

        public ObservableCollection<ButtonType> AllowedTypes
        {
            get { return _allowedTypes; }
            set { SetProperty(ref _allowedTypes, value); }
        }

        public ButtonSourceType SourceType
        {
            get { return _sourceType; }
            set
            {
                SetProperty(ref _sourceType, value);
            }
        }

        public Button(int number)
        {
            _allowedTypes = new ObservableCollection<ButtonType>();
            _allowedTypes.Add(ButtonType.Button_Normal);

            _sourceType = ButtonSourceType.NoSource;

            _config = new ButtonConfig();
            _config.PhysicalNumber = 0;
            _config.Type = ButtonType.Button_Normal;
            
            _state = false;
            Number = number;
        }

        public Button(int number, int physicalNumber)
        {
            _allowedTypes = new ObservableCollection<ButtonType>();
            _allowedTypes.Add(ButtonType.Button_Normal);

            _sourceType = ButtonSourceType.NoSource;

            _config = new ButtonConfig();           
            _config.PhysicalNumber = (sbyte) physicalNumber;
            _config.Type = ButtonType.Button_Normal;
            
            Number = number;
        }
    }

    public class Axis : BindableBase
    {
        private short _value;
        private short _rawValue;
        private AxisConfig _axisConfig;

        private ObservableCollection<AxisSourceType> _allowedSources;
        private byte _maxResolution;
        private bool _isShowAdvanced;

        private bool _isCalibrating;
        private Task _calibrationTask;
        private CancellationTokenSource ts;
        private CancellationToken ct;


        public string CalibrationString
        {
            get
            {
                if (_isCalibrating) return "Stop&Save";
                else return "Calibrate";
            }
        }

        public DelegateCommand CalibrateCommand { get; }
        public DelegateCommand SetCenterCommand { get; }
        public DelegateCommand ResetCalibrationCommand { get; }

        public int Number { get; private set; }

        public short Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
        public short RawValue
        {
            get { return _rawValue; }
            set { SetProperty(ref _rawValue, value); }
        }

        public AxisConfig AxisConfig
        {
            get { return _axisConfig; }
            set { SetProperty(ref _axisConfig, value); }
        }

        public ObservableCollection<AxisSourceType> AllowedSources
        {
            get { return _allowedSources; }
            set { SetProperty(ref _allowedSources, value); }
        }

        public byte MaxResolution
        {
            get { return _maxResolution; }
            set { SetProperty(ref _maxResolution, value); }
        }

        public bool IsShowAdvanced
        {
            get { return _isShowAdvanced; }
            set { SetProperty(ref _isShowAdvanced, value); }
        }

        public Axis(int number, AxisConfig axisConfig)
        {
            _axisConfig = axisConfig;
            _isCalibrating = false;
            Number = number;
            _value = 0;
            _rawValue = 0;

            _allowedSources = new ObservableCollection<AxisSourceType>();
            _allowedSources.Add(AxisSourceType.None);
            _allowedSources.Add(AxisSourceType.Encoder);
            _maxResolution = 16;
            

            CalibrateCommand = new DelegateCommand(() => Calibrate());
            SetCenterCommand = new DelegateCommand(() => SetCenter());
            ResetCalibrationCommand = new DelegateCommand(() => ResetCalibration());
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
                            //AxisConfig.IsCentered = true;
                            //AxisConfig.CalibCenter = RawValue;
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

        public void SetCenter()
        {
            AxisConfig.IsCentered = true;
            AxisConfig.CalibCenter = RawValue;
        }

        public void ResetCalibration()
        {
            AxisConfig.IsCentered = false;
            AxisConfig.CalibCenter = 0;
            AxisConfig.CalibMin = -32767;
            AxisConfig.CalibMax = 32767;
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
