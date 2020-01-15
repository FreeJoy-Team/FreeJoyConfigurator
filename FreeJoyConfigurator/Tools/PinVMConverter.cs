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
            {       PinType.NotUsed,
                    PinType.ButtonGnd,
                    PinType.ButtonVcc,
                    PinType.ButtonRow,
                    PinType.ButtonColumn,
                    PinType.ShiftReg_CS,
                    PinType.ShiftReg_Data,
                    PinType.TLE501x_CS,
            };

            _selectedType = PinType.NotUsed;
            _typeError = false;
        }
    }
}
