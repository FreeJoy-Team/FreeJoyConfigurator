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
                RegisterCnt = (int) ((float)_buttonCnt / 8.0);  
            }
        }

        public int RegisterCnt
        {
            get { return _registerCnt; }
            private set { SetProperty(ref _registerCnt, value); }
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
            _type = ShiftRegisterType.HC165; 
        }

        public ShiftRegister(int number, int buttonCnt)
        {
            _isEnabled = false;
            _number = number;
            _buttonCnt = buttonCnt;
            _registerCnt = (int)((float)_buttonCnt / 8.0);
            _type = ShiftRegisterType.HC165;
        }
        public ShiftRegister(int number, ShiftRegisterType type)
        {
            _isEnabled = false;
            _number = number;
            _buttonCnt = 0;
            _registerCnt = 0;
            _type = type;
        }

        public ShiftRegister(int number, int buttonCnt, ShiftRegisterType type)
        {
            _isEnabled = false;
            _number = number;
            _buttonCnt = buttonCnt;
            _registerCnt = (int)((float)_buttonCnt / 8.0);
            _type = type;
        }
    }
}
