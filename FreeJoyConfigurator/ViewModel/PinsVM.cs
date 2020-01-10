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
        #region Fields
        const int maxBtnCnt = 128;
        private int _rowCnt;
        private int _colCnt;
        private int _singleBtnCnt;
        private int _axesCnt;
        private int _axesToButtonsCnt;
        private ObservableCollection<PinVMConverter> _pins;

        public delegate void PinConfigChangedEvent();

        public event PinConfigChangedEvent ConfigChanged;

        private DeviceConfig _config;

        public DeviceConfig Config
        {
            get
            {
                return _config;
            }
            set
            {
                SetProperty(ref _config, value);
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
        public int AxesToButtonsCnt
        {
            get
            {
                return _axesToButtonsCnt;
            }
            private set
            {
                SetProperty(ref _axesToButtonsCnt, value);
            }
        }
        public ObservableCollection<PinVMConverter> Pins
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
        #endregion

        #region Constructor
        public PinsVM(DeviceConfig deviceConfig)
        {
            Config = deviceConfig;

            _pins = new ObservableCollection<PinVMConverter>();
            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                Pins.Add(new PinVMConverter());
                if (i < 8) Pins[i].AllowedTypes.Add(PinType.AxisAnalog);
                if (i < 16) Pins[i].AllowedTypes.Add(PinType.AxisToButtons);
                Pins[i].PropertyChanged += PinsVM_PropertyChanged;
            }
        }


        #endregion

        #region Public methods
        public void Update(DeviceConfig config)
        {
            Config = config;

            ObservableCollection<PinVMConverter> tmp = new ObservableCollection<PinVMConverter>();

            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                tmp.Add(new PinVMConverter());
                if (i < 8) tmp[i].AllowedTypes.Add(PinType.AxisAnalog);
                if (i < 8) tmp[i].AllowedTypes.Add(PinType.AxisToButtons);
                tmp[i].SelectedType = Config.PinConfig[i];
            }
            Pins = new ObservableCollection<PinVMConverter>(tmp);

            foreach (var pin in Pins) pin.PropertyChanged += PinsVM_PropertyChanged;

            RaisePropertyChanged(nameof(Pins));
            PinsVM_PropertyChanged(this, null);
        }

        public void ResetPins()
        {
            for (int i = 0; i < Pins.Count; i++)
            {
                _pins[i] = new PinVMConverter();
                if (i < 8) Pins[i].AllowedTypes.Add(PinType.AxisAnalog);
                if (i < 8) Pins[i].AllowedTypes.Add(PinType.AxisToButtons);
                Pins[i].PropertyChanged += PinsVM_PropertyChanged;
            }
            // update config
            DeviceConfig tmp = Config;
            for (int i = 0; i < tmp.PinConfig.Count; i++)
            {
                tmp.PinConfig[i] = Pins[i].SelectedType;
            }
            Config = tmp;
            PinsVM_PropertyChanged(this, null);
        }
        #endregion

        #region Local methods

        // Some dirty logic to display only allowed pin types
        private void PinsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _rowCnt = 0;
            _colCnt = 0;
            _singleBtnCnt = 0;

            AxesCnt = 0;
            AxesToButtonsCnt = 0;

            // count buttons and axes
            for (int i = 0; i < Pins.Count; i++)
            {
                if (Pins[i].SelectedType == PinType.ButtonGnd || Pins[i].SelectedType == PinType.ButtonVcc)
                {
                    _singleBtnCnt++;
                }
                else if (Pins[i].SelectedType == PinType.ButtonRow)
                {
                    _rowCnt++;
                }
                else if (Pins[i].SelectedType == PinType.ButtonColumn)
                {
                    _colCnt++;
                }
                else if (Pins[i].SelectedType == PinType.AxisAnalog)
                {
                    AxesCnt++;
                }
                else if (Pins[i].SelectedType == PinType.AxisToButtons)
                {
                    AxesToButtonsCnt++;
                }
            }

            // section of disabling not allowed types
            if (maxBtnCnt - _singleBtnCnt < _rowCnt * (_colCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.ButtonColumn)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ButtonColumn);
                    }
                }
            }
            if (maxBtnCnt - _singleBtnCnt < _colCnt * (_rowCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.ButtonRow)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ButtonRow);
                    }
                }
            }
            if (maxBtnCnt - _singleBtnCnt == _rowCnt * _colCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.ButtonGnd &&
                        Pins[i].SelectedType != PinType.ButtonRow && Pins[i].SelectedType != PinType.ButtonColumn)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ButtonGnd);
                    }
                    if (Pins[i].SelectedType != PinType.ButtonVcc &&
                        Pins[i].SelectedType != PinType.ButtonRow && Pins[i].SelectedType != PinType.ButtonColumn)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ButtonVcc);
                    }
                    
                }
            }

            // section for enabling allowed types
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType == PinType.ButtonRow && !Pins[i].AllowedTypes.Contains(PinType.ButtonColumn))
                    {
                        Pins[i].AllowedTypes.Add(PinType.ButtonColumn);
                    }
                    if (Pins[i].SelectedType == PinType.ButtonColumn && !Pins[i].AllowedTypes.Contains(PinType.ButtonRow))
                    {
                        Pins[i].AllowedTypes.Add(PinType.ButtonRow);
                    }
                }        
            if (maxBtnCnt - _singleBtnCnt >= _colCnt * (_rowCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonRow)) Pins[i].AllowedTypes.Add(PinType.ButtonRow);
                }
            }
            if (maxBtnCnt - _singleBtnCnt >= _rowCnt * (_colCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonColumn)) Pins[i].AllowedTypes.Add(PinType.ButtonColumn);
                }
            }
            if (maxBtnCnt - _singleBtnCnt > _rowCnt * _colCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                }
            }

            // update config
            DeviceConfig tmp = Config;
            for (int i=0; i<tmp.PinConfig.Count;i++)
            {
                tmp.PinConfig[i] = Pins[i].SelectedType;
            }
            Config = tmp;

            // let bindings know about changes
            RaisePropertyChanged(nameof(Pins));
            ConfigChanged();
        }
        #endregion

    }
}
