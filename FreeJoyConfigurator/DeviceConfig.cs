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
        public bool CutoCalib;
        public bool IsInverted;

        public byte[] CurveShape;
        public FilterLvl FilterLevel;

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
        public AxisConfig[] AxisConfig;
        public PinType[] PinConfig;
        public ButtonType[] Buttons;
        public EncoderConfig EncoderConfig;
        public char[] DeviceName;
        public UInt16 ButtonDebounceMs;
        public UInt16 TogglePressMs;
        public UInt16 EncoderPressMs;
    }
}
