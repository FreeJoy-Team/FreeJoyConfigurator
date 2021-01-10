using HidLibrary;
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
using System.Xml.Serialization;

namespace FreeJoyConfigurator
{

    public enum AxisSourceType : sbyte
    {
        Encoder = -3,
        I2C = -2,
        None = -1,
        A0 = 0,
        A1,
        A2,
        A3,
        A4,
        A5,
        A6,
        A7,
        A8,
        A9,
        A10,
        A15,
        B0,
        B1,
        B3,
        B4,
        B5,
        B6,
        B7,
        B8,
        B9,
        B10,
        B11,
        B12,
        B13,
        B14,
        B15,
        C13,
        C14,
        C15,
    };

    public enum ShiftRegSourceType : sbyte
    {
        NotDefined = -1,
        A0 = 0,
        A1,
        A2,
        A3,
        A4,
        A5,
        A6,
        A7,
        A8,
        A9,
        A10,
        A15,
        B0,
        B1,
        B3,
        B4,
        B5,
        B6,
        B7,
        B8,
        B9,
        B10,
        B11,
        B12,
        B13,
        B14,
        B15,
        C13,
        C14,
        C15,
    };

    public enum AxisType : byte
    {
        X = 0,
        Y,
        Z,
        Rx,
        Ry,
        Rz,
        Slider1,
        Slider2,
    };

    public enum AxisFunction :byte
    {
        None = 0,
        Plus,
        Minus,
        Equal,
    };

    public enum AxisButtonFullType : byte
    {
        Function_Enable = 0,
        Prescaler_Enable,
        Center,
        Reset,
        Down,
        Up,
    }
    public enum AxisButtonCutType : byte
    {
        Function_Enable = 0,
        Prescaler_Enable,
        Center,
        Reset,
    }

    public enum AxisAddressType : byte
    {
        AS5600 = 0x36,
        ADS1115_00 = 0x48,
        ADS1115_01,
        ADS1115_10,
        ADS1115_11,
    }

    public class AxisConfig : BindableBase
    {
        private short _calibMin;
        private short _calibCenter;
        private short _calibMax;

        private bool _isOutEnabled;
        private int _offsetAngle;
        private bool _isInverted;
        private byte _filterLevel;

        private ObservableCollection<Point> _curveShape;

        private byte _resolution;
        private byte _channel;
        private AxisAddressType _i2cAddress;
        private byte _deadband;
        private bool _isDynamicDeadband;

        private AxisSourceType _sourceMain;
        private AxisType _sourceSecondary;
        private AxisFunction _function;

        private sbyte _button1;
        private sbyte _button2;
        private sbyte _button3;
        private ushort _divider;

        private AxisButtonFullType _button1_type;
        private AxisButtonCutType _button2_type;
        private AxisButtonFullType _button3_type;

        private byte _prescaler;

        private bool _isCentered;
        private bool _isAddressEnabled;

        public short CalibMin
        {
            get{return _calibMin;}
            set
            {
                if (value < -32767) SetProperty(ref _calibMin, (short)-32767);
                else if (value >= CalibCenter && IsCentered) SetProperty(ref _calibMin, (short)(CalibCenter - 1));
                else if (value >= CalibMax) SetProperty(ref _calibMin, (short)(CalibMax - 2));
                else SetProperty(ref _calibMin, value);

                if (!IsCentered)
                {
                    CalibCenter = (short)((CalibMax - CalibMin) / 2 + CalibMin);
                }
            }
        }
        public short CalibCenter
        {
            get{return _calibCenter;}
            set
            {
                if (IsCentered)
                {
                    if (value <= CalibMin)
                    {
                        SetProperty(ref _calibCenter, (short)(CalibMin + 1));
                    }
                    else if (value >= CalibMax)
                    {
                        SetProperty(ref _calibCenter, (short)(CalibMax - 1));
                    }
                    else SetProperty(ref _calibCenter, value);
                }
                else
                {
                    SetProperty(ref _calibCenter, (short)((CalibMax - CalibMin) / 2 + CalibMin));
                }
            }
        }
        public short CalibMax
        {
            get {return _calibMax;}
            set
            {
                if (value <= CalibMin) SetProperty(ref _calibMax, (short)(CalibMin + 2));
                else if (value <= CalibCenter && IsCentered) SetProperty(ref _calibMax, (short)(CalibCenter + 1));
                else if (value > 32767) SetProperty(ref _calibMax, (short) 32767);
                else SetProperty(ref _calibMax, value);

                if (!IsCentered)
                {
                    CalibCenter = (short)((CalibMax - CalibMin) / 2 + CalibMin);
                }
            }
        }

        public bool IsOutEnabled
        {
            get {return _isOutEnabled;}
            set{ SetProperty(ref _isOutEnabled, value);}
        }
        public bool IsInverted
        {
            get {return _isInverted;}
            set{ SetProperty(ref _isInverted, value);}
        }
        public int OffsetAngle
        {
            get { return _offsetAngle; }
            set{SetProperty(ref _offsetAngle, value);}
        }
        public byte FilterLevel
        {
            get{ return _filterLevel;}
            set{SetProperty(ref _filterLevel, value);}
        }

        public ObservableCollection<Point> CurveShape
        {
            get {return _curveShape;}
            set{SetProperty(ref _curveShape, value);}
        }

        public byte Resolution
        {
            get {return _resolution; }
            set { SetProperty(ref _resolution, value);}
        }
        public byte Channel
        {
            get { return _channel; }
            set { SetProperty(ref _channel, value); }
        }
        public AxisAddressType I2cAddress
        {
            get { return _i2cAddress; }
            set { SetProperty(ref _i2cAddress, value); }
        }
        public byte Deadband
        {
            get { return _deadband; }
            set { SetProperty(ref _deadband, value); }
        }
        public bool IsDynamicDeadband
        {
            get { return _isDynamicDeadband; }
            set { SetProperty(ref _isDynamicDeadband, value); }
        }

        public AxisSourceType SourceMain
        {
            get{return _sourceMain;}
            set
            {
                SetProperty(ref _sourceMain, value);
                if (_sourceMain == AxisSourceType.I2C) IsAddressEnabled = true;
                else IsAddressEnabled = false;
            }
        }

        public AxisType SourceSecondary
        {
            get{return _sourceSecondary; }
            set { SetProperty(ref _sourceSecondary, value);}
        }
        public AxisFunction Function
        {
            get { return _function; }
            set { SetProperty(ref _function, value); }
        }


        public sbyte Button1
        {
            get { return _button1; }
            set { SetProperty(ref _button1, value); }
        }
        public sbyte Button2
        {
            get { return _button2; }
            set { SetProperty(ref _button2, value); }
        }
        public sbyte Button3
        {
            get { return _button3; }
            set { SetProperty(ref _button3, value); }
        }
        public ushort Divider
        {
            get { return _divider; }
            set { SetProperty(ref _divider, value); }
        }
        public AxisButtonFullType Button1_Type
        {
            get { return _button1_type; }
            set { SetProperty(ref _button1_type, value); }
        }
        public AxisButtonCutType Button2_Type
        {
            get { return _button2_type; }
            set { SetProperty(ref _button2_type, value); }
        }
        public AxisButtonFullType Button3_Type
        {
            get { return _button3_type; }
            set { SetProperty(ref _button3_type, value); }
        }

        public byte Prescaler
        {
            get { return _prescaler; }
            set { SetProperty(ref _prescaler, value); }
        }


        public bool IsCentered
        {
            get{return _isCentered; }
            set
            {
                SetProperty(ref _isCentered, value);
                if (!_isCentered)
                {
                    CalibCenter = (short)((CalibMax - CalibMin) / 2);
                }
            }
        }

        public bool IsAddressEnabled
        {
            get
            {
                return _isAddressEnabled;
            }
            set
            {
                SetProperty(ref _isAddressEnabled, value);
            }
        }

        public AxisConfig()
        {
            _calibMin = -32767;
            _calibCenter = 0;
            _calibMax = 32767;

            _isInverted = false;
            _offsetAngle = 0;
            _isOutEnabled = true;

            _sourceMain = AxisSourceType.None;
            _sourceSecondary = AxisType.X;
            _function = AxisFunction.None;

            _button1 = 0;
            _button2 = 0;
            _button3 = 0;
            _divider = 255;

            _button1_type = AxisButtonFullType.Down;
            _button2_type = AxisButtonCutType.Reset;
            _button3_type = AxisButtonFullType.Up;

            _prescaler = 100;

            _resolution = 16;
            _deadband = 0;

            _channel = 0;
            _i2cAddress = AxisAddressType.ADS1115_00;

            _curveShape = new ObservableCollection<Point>();
            for (int i = 0; i < 11; i++) _curveShape.Add(new Point(i, -100+20*i));
            _filterLevel = 0;

            _isCentered = false;
        }

    }

    public class FilterLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string converted;

            Byte original = (Byte) value;

            if (original == 0) converted = "Filter Off";
            else if (original == 1) converted = "Filter level 1";
            else if (original == 2) converted = "Filter level 2";
            else if (original == 3) converted = "Filter level 3";
            else if (original == 4) converted = "Filter level 4";
            else if (original == 5) converted = "Filter level 5";
            else if (original == 6) converted = "Filter level 6";
            else if (original == 7) converted = "Filter level 7";
            else converted = "Off";

            return converted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public enum PinType : byte
    {
        Not_Used = 0,
        Button_Gnd,
        Button_Vcc,
        Button_Row,
        Button_Column,

        Axis_Analog,
        Fast_Encoder,

        SPI_SCK,
        SPI_MOSI,
        SPI_MISO,

        TLE5011_GEN,
        TLE5011_CS,
        TLE5012B_CS,
        

        MCP3201_CS,
        MCP3202_CS,
        MCP3204_CS,
        MCP3208_CS,

        MLX90393_CS,
        AS5048A_CS,

        ShiftReg_LATCH,
        ShiftReg_DATA,

        LED_PWM,
        LED_Single,
        LED_Row,
        LED_Column,

        I2C_SCL,
        I2C_SDA,
       
    };


    public enum ButtonType
    {
        Button_Normal = 0,
        Button_Toggle,
        ToggleSwitch_OnOff,
        ToggleSwitch_On,
        ToggleSwitch_Off,

        Pov1_Up,
        Pov1_Right,
        Pov1_Down,
        Pov1_Left,
        Pov1_Center,

        Pov2_Up,
        Pov2_Right,
        Pov2_Down,
        Pov2_Left,
        Pov2_Center,

        Pov3_Up,
        Pov3_Right,
        Pov3_Down,
        Pov3_Left,

        Pov4_Up,
        Pov4_Right,
        Pov4_Down,
        Pov4_Left,

        Encoder_A,
        Encoder_B,
        
        RadioButton1,
        RadioButton2,
        RadioButton3,
        RadioButton4,

        Sequential_Toggle,
        Sequential_Button,
    };

    public enum ButtonSourceType
    {
        NoSource = 0,
        SingleButton,
        MatrixButton,
        ShiftRegister,
        AxisToButtons,
        Shift,
    }


    public class ButtonConfig : BindableBase
    {
        private sbyte _physicalNumber;
        private ShiftType _shiftModificator;
        private ButtonType _type;
        private TimerType _buttonDelayNumber;
        private TimerType _buttonToggleNumber;
        private bool _isInverted;
        private bool _isDisabled;

        private bool _isEnabled;


        public ButtonType Type
        {
            get {return _type;}
            set{SetProperty(ref _type, value);}
        }

        public bool IsInverted
        {
            get { return _isInverted; }
            set { SetProperty(ref _isInverted, value); }
        }

        public bool IsDisabled
        {
            get { return _isDisabled; }
            set { SetProperty(ref _isDisabled, value); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public sbyte PhysicalNumber
        {
            get{return _physicalNumber;}
            set
            {
                SetProperty(ref _physicalNumber, value);
                if (value > 0) IsEnabled = true;
                else IsEnabled = false;
            }
        }

        public ShiftType ShiftModificator
        {
            get { return _shiftModificator; }
            set {SetProperty(ref _shiftModificator, value);}
        }

        public TimerType ButtonDelayNumber
        {
            get { return _buttonDelayNumber; }
            set { SetProperty(ref _buttonDelayNumber, value); }
        }

        public TimerType ButtonToggleNumber
        {
            get { return _buttonToggleNumber; }
            set { SetProperty(ref _buttonToggleNumber, value); }
        }

        public ButtonConfig()
        {
            _isEnabled = false;
            _isInverted = false;
            _isDisabled = false;
            _physicalNumber = 0;
            _shiftModificator = ShiftType.NoShift;
            _buttonDelayNumber = TimerType.No;
            _buttonToggleNumber = TimerType.No;

            _type = ButtonType.Button_Normal;
            
        }

        public ButtonConfig (ButtonType type)
        {
            _isEnabled = false;
            _isInverted = false;
            _isDisabled = false;
            _physicalNumber = 0;
            _shiftModificator = ShiftType.NoShift;
            _buttonDelayNumber = TimerType.No;
            _buttonToggleNumber = TimerType.No;

            _type = type;
        }
    }

    public enum ShiftType
    {
        NoShift = 0,
        Shift1,
        Shift2,
        Shift3,
        Shift4,
        Shift5,
    }

    public enum TimerType
    {
        No = 0,
        Timer1,
        Timer2,
        Timer3,
    }

    public class ShiftModificatorConfig : BindableBase
    {
        private sbyte _button;

        public sbyte Button
        {
            get { return _button; }
            set { SetProperty(ref _button, value); }
        }
    }

    public enum EncoderType
    {
        Encoder_1x = 0,
        Encoder_2x,
        Encoder_4x,
    }

    public class EncoderConfig : BindableBase
    {
        private EncoderType _type;

        public EncoderType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
    }

    public class AxisToButtonsConfig : BindableBase
    {
        private ObservableCollection<byte> _points;
        private byte _buttonsCnt;

        public ObservableCollection<byte> Points
        {
            get { return _points; }
            set { SetProperty(ref _points, value); }
        }

        public byte ButtonsCnt
        {
            get { return _buttonsCnt; }
            set
            {
                SetProperty(ref _buttonsCnt, value);

            }
        }

        public AxisToButtonsConfig()
        {
            _points = new ObservableCollection<byte>();
            for (int i = 0; i < 13; i++) _points.Add(new byte());

            _points[0] = 0;
            _points[1] = 255;

            _buttonsCnt = 0;
        }
    }

    public enum ShiftRegisterType
    {
        HC165_PullDown = 0,
        CD4021_PullDown,
        HC165_PullUp,
        CD4021_PullUp,
    };

    public class ShiftRegisterConfig : BindableBase
    {
        private ShiftRegisterType _type;
        private short _buttonCnt;

        public ShiftRegisterType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        public short ButtonCnt
        {
            get { return _buttonCnt; }
            set { SetProperty(ref _buttonCnt, value); }
        }

        public ShiftRegisterConfig()
        {
            _type = ShiftRegisterType.HC165_PullUp;
            _buttonCnt = 0;
        }
    }

    public class LedPwm : BindableBase
    {
        private byte _dutyCycle;
        private bool _isAxis;
        private byte _axisNumber;

        public byte DutyCycle
        {
            get { return _dutyCycle; }
            set { SetProperty(ref _dutyCycle, value); }
        }

        public bool IsAxis
        {
            get { return _isAxis; }
            set { SetProperty(ref _isAxis, value); }
        }

        public byte AxisNumber
        {
            get { return _axisNumber; }
            set { SetProperty(ref _axisNumber, value); }
        }

        public LedPwm()
        {
            _dutyCycle = 50;
            _isAxis = false;
            _axisNumber = 0;
        }

        public LedPwm(byte dutyCycle, bool isAxis, byte axisNumber)
        {
            _dutyCycle = dutyCycle;
            _isAxis = isAxis;
            _axisNumber = axisNumber;
        }
    }

    public enum LedType
    {
        Normal = 0,
        Inverted,
    }

    public class LedConfig : BindableBase
    {
        private sbyte _inputNumber;
        private LedType _type;

        public sbyte InputNumber
        {
            get { return _inputNumber; }
            set { SetProperty(ref _inputNumber, value); }
        }
        public LedType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
    
    }

    public class DeviceConfig : BindableBase
    {
        private string _deviceName;

        [XmlElement("Firmware_Version")]
        public UInt16 FirmwareVersion { get; set; }
        [XmlElement("Device_Name")]
        public string DeviceName
        {
            get
            {
                return _deviceName;
            }
            set
            {
                string tmp;
                if (value.Length > 25) tmp = value.Substring(0, 25);
                else tmp = value;
                SetProperty(ref _deviceName, tmp);
            }
        }
        [XmlElement("Button_Debounce_Time")]
        public UInt16 ButtonDebounceMs { get; set; }
        [XmlElement("A2b_Debounce_Time")]
        public UInt16 A2bDebounceMs { get; set; }
        [XmlElement("Encoder_Press_Time")]
        public byte EncoderPressMs { get; set; }
        [XmlElement("Button_Timer1_Time")]
        public UInt16 ButtonTimer1Ms { get; set; }
        [XmlElement("Button_Timer2_Time")]
        public UInt16 ButtonTimer2Ms { get; set; }
        [XmlElement("Button_Timer3_Time")]
        public UInt16 ButtonTimer3Ms { get; set; }
        [XmlElement("Exchange_Period")]
        public byte ExchangePeriod { get; set; }
        [XmlElement("Pin_Config")]
        public ObservableCollection<PinType> PinConfig { get; set; }
        [XmlElement("Axis_Config")]
        public ObservableCollection<AxisConfig> AxisConfig { get; set; }
        [XmlElement("ShiftModificator_Config")]
        public ObservableCollection<ShiftModificatorConfig> ShiftModificatorConfig { get; set; }
        [XmlElement("Button_Config")]
        public ObservableCollection<ButtonConfig> ButtonConfig { get; set; }
        [XmlElement("AxisToButtons_Config")]
        public ObservableCollection<AxisToButtonsConfig> AxisToButtonsConfig { get; set; }
        [XmlElement("ShitRegisters_Config")]
        public ObservableCollection<ShiftRegisterConfig> ShiftRegistersConfig { get; set; }
        [XmlElement("EncodersConfig")]
        public ObservableCollection<EncoderConfig> EncodersConfig { get; set; }
        [XmlElement("DynamicConfig")]
        public bool IsDynamicConfig { get; set; }
        [XmlElement("Vid")]
        public UInt16 Vid { get; set; }
        [XmlElement("Pid")]
        public UInt16 Pid { get; set; }
        [XmlElement("LedPwmConfig")]
        public ObservableCollection<LedPwm> LedPwmConfig { get; set; }
        [XmlElement("LedConfig")]
        public ObservableCollection<LedConfig> LedConfig { get; set; }


        public DeviceConfig()
        {
            DeviceName = "FreeJoy";
            ButtonDebounceMs = 50;
            A2bDebounceMs = 50;
            EncoderPressMs = 10;
            ButtonTimer1Ms = 50;
            ButtonTimer2Ms = 200;
            ButtonTimer3Ms = 300;
            ExchangePeriod = 5;
            IsDynamicConfig = false;
            Vid = 0x0483;
            Pid = 0x5757;

            AxisConfig = new ObservableCollection<AxisConfig>();
            for (int i = 0; i < 8; i++) AxisConfig.Add(new AxisConfig());

            PinConfig = new ObservableCollection<PinType>();
            for (int i = 0; i < 30; i++) PinConfig.Add(PinType.Not_Used);

            ShiftModificatorConfig = new ObservableCollection<ShiftModificatorConfig>();
            for (int i = 0; i < 5; i++) ShiftModificatorConfig.Add(new ShiftModificatorConfig());

            ButtonConfig = new ObservableCollection<ButtonConfig>();
            for (int i = 0; i < 128; i++) ButtonConfig.Add(new ButtonConfig());

            AxisToButtonsConfig = new ObservableCollection<AxisToButtonsConfig>();
            for (int i = 0; i < 8; i++) AxisToButtonsConfig.Add(new AxisToButtonsConfig());

            ShiftRegistersConfig = new ObservableCollection<ShiftRegisterConfig>();
            for (int i = 0; i < 4; i++) ShiftRegistersConfig.Add(new ShiftRegisterConfig());

            EncodersConfig = new ObservableCollection<EncoderConfig>();
            for (int i = 0; i < 16; i++) EncodersConfig.Add(new EncoderConfig());

            LedPwmConfig = new ObservableCollection<LedPwm>();
            for (int i = 0; i < 4; i++) LedPwmConfig.Add(new LedPwm());

                LedConfig = new ObservableCollection<LedConfig>();
            for (int i = 0; i < 24; i++) LedConfig.Add(new LedConfig());
        }
    }
        
}
