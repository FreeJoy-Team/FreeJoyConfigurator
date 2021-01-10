using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class PinVMConverter : BindableBase
    {
        private ObservableCollection<PinType> _allowedTypes;
        private PinType _selectedType;
        private bool _typeError;

        public ObservableCollection<PinType> AllowedTypes
        {
            get
            {
                return _allowedTypes;
            }
            set
            {
                SetProperty(ref _allowedTypes, value);
            }
        }

        public PinType SelectedType
        {
            get
            {
                return _selectedType;
            }
            set
            {
                SetProperty(ref _selectedType, value);
            }
        }

        public bool TypeError
        {
            get
            {
                return _typeError;
            }
            set
            {
                SetProperty(ref _typeError, value);
            }
        }

        public PinVMConverter()
        {
            _allowedTypes = new ObservableCollection<PinType>()
            {       PinType.Not_Used,
                    PinType.Button_Gnd,
                    PinType.Button_Vcc,
                    PinType.Button_Row,
                    PinType.Button_Column,
                    PinType.ShiftReg_LATCH,
                    PinType.ShiftReg_DATA,
                    PinType.TLE5011_CS,
                    PinType.TLE5012B_CS,
                    PinType.MCP3201_CS,
                    PinType.MCP3202_CS,
                    PinType.MCP3204_CS,
                    PinType.MCP3208_CS,
                    PinType.MLX90393_CS,
                    PinType.AS5048A_CS,
                    PinType.LED_Single,
                    PinType.LED_Row,
                    PinType.LED_Column,
                    
            };

            _selectedType = PinType.Not_Used;
            _typeError = false;
        }
    }
}
