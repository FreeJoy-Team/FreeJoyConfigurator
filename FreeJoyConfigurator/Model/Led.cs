using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class Led : BindableBase
    {
        private bool _state;
        private LedType _type;
        private sbyte _inputNumber;

        public int Number { get; private set; }

        public bool State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public LedType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        public sbyte InputNumber
        {
            get { return _inputNumber; }
            set { SetProperty(ref _inputNumber, value); }
        }

        public Led()
        {
            Number = 0;
            _state = false;
            _type = LedType.Normal;
            _inputNumber = 0;
        }

        public Led(int number)
        {
            Number = number;
            _state = false;
            _type = LedType.Normal;
            _inputNumber = 0;
        }

        public Led(int number, sbyte inputNumber, LedType type)
        {
            Number = number;
            _state = false;
            _type = type;
            _inputNumber = inputNumber;
        }
    }
}
