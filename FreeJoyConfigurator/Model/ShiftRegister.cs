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
        private int _buttonCnt;
        private string _name;

        public int ButtonCnt
        {
            get { return _buttonCnt; }
            set { SetProperty(ref _buttonCnt, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public ShiftRegister ()
        {
            _buttonCnt = 0;
            _name = " "; 
        }

        public ShiftRegister(int buttonCnt)
        {
            _buttonCnt = buttonCnt;
            _name = " ";
        }
        public ShiftRegister(string name)
        {
            _buttonCnt = 0;
            _name = name;
        }

        public ShiftRegister(int buttonCnt, string name)
        {
            _buttonCnt = buttonCnt;
            _name = name;
        }
    }
}
