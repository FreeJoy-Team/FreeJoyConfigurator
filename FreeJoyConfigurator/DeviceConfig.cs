using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{

    public class AxisConfig
    {
        public enum FilterLvl
        {
            FilterNo = 0,
            FilterLow,
            FilterMedium,
            FilterHigh,
        };

        public UInt16 CalibMin;
        public UInt16 CalibCenter;
        public UInt16 CalibMax;
        public bool AutoCalib;
        public bool IsInverted;

        public byte[] CurveShape;
        public FilterLvl FilterLevel;

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
        public UInt16 FirmwareVersion;
        public string DeviceName;
        public UInt16 ButtonDebounceMs;
        public UInt16 TogglePressMs;
        public UInt16 EncoderPressMs;
        public UInt16 ExchangePeriod;
        public PinType[] PinConfig;

        public AxisConfig[] AxisConfig;
        
        public ButtonType[] Buttons;

        public EncoderConfig[] EncoderConfig;


        public DeviceConfig()
        {
            AxisConfig = new AxisConfig[8];
            PinConfig = new PinType[30];
            Buttons = new ButtonType[128];
            EncoderConfig = new EncoderConfig[12];
        }
    }
}
