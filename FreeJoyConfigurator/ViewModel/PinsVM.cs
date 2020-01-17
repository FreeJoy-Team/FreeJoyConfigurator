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
        const int maxShiftRegistersCnt = 4;
        const int maxAxesCnt = 8;

        private int _shiftRegisterDataCnt;
        private int _shiftRegisterCsCnt;
        private int _rowCnt;
        private int _colCnt;
        private int _singleBtnCnt;
        private int _axesCnt;
        private int _axesToButtonsCnt;
        private int _spiDevicesCnt;
        private int _tleCnt;
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
                //if (i < 8) Pins[i].AllowedTypes.Add(PinType.AxisAnalog);
                //if (i < 8) Pins[i].AllowedTypes.Add(PinType.AxisToButtons);
                //if (i == 14)
                //{
                //    Pins[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                //    Pins[i].AllowedTypes.Remove(PinType.ShiftReg_CS);
                //    Pins[i].AllowedTypes.Remove(PinType.ShiftReg_Data);
                //    Pins[i].AllowedTypes.Add(PinType.SPI_SCK);
                //}
                //if (i == 16 || i == 17)
                //{
                //    Pins[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                //}
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
                if (i == 14)
                {
                    tmp[i].AllowedTypes.Remove(PinType.TLE501x_CS);
                    tmp[i].AllowedTypes.Remove(PinType.ShiftReg_CS);
                    tmp[i].AllowedTypes.Remove(PinType.ShiftReg_DATA);
                    if (!tmp[i].AllowedTypes.Contains(PinType.SPI_SCK)) tmp[i].AllowedTypes.Add(PinType.SPI_SCK);
                }
                if (i == 16)
                {
                    tmp[i].AllowedTypes.Remove(PinType.TLE501x_CS);
                    if (!tmp[i].AllowedTypes.Contains(PinType.TLE501x_DATA)) tmp[i].AllowedTypes.Add(PinType.TLE501x_DATA);
                }
                if (i == 17)
                {
                    tmp[i].AllowedTypes.Remove(PinType.TLE501x_CS);
                    if (!tmp[i].AllowedTypes.Contains(PinType.TLE501x_GEN)) tmp[i].AllowedTypes.Add(PinType.TLE501x_GEN);
                }
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
                if (i == 14)
                {
                    Pins[i].AllowedTypes.Remove(PinType.TLE501x_CS);
                    Pins[i].AllowedTypes.Remove(PinType.ShiftReg_CS);
                    Pins[i].AllowedTypes.Remove(PinType.ShiftReg_DATA);
                    if (!Pins[i].AllowedTypes.Contains(PinType.SPI_SCK)) Pins[i].AllowedTypes.Add(PinType.SPI_SCK);
                }
                if (i == 16)
                {
                    Pins[i].AllowedTypes.Remove(PinType.TLE501x_CS);
                    if (!Pins[i].AllowedTypes.Contains(PinType.TLE501x_DATA)) Pins[i].AllowedTypes.Add(PinType.TLE501x_DATA);
                }
                if (i == 17)
                {
                    Pins[i].AllowedTypes.Remove(PinType.TLE501x_CS);
                    if (!Pins[i].AllowedTypes.Contains(PinType.TLE501x_GEN)) Pins[i].AllowedTypes.Add(PinType.TLE501x_GEN);
                }
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
            _shiftRegisterCsCnt = 0;
            _shiftRegisterDataCnt = 0;
            _rowCnt = 0;
            _colCnt = 0;
            _singleBtnCnt = 0;

            AxesCnt = 0;
            AxesToButtonsCnt = 0;
            _spiDevicesCnt = 0;
            _tleCnt = 0;

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
                else if (Pins[i].SelectedType == PinType.ShiftReg_CS)
                {
                    _shiftRegisterCsCnt++;
                    _spiDevicesCnt++;
                }
                else if (Pins[i].SelectedType == PinType.TLE501x_CS)
                {
                    AxesCnt++;
                    _spiDevicesCnt++;
                    _tleCnt++;
                }
            }
            for (int i=0; i<Config.AxisToButtonsConfig.Count;i++)
            {
                if (Config.AxisToButtonsConfig[i].IsEnabled)
                {
                    AxesToButtonsCnt++;
                }
            }

            // SPI pins management
            if (_spiDevicesCnt <= 0)
            {
                if (!Pins[14].AllowedTypes.Contains(PinType.NotUsed)) Pins[14].AllowedTypes.Insert(0,PinType.NotUsed);
                if (!Pins[14].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[14].AllowedTypes.Insert(1, PinType.ButtonGnd);
                if (!Pins[14].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[14].AllowedTypes.Insert(2, PinType.ButtonVcc);
                if (!Pins[14].AllowedTypes.Contains(PinType.ButtonRow)) Pins[14].AllowedTypes.Insert(3, PinType.ButtonRow);
                if (!Pins[14].AllowedTypes.Contains(PinType.ButtonColumn)) Pins[14].AllowedTypes.Insert(4, PinType.ButtonColumn);
                if (!Pins[14].AllowedTypes.Contains(PinType.SPI_SCK)) Pins[14].AllowedTypes.Insert(5, PinType.SPI_SCK);

                Pins[14].AllowedTypes.Remove(PinType.TLE501x_CS);
                Pins[14].AllowedTypes.Remove(PinType.ShiftReg_CS);
                Pins[14].AllowedTypes.Remove(PinType.ShiftReg_DATA);
            }
            if (_tleCnt <= 0)
            {
                if (!Pins[16].AllowedTypes.Contains(PinType.NotUsed)) Pins[16].AllowedTypes.Insert(0, PinType.NotUsed);
                if (!Pins[16].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[16].AllowedTypes.Insert(1, PinType.ButtonGnd);
                if (!Pins[16].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[16].AllowedTypes.Insert(2, PinType.ButtonVcc);
                if (!Pins[16].AllowedTypes.Contains(PinType.ButtonRow)) Pins[16].AllowedTypes.Insert(3, PinType.ButtonRow);
                if (!Pins[16].AllowedTypes.Contains(PinType.ButtonColumn)) Pins[16].AllowedTypes.Insert(4, PinType.ButtonColumn);
                if (!Pins[16].AllowedTypes.Contains(PinType.TLE501x_DATA)) Pins[16].AllowedTypes.Insert(5, PinType.TLE501x_DATA);

                Pins[16].AllowedTypes.Remove(PinType.TLE501x_CS);
                Pins[16].AllowedTypes.Remove(PinType.ShiftReg_CS);
                Pins[16].AllowedTypes.Remove(PinType.ShiftReg_DATA);

                if (!Pins[17].AllowedTypes.Contains(PinType.NotUsed)) Pins[17].AllowedTypes.Insert(0, PinType.NotUsed);
                if (!Pins[17].AllowedTypes.Contains(PinType.ButtonGnd)) Pins[17].AllowedTypes.Insert(1, PinType.ButtonGnd);
                if (!Pins[17].AllowedTypes.Contains(PinType.ButtonVcc)) Pins[17].AllowedTypes.Insert(2, PinType.ButtonVcc);
                if (!Pins[17].AllowedTypes.Contains(PinType.ButtonRow)) Pins[17].AllowedTypes.Insert(3, PinType.ButtonRow);
                if (!Pins[17].AllowedTypes.Contains(PinType.ButtonColumn)) Pins[17].AllowedTypes.Insert(4, PinType.ButtonColumn);
                if (!Pins[17].AllowedTypes.Contains(PinType.TLE501x_GEN)) Pins[17].AllowedTypes.Insert(5, PinType.TLE501x_GEN);

                Pins[17].AllowedTypes.Remove(PinType.TLE501x_CS);
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
            if (_shiftRegisterCsCnt >= maxShiftRegistersCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.ShiftReg_CS)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ShiftReg_CS);
                    }
                }
            }
            if (_shiftRegisterDataCnt >= maxShiftRegistersCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.ShiftReg_DATA)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ShiftReg_DATA);
                    }
                }
            }
            if (AxesCnt >= maxAxesCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.TLE501x_CS )
                    {
                        Pins[i].AllowedTypes.Remove(PinType.TLE501x_CS);
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
            if (_shiftRegisterCsCnt < maxShiftRegistersCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ShiftReg_CS) && i != 14)
                    {
                        Pins[i].AllowedTypes.Add(PinType.ShiftReg_CS);
                    }
                }
            }
            if (_shiftRegisterDataCnt < maxShiftRegistersCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ShiftReg_DATA) && i != 14)
                    {
                        Pins[i].AllowedTypes.Add(PinType.ShiftReg_DATA);
                    }
                }
            }
            if (AxesCnt < maxAxesCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.TLE501x_CS) && i != 14 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.TLE501x_CS);
                    }
                }
            }

            // SPI pins management
            if (_spiDevicesCnt > 0)
            {
                Pins[14].AllowedTypes.Clear();                
                Pins[14].AllowedTypes.Add(PinType.SPI_SCK);
                Pins[14].SelectedType = PinType.SPI_SCK;
            }
            if (_tleCnt > 0)
            {
                Pins[16].AllowedTypes.Clear();
                Pins[16].AllowedTypes.Add(PinType.TLE501x_DATA);
                Pins[16].SelectedType = PinType.TLE501x_DATA;

                Pins[17].SelectedType = PinType.TLE501x_GEN;
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
