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
        private int _totalBtnCnt;
        private int _axesCnt;
        private int _axesToButtonsCnt;
        private ObservableCollection<PinVMConverter> _pins;

        public delegate void PinConfigChangedEvent();

        public event PinConfigChangedEvent ConfigChanged;

        public DeviceConfig Config { get; set; }

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
            ObservableCollection<PinVMConverter> tmp = new ObservableCollection<PinVMConverter>();

            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                tmp.Add(new PinVMConverter());
                if (i < 8) tmp[i].AllowedTypes.Add(PinType.AxisAnalog);
                if (i < 16) tmp[i].AllowedTypes.Add(PinType.AxisToButtons);
                tmp[i].SelectedType = config.PinConfig[i];
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
                if (i < 16) Pins[i].AllowedTypes.Add(PinType.AxisToButtons);
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
            RowCnt = 0;
            ColCnt = 0;
            SingleBtnCnt = 0;
            AxesCnt = 0;
            AxesToButtonsCnt = 0;

            // count buttons
            for (int i = 0; i < Pins.Count; i++)
            {
                if (Pins[i].SelectedType == PinType.ButtonGnd || Pins[i].SelectedType == PinType.ButtonVcc)
                {
                    SingleBtnCnt++;
                }
                else if (Pins[i].SelectedType == PinType.ButtonRow)
                {
                    RowCnt++;
                }
                else if (Pins[i].SelectedType == PinType.ButtonColumn)
                {
                    ColCnt++;
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
            if (maxBtnCnt - SingleBtnCnt < RowCnt * (ColCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.ButtonColumn)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ButtonColumn);
                    }
                }
            }
            if (maxBtnCnt - SingleBtnCnt < ColCnt * (RowCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.ButtonRow)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ButtonRow);
                    }
                }
            }
            if (maxBtnCnt - SingleBtnCnt == RowCnt * ColCnt)
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
            if (maxBtnCnt - SingleBtnCnt >= ColCnt * (RowCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonRow)) Pins[i].AllowedTypes.Add(PinType.ButtonRow);
                }
            }
            if (maxBtnCnt - SingleBtnCnt >= RowCnt * (ColCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonColumn)) Pins[i].AllowedTypes.Add(PinType.ButtonColumn);
                }
            }
            if (maxBtnCnt - SingleBtnCnt > RowCnt * ColCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[i].AllowedTypes.Add(PinType.ButtonGnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[i].AllowedTypes.Add(PinType.ButtonVcc);
                }
            }
            //if (RowCnt*ColCnt > 0 && (maxBtnCnt /RowCnt == ColCnt+1 || maxBtnCnt/ColCnt == RowCnt))
            //{
            //    for (int i = 0; i < Pins.Count; i++)
            //    {
            //        if (Pins[i].SelectedType == PinType.ButtonGnd || Pins[i].SelectedType == PinType.ButtonGnd)
            //        {
            //            if (!Pins[i].AllowedTypes.Contains(PinType.ButtonRow)) Pins[i].AllowedTypes.Add(PinType.ButtonRow);
            //            if (!Pins[i].AllowedTypes.Contains(PinType.ButtonColumn)) Pins[i].AllowedTypes.Add(PinType.ButtonColumn);
            //        }
            //    }
            //}

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
