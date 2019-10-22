using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FreeJoyConfigurator
{
    public class PinsVM : BindableBase
    {
        const int maxBtnCnt = 128;

        private int _rowCnt;
        private int _colCnt;
        private int _singleBtnCnt;
        private int _totalBtnCnt;
        private int _axesCnt;

        public int RowCnt
        {
            get
            {
                return _rowCnt;
            }
            private set
            {
                SetProperty(ref _rowCnt, value);
                TotalBtnCnt = _rowCnt * _colCnt + _singleBtnCnt;
            }
        }
        public int ColCnt
        {
            get
            {
                return _colCnt;
            }
            private set
            {
                SetProperty(ref _colCnt, value);
                TotalBtnCnt = _rowCnt * _colCnt + _singleBtnCnt;
            }
        }
        public int SingleBtnCnt
        {
            get
            {
                return _singleBtnCnt;
            }
            private set
            {
                SetProperty(ref _singleBtnCnt, value);
                TotalBtnCnt = _rowCnt * _colCnt + _singleBtnCnt;
            }
        }
        public int TotalBtnCnt
        {
            get
            {
                return _totalBtnCnt;
            }
            private set
            {
                SetProperty(ref _totalBtnCnt, value);
            }
        }
        public int AxesCnt
        {
            get
            {
                return _axesCnt;
            }
            private set
            {
                SetProperty(ref _axesCnt, value);
            }
        }


        private ObservableCollection<PinConverter> _pins;
        public ObservableCollection<PinConverter> Pins
        {
            get
            {
                return _pins;
            }
            set
            {
                SetProperty(ref _pins, value);
            }
        }


        public DeviceConfig Config { get; set; }

        public PinsVM(DeviceConfig deviceConfig)
        {
            Config = deviceConfig;
            Config.PropertyChanged += Config_PropertyChanged;

            _pins = new ObservableCollection<PinConverter>();
            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                _pins.Add(new PinConverter());
                if (i < 8) _pins[i].AllowedTypes.Add(PinType.AxisAnalog);
                Pins[i].PropertyChanged += PinsVM_PropertyChanged;
            }
        }

        private void Config_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                Pins[i].SelectedType = Config.PinConfig[i];
            }
        }

        public void ResetPins()
        {
            for (int i = 0; i < _pins.Count; i++)
            {
                _pins[i] = new PinConverter();
                if (i < 8) _pins[i].AllowedTypes.Add(PinType.AxisAnalog);
                Pins[i].PropertyChanged += PinsVM_PropertyChanged;
            }
        }

        // Some dirty logic to display only allowed pin types
        private void PinsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RowCnt = 0;
            ColCnt = 0;
            SingleBtnCnt = 0;
            AxesCnt = 0;

            // count buttons
            for (int i = 0; i < _pins.Count; i++)
            {
                if (_pins[i].SelectedType == PinType.ButtonGnd || _pins[i].SelectedType == PinType.ButtonVcc)
                {
                    SingleBtnCnt++;
                }
                else if (_pins[i].SelectedType == PinType.ButtonRow)
                {
                    RowCnt++;
                }
                else if (_pins[i].SelectedType == PinType.ButtonColumn)
                {
                    ColCnt++;
                }
                else if (_pins[i].SelectedType == PinType.AxisAnalog)
                {
                    AxesCnt++;
                }
            }

            // section of disabling not allowed types
            if (maxBtnCnt - SingleBtnCnt < RowCnt * (ColCnt + 1))
            {
                for (int i = 0; i < _pins.Count; i++)
                {
                    if (_pins[i].SelectedType != PinType.ButtonColumn)
                    {
                        _pins[i].AllowedTypes.Remove(PinType.ButtonColumn);
                    }
                }
            }
            if (maxBtnCnt - SingleBtnCnt < ColCnt * (RowCnt + 1))
            {
                for (int i = 0; i < _pins.Count; i++)
                {
                    if (_pins[i].SelectedType != PinType.ButtonRow)
                        _pins[i].AllowedTypes.Remove(PinType.ButtonRow);
                }
            }
            if (maxBtnCnt - SingleBtnCnt == RowCnt * ColCnt)
            {
                for (int i = 0; i < _pins.Count; i++)
                {
                    if (_pins[i].SelectedType != PinType.ButtonGnd &&
                        _pins[i].SelectedType != PinType.ButtonRow && _pins[i].SelectedType != PinType.ButtonColumn)
                    {
                        _pins[i].AllowedTypes.Remove(PinType.ButtonGnd);
                    }
                    if (_pins[i].SelectedType != PinType.ButtonVcc &&
                        _pins[i].SelectedType != PinType.ButtonRow && _pins[i].SelectedType != PinType.ButtonColumn)
                    {
                        _pins[i].AllowedTypes.Remove(PinType.ButtonVcc);
                    }
                    
                }
            }
            if (maxBtnCnt >= RowCnt * ColCnt)
            {
                for (int i = 0; i < _pins.Count; i++)
                {
                    if (_pins[i].SelectedType == PinType.ButtonRow && !_pins[i].AllowedTypes.Contains(PinType.ButtonColumn))
                    {
                        _pins[i].AllowedTypes.Add(PinType.ButtonColumn);
                    }
                    if (_pins[i].SelectedType == PinType.ButtonColumn && !_pins[i].AllowedTypes.Contains(PinType.ButtonRow))
                    {
                        _pins[i].AllowedTypes.Add(PinType.ButtonRow);
                    }
                }
            }

            // section for enabling allowed types
            if (maxBtnCnt - SingleBtnCnt > ColCnt * (RowCnt + 1))
            {
                for (int i = 0; i < _pins.Count; i++)
                {
                    if (!_pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) _pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!_pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) _pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                    if (!_pins[i].AllowedTypes.Contains(PinType.ButtonRow)) _pins[i].AllowedTypes.Add(PinType.ButtonRow);
                }
            }
            else if (maxBtnCnt - SingleBtnCnt > RowCnt * (ColCnt + 1))
            {
                for (int i = 0; i < _pins.Count; i++)
                {
                    if (!_pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) _pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!_pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) _pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                    if (!_pins[i].AllowedTypes.Contains(PinType.ButtonColumn)) _pins[i].AllowedTypes.Add(PinType.ButtonColumn);
                }
            }
            else if (maxBtnCnt - SingleBtnCnt > RowCnt * ColCnt)
            {
                for (int i = 0; i < _pins.Count; i++)
                {
                    if (!_pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) _pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!_pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) _pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                }
            }

            // let bindings know about changes
            RaisePropertyChanged(nameof(Pins));
        }

        public class PinConverter : BindableBase
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

            public PinConverter()
            {
                _allowedTypes = new ObservableCollection<PinType>()
                {   PinType.NotUsed,
                    PinType.ButtonGnd,
                    PinType.ButtonVcc,
                    PinType.ButtonRow,
                    PinType.ButtonColumn };

                _selectedType = PinType.NotUsed;
                _typeError = false;
            }
        }
    }
}
