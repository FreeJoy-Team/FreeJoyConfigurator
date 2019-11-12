using HidLibrary;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace FreeJoyConfigurator
{
    
    public class AxisConfig : BindableBase
    {
        public enum FilterLvl : byte
        {
            FilterNo = 0,
            FilterLow,
            FilterMedium,
            FilterHigh,
        };

        private ushort _calibMin;
        private ushort _calibCenter;
        private ushort _calibMax;
        private bool _isAutocalib;
        private bool _isInverted;
        private byte[] _curveShape;
        private FilterLvl _filterLevel;

        private bool _isCalibCenterUnlocked;

        public ushort CalibMin
        {
            get
            {
                return _calibMin;
            }
            set
            {
                if (value < 0) SetProperty(ref _calibMin, (ushort)0);
                else if (value >= CalibCenter && IsCalibCenterUnlocked) SetProperty(ref _calibMin, (ushort)(CalibCenter - 1));
                else if (value >= CalibMax) SetProperty(ref _calibMin, (ushort)(CalibMax - 2));
                else SetProperty(ref _calibMin, value);

                if (!IsCalibCenterUnlocked)
                {
                    CalibCenter = (ushort)((CalibMax - CalibMin) / 2);
                }
            }
        }
        public ushort CalibCenter
        {
            get
            {
                return _calibCenter;
            }
            set
            {
                if (IsCalibCenterUnlocked)
                {
                    if (value <= CalibMin)
                    {
                        SetProperty(ref _calibCenter, (ushort)(CalibMin + 1));
                    }
                    else if (value >= CalibMax)
                    {
                        SetProperty(ref _calibCenter, (ushort)(CalibMax - 1));
                    }
                    else SetProperty(ref _calibCenter, value);
                }
                else
                {
                    SetProperty(ref _calibCenter, (ushort)((CalibMax - CalibMin) / 2));
                }
            }
        }
        public ushort CalibMax
        {
            get
            {
                return _calibMax;
            }
            set
            {
                if (value <= CalibMin) SetProperty(ref _calibMax, (ushort)(CalibMin + 2));
                else if (value <= CalibCenter && IsCalibCenterUnlocked) SetProperty(ref _calibMax, (ushort)(CalibCenter + 1));
                else if (value > 4095) SetProperty(ref _calibMax, (ushort) 4095);
                else SetProperty(ref _calibMax, value);

                if (!IsCalibCenterUnlocked)
                {
                    CalibCenter = (ushort)((CalibMax - CalibMin) / 2);
                }
            }
        }
      
        public bool IsAutoCalib
        {
            get
            {
                return _isAutocalib;
            }
            set
            {
                SetProperty(ref _isAutocalib, value);
            }
        }
      
        public bool IsInverted
        {
            get
            {
                return _isInverted;
            }
            set
            {
                SetProperty(ref _isInverted, value);
            }
        }

        public byte[] CurveShape
        {
            get
            {
                return _curveShape;
            }
            set
            {
                SetProperty(ref _curveShape, value);
            }
        }

        public FilterLvl FilterLevel
        {
            get
            {
                return _filterLevel;
            }
            set
            {
                SetProperty(ref _filterLevel, value);
            }
        }

        public bool IsCalibCenterUnlocked
        {
            get
            {
                return _isCalibCenterUnlocked;
            }
            set
            {
                SetProperty(ref _isCalibCenterUnlocked, value);
                if (!_isCalibCenterUnlocked)
                {
                    CalibCenter = (ushort)((CalibMax - CalibMin) / 2);
                }
            }
        }

        public AxisConfig()
        {
            _calibMin = 0;
            _calibCenter = 2047;
            _calibMax = 4095;
            _isAutocalib = false;
            _isInverted = false;

            _curveShape = new byte[10];
            _filterLevel = FilterLvl.FilterNo;

            _isCalibCenterUnlocked = false;
        }

    }

    public enum PinType : byte
    {
        NotUsed = 0,
        ButtonGnd,
        ButtonVcc,
        ButtonRow,
        ButtonColumn,

        AxisAnalog,
        //AxisToButtons,

        //EncoderSingleInput,
        //EncoderChainedInput,
        //EncoderChainedCommon,
    };


    public enum ButtonType
    {
        BtnNormal = 0,
        BtnInverted,
        BtnToggle,
        ToggleSw,
        ToggleSwOn,
        ToggleSwOff,

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
        Pov3Down,
        Pov3Left,
        Pov4Up,
        Pov4Right,
        Pov4Down,
        Pov4Left,

        Encoder_A,
        Encoder_B,
        //BtnToAnalog,
        //Shift,
    };

    public class ButtonConfig : BindableBase
    {
        private ButtonType _type;
        public ButtonType Type
        {
            get
            {
                return _type;
            }
            set
            {
                SetProperty(ref _type, value);
            }
        }

        public ButtonConfig()
        {
            _type = ButtonType.BtnNormal;
        }

        public ButtonConfig (ButtonType type)
        {
            _type = type;
        }
    }

    public class DeviceConfig : BindableBase
    {
        [XmlIgnore]
        static private byte configPacketNumber = 0;

        [XmlElement("Firmware_Version")]
        public UInt16 FirmwareVersion { get; set; }
        [XmlElement("Device_Name")]
        public string DeviceName { get; set; }
        [XmlElement("Button_Debounce_Time")]
        public UInt16 ButtonDebounceMs { get; set; }
        [XmlElement("Toggle_Press_Time")]
        public UInt16 TogglePressMs { get; set; }
        [XmlElement("Encoder_Press_Time")]
        public UInt16 EncoderPressMs { get; set; }
        [XmlElement("Exchange_Period")]
        public UInt16 ExchangePeriod { get; set; }
        [XmlElement("Pin_Config")]
        public ObservableCollection<PinType> PinConfig { get; set; }
        [XmlElement("Axis_Config")]
        public ObservableCollection<AxisConfig> AxisConfig { get; set; }
        [XmlElement("Button_Config")]
        public ObservableCollection<ButtonConfig> ButtonConfig { get; set; }

        public delegate void ConfigReceivedEventHandler(DeviceConfig deviceConfig);

        public delegate void ConfigSentEventHandler(DeviceConfig deviceConfig);

        public event ConfigReceivedEventHandler Received;
        public event ConfigSentEventHandler Sent;

        public DeviceConfig()
        {
            DeviceName = "FreeJoy";
            ButtonDebounceMs = 50;
            TogglePressMs = 300;
            EncoderPressMs = 100;
            ExchangePeriod = 10;

            AxisConfig = new ObservableCollection<AxisConfig>();
            for (int i=0; i<8; i++) AxisConfig.Add(new AxisConfig());

            PinConfig = new ObservableCollection<PinType>();
            for (int i = 0; i < 30; i++) PinConfig.Add(PinType.NotUsed);

            ButtonConfig = new ObservableCollection<ButtonConfig>();
            for (int i = 0; i < 128; i++) ButtonConfig.Add(new ButtonConfig());


            //Hid.Connect();
            Hid.PacketReceived += PacketReceivedEventHandler;
        }

        public void PacketReceivedEventHandler(HidReport report)
        {
            var config = this;
            List<HidReport> hrs;
            HidReport hr = report;
            byte[] buffer = new byte[1];

            switch ((ReportID)hr.ReportId)               
            {
                case ReportID.CONFIG_IN_REPORT:
                    configPacketNumber = hr.Data[0];
                    Console.WriteLine("Config packet received: {0}", configPacketNumber);

                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ReportConverter.ReportToConfig(ref config, hr);
                    }));
               
                    if (configPacketNumber < 10)
                    {
                        buffer[0] = ++configPacketNumber;
                        Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
                        Console.WriteLine("Requesting config packet..: {0}", configPacketNumber);
                    }
                    else
                    {

                        RaisePropertyChanged(nameof(config));
                        App.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Received(this);
                        }));
                    }
                    break;

                case ReportID.CONFIG_OUT_REPORT:
                    configPacketNumber = hr.Data[0];
                    hrs = ReportConverter.ConfigToReports(config);

                    Console.WriteLine("Config packet requested: {0}", configPacketNumber);

                    Hid.ReportSend(hrs[configPacketNumber-1]);
                    Console.WriteLine("Sending config packet..: {0}", configPacketNumber);

                    if (configPacketNumber >= 10)
                    {
                        App.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Sent(this);
                        }));
                    }
                    break;

                default:
                    break;
            }
        }

        public void GetConfigRequest()
        {
            byte[] buffer = new byte[1];

            buffer[0] = 1;
            Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
            Console.WriteLine("Requesting config packet..: 1");
        }

        public void SendConfig()
        {
            var config = this;
            List<HidReport> hr;

            hr = ReportConverter.ConfigToReports(config);

            Hid.ReportSend(hr[0]);
            Console.WriteLine("Sending config packet..: 1");
        }
    }
}
