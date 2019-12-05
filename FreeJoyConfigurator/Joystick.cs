using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HidLibrary;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class Joystick : BindableBase
    {
        private DeviceConfig _config;
        
        public ObservableCollection<Axis> Axes { get; private set; }
        public ObservableCollection<Button> Buttons { get; private set; }

        public Joystick(DeviceConfig config)
        {
            _config = config;
            Axes = new ObservableCollection<Axis>();
            for (int i = 0; i < 8; i++)
            {
                Axes.Add(new Axis(i+1));
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
        private bool _state;
        public int Number { get; private set; }

        public ButtonType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
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
            _state = false;
        }

        public Button(bool state, int number)
        {
            Number = number;
            _type = ButtonType.BtnNormal;
            _state = state;
        }
        public Button(bool state, ButtonType type, int number)
        {
            Number = number;
            _type = type;
            _state = state;
        }
    }

    public class Axis : BindableBase
    {
        private ushort _value;
        private ushort _rawValue;
        private bool _isEnabled;

        public int Number {get; private set;}
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

        public Axis(int number)
        {
            Number = number;
            _value = 0;
            _rawValue = 0;
        }

        public Axis (ushort value, int number)
        {
            Number = number;
            _value = value;
            _rawValue = 0;
        }

        public Axis(ushort value, ushort rawValue, int number)
        {
            Number = number;
            _value = value;
            _rawValue = rawValue;
        }
    }
}
