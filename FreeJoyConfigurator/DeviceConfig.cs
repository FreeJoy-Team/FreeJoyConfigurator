using HidLibrary;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{

    public class AxisConfig : BindableBase
    {
        public enum FilterLvl
        {
            FilterNo = 0,
            FilterLow,
            FilterMedium,
            FilterHigh,
        };

        public UInt16 CalibMin { get; set; }
        public UInt16 CalibCenter { get; set; }
        public UInt16 CalibMax { get; set; }
        public bool AutoCalib { get; set; }
        public bool IsInverted { get; set; }

        public byte[] CurveShape { get; set; }
        public FilterLvl FilterLevel { get; set; }

        public AxisConfig()
        {
            CalibMin = 0;
            CalibCenter = 2047;
            CalibMax = 4095;
            AutoCalib = false;
            IsInverted = false;

            CurveShape = new byte[10];
            FilterLevel = FilterLvl.FilterNo;
        }

    }

    public enum PinType
    {
        NotUsed = 0,
        ButtonGnd,
        ButtonVcc,
        ButtonRow,
        ButtonColumn,

        AxisAnalog,
        AxisToButtons,

        EncoderSingleInput,
        EncoderChainedInput,
        EncoderChainedCommon,
    };


    public enum ButtonType
    {
        ButtonNormal = 0,
        ButtonInverted,
        ButtonToggle,
        ButtonToggleOn,
        ButtonToggleOff,

        Pov1Up,
        Pov1Right,
        Pov1Down,
        Pov1Left,
        Pov2Up,
        Pov2Right,
        Pov2Down,
        Pov2Left,
        Pov3Up,
        Pov3Right,
        Pov31Down,
        Pov3Left,
        Pov4Up,
        Pov4Right,
        Pov4Down,
        Pov4Left,

        ButtonToAnalog,
        ButtonShift,
    };


    public class EncoderConfig : BindableBase
    {
        public enum EncoderType
        {
            Encoder1_1 = 0,
            Encoder1_2,
            Encoder1_4,
        };

        public byte PinA;
        public byte PinB;
        public byte PinC;
        public EncoderType Type;
    }

    public class DeviceConfig : BindableBase
    {
        static private byte configPacketNumber = 0;

        private UInt16 _firmwareVersion;
        private string _deviceName;
        private UInt16 _buttonDebounceMs;
        private UInt16 _togglePressMs;
        private UInt16 _encoderPressMs;
        private UInt16 _exchangePeriod;
        private ObservableCollection<PinType> _pinConfig;
        private ObservableCollection<AxisConfig> _axisConfig;
        private ObservableCollection<ButtonType> _buttons;
        private ObservableCollection<EncoderConfig> _encoderConfig;

        public UInt16 FirmwareVersion
        {
            get { return _firmwareVersion; }
            set { SetProperty(ref _firmwareVersion, value); }
        }
        public string DeviceName
        {
            get { return _deviceName; }
            set { SetProperty(ref _deviceName, value); }
        }
        public UInt16 ButtonDebounceMs
        {
            get { return _buttonDebounceMs; }
            set { SetProperty(ref _buttonDebounceMs, value); }
        }
        public UInt16 TogglePressMs
        {
            get { return _togglePressMs; }
            set { SetProperty(ref _togglePressMs, value); }
        }
        public UInt16 EncoderPressMs
        {
            get { return _encoderPressMs; }
            set { SetProperty(ref _encoderPressMs, value); }
        }
        public UInt16 ExchangePeriod
        {
            get { return _exchangePeriod; }
            set { SetProperty(ref _exchangePeriod, value); }
        }
        public ObservableCollection<PinType> PinConfig
        {
            get {
                return _pinConfig;
            }
            set
            {
                SetProperty(ref _pinConfig, value);
            }
        }
        public ObservableCollection<AxisConfig> AxisConfig
        {
            get { return _axisConfig; }
            set { SetProperty(ref _axisConfig, value); }
        }
        public ObservableCollection<ButtonType> Buttons
        {
            get { return _buttons; }
            set { SetProperty(ref _buttons, value); }
        }
        public ObservableCollection<EncoderConfig> EncoderConfig
        {
            get { return _encoderConfig; }
            set { SetProperty(ref _encoderConfig, value); }
        }


        public DeviceConfig()
        {
            _deviceName = "FreeJoy";
            _axisConfig = new ObservableCollection<AxisConfig>();
            for (int i=0; i<8; i++) _axisConfig.Add(new AxisConfig());

            _pinConfig = new ObservableCollection<PinType>();
            for (int i = 0; i < 30; i++) _pinConfig.Add(PinType.NotUsed);

            _buttons = new ObservableCollection<ButtonType>();
            for (int i = 0; i < 128; i++) _buttons.Add(ButtonType.ButtonNormal);

                _encoderConfig = new ObservableCollection<EncoderConfig>();
            for (int i = 0; i < 12; i++) _encoderConfig.Add(new EncoderConfig());


            //Hid.Connect();
            Hid.PacketReceived += PacketReceivedEventHandler;
        }

        public void PacketReceivedEventHandler(HidReport report)
        {
            var config = this;
            HidReport hr = report;
            byte[] buffer = new byte[1];

            if ((ReportID)hr.ReportId == (ReportID.CONFIG_IN_REPORT))
            {
                configPacketNumber = hr.Data[0];
                Console.WriteLine("Config packet received: {0}", configPacketNumber);

                ReportConverter.ReportToConfig(ref config, hr);
                RaisePropertyChanged(nameof(config));
                if (configPacketNumber < 10)
                {
                    buffer[0] = ++configPacketNumber;
                    Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
                }           
            }
        }

        public void GetConfigRequest()
        {
            byte[] buffer = new byte[1];

            buffer[0] = 1;
            Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
            Console.WriteLine("Config requested");
        }

        public void SendConfig()
        {
            var config = this;
            List<HidReport> hr;

            hr = ReportConverter.ConfigToReports(config);

            Hid.ReportSend((byte)ReportID.CONFIG_OUT_REPORT, hr[0].Data);
        }
    }
}
