using System;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class Encoder : BindableBase
    {
        private bool _isEnabled;
        private int _number;
        private int _inputA;
        private int _inputB;
        private EncoderType _type;

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

        public int InputA
        {
            get { return _inputA; }
            set
            {
                SetProperty(ref _inputA, value);
            }
        }

        public int InputB
        {
            get { return _inputB; }
            set
            {
                SetProperty(ref _inputB, value);
            }
        }

        public EncoderType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        public Encoder(int number)
        {
            _isEnabled = false;
            _number = number;
            _type = EncoderType.Encoder_1x;
            _inputA = -1;
            _inputB = -1;
        }

        public Encoder(int number, EncoderType type)
        {
            _isEnabled = false;
            _number = number;
            _type = type;
            _inputA = -1;
            _inputB = -1;
        }

        public Encoder(int number, EncoderType type, int inputA, int inputB)
        {
            _isEnabled = false;
            _number = number;
            _type = type;
            _inputA = inputA;
            _inputB = inputB;
        }
    }

}
