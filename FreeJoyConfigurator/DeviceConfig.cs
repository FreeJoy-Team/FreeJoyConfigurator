using HidLibrary;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
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


    public class EncoderConfig
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

    public class DeviceConfig
    {
        private Hid hid;
        private DeviceConfig config;
        private byte configPacketNumber = 0;
        private ConfigReport configReport;

        public UInt16 FirmwareVersion { get; set; }
        public string DeviceName { get; set; }
        public UInt16 ButtonDebounceMs { get; set; }
        public UInt16 TogglePressMs { get;  set; }
        public UInt16 EncoderPressMs { get;  set; }
        public UInt16 ExchangePeriod { get;  set; }
        public PinType[] PinConfig { get;  set; }
        public AxisConfig[] AxisConfig { get;  set; }
        public ButtonType[] Buttons { get;  set; }
        public EncoderConfig[] EncoderConfig { get;  set; }


        public DeviceConfig()
        {
            AxisConfig = new AxisConfig[8];
            PinConfig = new PinType[30];
            Buttons = new ButtonType[128];
            EncoderConfig = new EncoderConfig[12];

            hid = new Hid();
            hid.PacketReceived += PacketReceivedEventHandler;
        }

        public void PacketReceivedEventHandler(object sender, HidReport report)
        {
            HidReport hr = report;

            if ((ReportID)hr.ReportId == (ReportID.CONFIG_REPORT))
            {
                configPacketNumber = hr.Data[0];
                configReport = new ConfigReport(ref config, hr);
                if (configPacketNumber < 10)
                {
                    hid.ReportSend((byte)ReportID.CONFIG_REPORT, new byte[1] { ++configPacketNumber });
                }
            }
        }
    }
}
