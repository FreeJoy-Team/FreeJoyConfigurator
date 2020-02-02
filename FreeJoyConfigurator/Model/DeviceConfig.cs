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
        Buttons = -1,
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
        B2,
        B4,
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
        Plus_Absolute,
        Plus_Relative,
        Minus_Absolute,
        Minus_Relative,
    };

    public class AxisConfig : BindableBase
    {
        private short _calibMin;
        private short _calibCenter;
        private short _calibMax;

        private bool _isOutEnabled;
        private bool _isMagnetOffset;
        private bool _isInverted;
        private byte _filterLevel;

        private ObservableCollection<Point> _curveShape;

        private byte _resolution;
        private byte _deadZone;

        private AxisSourceType _sourceMain;
        private AxisType _sourceSecondary;
        private AxisFunction _function;

        private sbyte _incrementButton;
        private sbyte _decrementButton;
        private sbyte _centerButton;
        private byte _step;

        private bool _isCalibCenterUnlocked;
        

        public short CalibMin
        {
            get{return _calibMin;}
            set
            {
                if (value < -32767) SetProperty(ref _calibMin, (short)-32767);
                else if (value >= CalibCenter && IsCalibCenterUnlocked) SetProperty(ref _calibMin, (short)(CalibCenter - 1));
                else if (value >= CalibMax) SetProperty(ref _calibMin, (short)(CalibMax - 2));
                else SetProperty(ref _calibMin, value);

                if (!IsCalibCenterUnlocked)
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
                if (IsCalibCenterUnlocked)
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
                else if (value <= CalibCenter && IsCalibCenterUnlocked) SetProperty(ref _calibMax, (short)(CalibCenter + 1));
                else if (value > 32767) SetProperty(ref _calibMax, (short) 32767);
                else SetProperty(ref _calibMax, value);

                if (!IsCalibCenterUnlocked)
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
        public bool IsMagnetOffset
        {
            get { return _isMagnetOffset; }
            set{SetProperty(ref _isMagnetOffset, value);}
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
        public byte DeadZone
        {
            get { return _deadZone; }
            set { SetProperty(ref _deadZone, value); }
        }

        public AxisSourceType SourceMain
        {
            get{return _sourceMain;}
            set{ SetProperty(ref _sourceMain, value);}
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

        public sbyte IncrementButton
        {
            get { return _incrementButton; }
            set { SetProperty(ref _incrementButton, value); }
        }
        public sbyte DecrementButton
        {
            get { return _decrementButton; }
            set { SetProperty(ref _decrementButton, value); }
        }
        public sbyte CenterButton
        {
            get { return _centerButton; }
            set { SetProperty(ref _centerButton, value); }
        }
        public byte Step
        {
            get { return _step; }
            set { SetProperty(ref _step, value); }
        }


        public bool IsCalibCenterUnlocked
        {
            get{return _isCalibCenterUnlocked;}
            set
            {
                SetProperty(ref _isCalibCenterUnlocked, value);
                if (!_isCalibCenterUnlocked)
                {
                    CalibCenter = (short)((CalibMax - CalibMin) / 2);
                }
            }
        }


        public AxisConfig()
        {
            _calibMin = -32767;
            _calibCenter = 0;
            _calibMax = 32767;

            _isInverted = false;
            _isMagnetOffset = false;
            _isOutEnabled = true;

            _sourceMain = AxisSourceType.Buttons;
            _sourceSecondary = AxisType.X;
            _function = AxisFunction.None;

            _decrementButton = 0;
            _incrementButton = 0;
            _centerButton = 0;
            _step = 0;

            _resolution = 16;
            _deadZone = 0;


            _curveShape = new ObservableCollection<Point>();
            for (int i = 0; i < 11; i++) _curveShape.Add(new Point(i, 0));
            _filterLevel = 0;

            _isCalibCenterUnlocked = false;
        }

    }

    public class FilterLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string converted;

            Byte original = (Byte) value;

            if (original == 0) converted = "Off";
            else if(original == 1) converted = "Low";
            else if (original == 2) converted = "Medium";
            else if(original == 3) converted = "High";
            else converted = "Filter No";

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
//        AxisToButtons,

        SPI_SCK = 7,

        TLE501x_CS,
        TLE501x_DATA,
        TLE501x_GEN,

        ShiftReg_LATCH,
        ShiftReg_DATA,
        
    };


    public enum ButtonType
    {
        Button_Normal = 0,
        Button_Inverted,
        Button_Toggle,
        ToggleSwitch_OnOff,
        ToggleSwitch_On,
        ToggleSwitch_Off,

        Pov1_Up,
        Pov1_Right,
        Pov1_Down,
        Pov1_Left,
        Pov2_Up,
        Pov2_Right,
        Pov2_Down,
        Pov2_Left,
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
        private int _maxPhysicalNumber;
        private ShiftType _shiftModificator;
        private ButtonType _type;
        private bool _isEnabled;
        
        

        public ButtonType Type
        {
            get {return _type;}
            set{SetProperty(ref _type, value);}
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

        public int MaxPhysicalNumber
        {
            get { return _maxPhysicalNumber; }
            set
            {
                SetProperty(ref _maxPhysicalNumber, value);
            }
        }

        public ShiftType ShiftModificator
        {
            get { return _shiftModificator; }
            set {SetProperty(ref _shiftModificator, value);}
        }

        public ButtonConfig()
        {
            _isEnabled = false;
            _physicalNumber = 0;
            _shiftModificator = 0;

            _type = ButtonType.Button_Normal;
            
        }

        public ButtonConfig (ButtonType type)
        {
            _isEnabled = false;
            _physicalNumber = 0;
            _shiftModificator = 0;

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


    public class ShiftModificatorConfig : BindableBase
    {
        private sbyte _button;

        public sbyte Button
        {
            get { return _button; }
            set { SetProperty(ref _button, value); }
        }
        

    }

    public class AxisToButtonsConfig : BindableBase
    {
        private ObservableCollection<sbyte> _points;
        private byte _buttonsCnt;
        private bool _isEnabled;

        public ObservableCollection<sbyte> Points
        {
            get { return _points; }
            set { SetProperty(ref _points, value); }
        }

        public byte ButtonsCnt
        {
            get { return _buttonsCnt; }
            set { SetProperty(ref _buttonsCnt, value); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public AxisToButtonsConfig()
        {
            _points = new ObservableCollection<sbyte>();
            for (int i = 0; i < 13; i++) _points.Add(new sbyte());

            _points[0] = 0;
            _points[1] = 50;
            _points[2] = 100;

            _buttonsCnt = 2;
            _isEnabled = false;
        }
    }

    public enum ShiftRegisterType
    {
        HC165 = 0,
        CD4021 = 1,
    };

    public class ShiftRegisterConfig : BindableBase
    {
        private ShiftRegisterType _type;
        private short _buttonCnt;
        //private byte _selectPin;
        //private byte _dataPin;

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

        //public byte LatchPin
        //{
        //    get { return _selectPin; }
        //    set { SetProperty(ref _selectPin, value); }
        //}
        //public byte ClockPin

        //{
        //    get { return _dataPin; }
        //    set { SetProperty(ref _dataPin, value); }
        //}

        public ShiftRegisterConfig()
        {
            _type = ShiftRegisterType.CD4021;
            _buttonCnt = 0;
            //_selectPin = 0xFF;
            //_dataPin = 0xFF;
        }
    }

    public class DeviceConfig : BindableBase
    {
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
        [XmlElement("ShiftModificator_Config")]
        public ObservableCollection<ShiftModificatorConfig> ShiftModificatorConfig { get; set; }
        [XmlElement("Button_Config")]
        public ObservableCollection<ButtonConfig> ButtonConfig { get; set; }
        [XmlElement("AxisToButtons_Config")]
        public ObservableCollection<AxisToButtonsConfig> AxisToButtonsConfig { get; set; }
        [XmlElement("ShitRegisters_Config")]
        public ObservableCollection<ShiftRegisterConfig> ShiftRegistersConfig { get; set; }

        public DeviceConfig()
        {
            DeviceName = "FreeJoy";
            ButtonDebounceMs = 50;
            TogglePressMs = 300;
            EncoderPressMs = 100;
            ExchangePeriod = 10;

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
        }
    }
        
}
