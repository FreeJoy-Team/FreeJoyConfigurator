using HidLibrary;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

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

    public enum PinType : byte
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
        Normal = 0,
        Inverted,
        Toggle,
        ToggleOn,
        ToggleOff,

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
            _type = ButtonType.Normal;
        }

        public ButtonConfig (ButtonType type)
        {
            _type = type;
        }
    }


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

        public UInt16 FirmwareVersion { get; set; }
        public string DeviceName { get; set; }
        public UInt16 ButtonDebounceMs { get; set; }
        public UInt16 TogglePressMs { get; set; }
        public UInt16 EncoderPressMs { get; set; }
        public UInt16 ExchangePeriod { get; set; }
        public ObservableCollection<PinType> PinConfig { get; set; }
        public ObservableCollection<AxisConfig> AxisConfig { get; set; }
        public ObservableCollection<ButtonConfig> ButtonConfig { get; set; }
        public ObservableCollection<EncoderConfig> EncoderConfig { get; set; }

        public DeviceConfig()
        {
            DeviceName = "FreeJoy";
            AxisConfig = new ObservableCollection<AxisConfig>();
            for (int i=0; i<8; i++) AxisConfig.Add(new AxisConfig());

            PinConfig = new ObservableCollection<PinType>();
            for (int i = 0; i < 30; i++) PinConfig.Add(PinType.NotUsed);

            ButtonConfig = new ObservableCollection<ButtonConfig>();
            for (int i = 0; i < 128; i++) ButtonConfig.Add(new ButtonConfig());

                EncoderConfig = new ObservableCollection<EncoderConfig>();
            for (int i = 0; i < 12; i++) EncoderConfig.Add(new EncoderConfig());


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

                    //App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                        ReportConverter.ReportToConfig(ref config, hr);
                    //}));
               
                
                    RaisePropertyChanged(nameof(config));
                    if (configPacketNumber < 10)
                    {
                        buffer[0] = ++configPacketNumber;
                        Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
                        Console.WriteLine("Requesting config packet..: {0}", configPacketNumber);
                    }
                    break;

                case ReportID.CONFIG_OUT_REPORT:
                    configPacketNumber = hr.Data[0];
                    hrs = ReportConverter.ConfigToReports(config);

                    Console.WriteLine("Config packet requested: {0}", configPacketNumber);

                    Hid.ReportSend(hrs[configPacketNumber-1]);
                    Console.WriteLine("Sending config packet..: {0}", configPacketNumber);
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
