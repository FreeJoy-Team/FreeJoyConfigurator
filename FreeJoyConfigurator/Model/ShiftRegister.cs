using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class ShiftRegister : BindableBase
    {
        private bool _isEnabled;
        private int _number;
        private int _buttonCnt;
        private int _registerCnt;
        private ShiftRegSourceType _latchPin;
        private ShiftRegSourceType _dataPin;
        private ShiftRegisterType _type;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public int Number
        {
            get { return _number; }
            set
            {
                SetProperty(ref _number, value);
            }
        }

        public int ButtonCnt
        {
            get { return _buttonCnt; }
            set
            {
                SetProperty(ref _buttonCnt, value);
                RegisterCnt = (int)Math.Ceiling((float)_buttonCnt / 8.0);  
            }
        }

        public int RegisterCnt
        {
            get { return _registerCnt; }
            private set { SetProperty(ref _registerCnt, value); }
        }

        public ShiftRegSourceType LatchPin
        {
            get { return _latchPin; }
            set { SetProperty(ref _latchPin, value); }
        }

        public ShiftRegSourceType DataPin
        {
            get { return _dataPin; }
            set { SetProperty(ref _dataPin, value); }
        }

        public ShiftRegisterType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        public ShiftRegister (int number)
        {
            _isEnabled = false;
            _number = number;
            _buttonCnt = 0;
            _registerCnt = 0;
            _type = ShiftRegisterType.HC165_PullUp;
            _latchPin = ShiftRegSourceType.NotDefined;
            _dataPin = ShiftRegSourceType.NotDefined;
        }

        public ShiftRegister(int number, int buttonCnt)
        {
            _isEnabled = false;
            _number = number;
            _buttonCnt = buttonCnt;
            _registerCnt = (int) Math.Ceiling((float)_buttonCnt / 8.0);
            _type = ShiftRegisterType.HC165_PullUp;
            _latchPin = ShiftRegSourceType.NotDefined;
            _dataPin = ShiftRegSourceType.NotDefined;
        }
        public ShiftRegister(int number, ShiftRegisterType type)
        {
            _isEnabled = false;
            _number = number;
            _buttonCnt = 0;
            _registerCnt = 0;
            _type = type;
            _latchPin = ShiftRegSourceType.NotDefined;
            _dataPin = ShiftRegSourceType.NotDefined;
        }

        public ShiftRegister(int number, int buttonCnt, ShiftRegisterType type)
        {
            _isEnabled = false;
            _number = number;
            _buttonCnt = buttonCnt;
            _registerCnt = (int)((float)_buttonCnt / 8.0);
            _type = type;
            _latchPin = ShiftRegSourceType.NotDefined;
            _dataPin = ShiftRegSourceType.NotDefined;
        }

        public ShiftRegister(int number, int buttonCnt, ShiftRegisterType type, ShiftRegSourceType latchPin, ShiftRegSourceType dataPin)
        {
            _isEnabled = false;
            _number = number;
            _buttonCnt = buttonCnt;
            _registerCnt = (int)((float)_buttonCnt / 8.0);
            _type = type;
            _latchPin = latchPin;
            _dataPin = dataPin;
        }
    }
}
