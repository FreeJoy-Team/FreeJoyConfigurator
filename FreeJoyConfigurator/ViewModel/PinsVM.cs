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
        const int maxLedCnt = 24;
        const int maxShiftRegistersCnt = 4;
        const int maxSensorCnt = 8;
       
        private int _rowCnt;
        private int _colCnt;
        private int _singleBtnCnt;
        private int _SensorCnt;
        private int _axesToButtonsCnt;

        private int _shiftRegisterDataCnt;
        private int _shiftRegisterCsCnt;

        private int _spiDevicesCnt;
        private int _spiHalfDuplexCnt;
        private int _spiFullDuplexCnt;

        private bool _genEnabled;

        private bool _isI2cEnabled;

        private int _ledRowCnt;
        private int _ledColCnt;
        private int _singleLedCnt;
        private int _totalLedCnt;

        private bool _axesError;        
        private bool _ledsError;

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

        public int SensorCnt
        {
            get
            {
                return _SensorCnt;
            }
            private set
            {
                SetProperty(ref _SensorCnt, value);
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
        public int TotalLedCnt
        {
            get
            {
                return _totalLedCnt;
            }
            private set
            {
                SetProperty(ref _totalLedCnt, value);
            }
        }
        public bool AxesError
        {
            get
            {
                return _axesError;
            }
            private set
            {
                SetProperty(ref _axesError, value);
            }
        }
        
        public bool LedsError
        {
            get
            {
                return _ledsError;
            }
            private set
            {
                SetProperty(ref _ledsError, value);
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
                if (i < 8) tmp[i].AllowedTypes.Add(PinType.Axis_Analog);
                if (i == 8 || i== 9) tmp[i].AllowedTypes.Add(PinType.Fast_Encoder);
                if (i == 8 || i == 12 || i == 13 || i == 15)
                {
                    if (!tmp[i].AllowedTypes.Contains(PinType.LED_PWM)) tmp[i].AllowedTypes.Add(PinType.LED_PWM);
                }
                if (i == 14)
                {
                    tmp[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                    tmp[i].AllowedTypes.Remove(PinType.TLE5012B_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3201_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3202_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3204_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3208_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MLX90393_CS);
                    tmp[i].AllowedTypes.Remove(PinType.AS5048A_CS);
                    tmp[i].AllowedTypes.Remove(PinType.ShiftReg_LATCH);
                    tmp[i].AllowedTypes.Remove(PinType.ShiftReg_DATA);
                    if (!tmp[i].AllowedTypes.Contains(PinType.SPI_SCK)) tmp[i].AllowedTypes.Add(PinType.SPI_SCK);
                }
                if (i == 15)
                {
                    tmp[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                    tmp[i].AllowedTypes.Remove(PinType.TLE5012B_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3201_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3202_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3204_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3208_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MLX90393_CS);
                    tmp[i].AllowedTypes.Remove(PinType.AS5048A_CS);
                    if (!tmp[i].AllowedTypes.Contains(PinType.SPI_MISO)) tmp[i].AllowedTypes.Add(PinType.SPI_MISO);
                }
                if (i == 16)
                {
                    tmp[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                    tmp[i].AllowedTypes.Remove(PinType.TLE5012B_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3201_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3202_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3204_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MCP3208_CS);
                    tmp[i].AllowedTypes.Remove(PinType.MLX90393_CS);
                    tmp[i].AllowedTypes.Remove(PinType.AS5048A_CS);
                    if (!tmp[i].AllowedTypes.Contains(PinType.SPI_MOSI)) tmp[i].AllowedTypes.Add(PinType.SPI_MOSI);
                }
                if (i == 17)
                {
                    tmp[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                    if (!tmp[i].AllowedTypes.Contains(PinType.TLE5011_GEN)) tmp[i].AllowedTypes.Add(PinType.TLE5011_GEN);
                }
                if (i == 21)
                {
                    if (!tmp[i].AllowedTypes.Contains(PinType.I2C_SCL)) tmp[i].AllowedTypes.Add(PinType.I2C_SCL);
                }
                if (i == 22)
                {
                    if (!tmp[i].AllowedTypes.Contains(PinType.I2C_SDA)) tmp[i].AllowedTypes.Add(PinType.I2C_SDA);
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
                if (i < 8) Pins[i].AllowedTypes.Add(PinType.Axis_Analog);
                if (i == 8 || i == 9) Pins[i].AllowedTypes.Add(PinType.Fast_Encoder);
                if (i == 8 || i == 12 || i == 13 || i == 15)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.LED_PWM)) Pins[i].AllowedTypes.Add(PinType.LED_PWM);
                }
                if (i == 14)
                {
                    Pins[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                    Pins[i].AllowedTypes.Remove(PinType.TLE5012B_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3201_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3202_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3204_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3208_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MLX90393_CS);
                    Pins[i].AllowedTypes.Remove(PinType.AS5048A_CS);
                    Pins[i].AllowedTypes.Remove(PinType.ShiftReg_LATCH);
                    Pins[i].AllowedTypes.Remove(PinType.ShiftReg_DATA);
                    if (!Pins[i].AllowedTypes.Contains(PinType.SPI_SCK)) Pins[i].AllowedTypes.Add(PinType.SPI_SCK);
                }
                if (i == 15)
                {
                    Pins[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                    Pins[i].AllowedTypes.Remove(PinType.TLE5012B_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3201_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3202_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3204_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3208_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MLX90393_CS);
                    Pins[i].AllowedTypes.Remove(PinType.AS5048A_CS);
                    if (!Pins[i].AllowedTypes.Contains(PinType.SPI_MISO)) Pins[i].AllowedTypes.Add(PinType.SPI_MISO);
                }
                if (i == 16)
                {
                    Pins[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                    Pins[i].AllowedTypes.Remove(PinType.TLE5012B_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3201_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3202_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3204_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MCP3208_CS);
                    Pins[i].AllowedTypes.Remove(PinType.MLX90393_CS);
                    Pins[i].AllowedTypes.Remove(PinType.AS5048A_CS);
                    if (!Pins[i].AllowedTypes.Contains(PinType.SPI_MOSI)) Pins[i].AllowedTypes.Add(PinType.SPI_MOSI);
                }
                if (i == 17)
                {
                    Pins[i].AllowedTypes.Remove(PinType.TLE5011_CS);
                    if (!Pins[i].AllowedTypes.Contains(PinType.TLE5011_GEN)) Pins[i].AllowedTypes.Add(PinType.TLE5011_GEN);
                }
                if (i == 21)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.I2C_SCL)) Pins[i].AllowedTypes.Add(PinType.I2C_SCL);
                }
                if (i == 22)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.I2C_SDA)) Pins[i].AllowedTypes.Add(PinType.I2C_SDA);
                }
                Pins[i].PropertyChanged += PinsVM_PropertyChanged;
            }
            // update config
            //DeviceConfig tmp = Config;
            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                Config.PinConfig[i] = Pins[i].SelectedType;
            }
            //Config = tmp;
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
            _ledColCnt = 0;
            _ledRowCnt = 0;
            _singleLedCnt = 0;
            TotalLedCnt = 0;

            SensorCnt = 0;
            AxesToButtonsCnt = 0;
            _spiDevicesCnt = 0;
            _spiHalfDuplexCnt = 0;
            _spiFullDuplexCnt = 0;

            _genEnabled = false;

            _isI2cEnabled = false;

            AxesError = false;
            LedsError = false;

            // count buttons and axes
            for (int i = 0; i < Pins.Count; i++)
            {
                if (Pins[i].SelectedType == PinType.Button_Gnd || Pins[i].SelectedType == PinType.Button_Vcc)
                {
                    _singleBtnCnt++;
                }
                else if (Pins[i].SelectedType == PinType.Button_Row)
                {
                    _rowCnt++;
                }
                else if (Pins[i].SelectedType == PinType.Button_Column)
                {
                    _colCnt++;
                }
                else if (Pins[i].SelectedType == PinType.Axis_Analog)
                {
                    SensorCnt++;
                }
                else if (Pins[i].SelectedType == PinType.ShiftReg_LATCH)
                {
                    _shiftRegisterCsCnt++;
                    _spiDevicesCnt++;
                }
                else if (Pins[i].SelectedType == PinType.TLE5011_CS)
                {
                    SensorCnt++;
                    _spiDevicesCnt++;
                    _spiHalfDuplexCnt++;
                    _genEnabled = true;
                }
                else if (Pins[i].SelectedType == PinType.TLE5012B_CS)
                {
                    SensorCnt++;
                    _spiDevicesCnt++;
                    _spiHalfDuplexCnt++;
                }
                else if (Pins[i].SelectedType == PinType.MCP3201_CS)
                {
                    SensorCnt++;
                    _spiDevicesCnt++;
                    _spiFullDuplexCnt++;
                }
                else if (Pins[i].SelectedType == PinType.MCP3202_CS)
                {
                    SensorCnt++;
                    _spiDevicesCnt++;
                    _spiFullDuplexCnt++;
                }
                else if (Pins[i].SelectedType == PinType.MCP3204_CS)
                {
                    SensorCnt++;
                    _spiDevicesCnt++;
                    _spiFullDuplexCnt++;
                }
                else if (Pins[i].SelectedType == PinType.MCP3208_CS)
                {
                    SensorCnt++;
                    _spiDevicesCnt++;
                    _spiFullDuplexCnt++;
                }
                else if (Pins[i].SelectedType == PinType.MLX90393_CS)
                {
                    SensorCnt++;
                    _spiDevicesCnt++;
                    _spiFullDuplexCnt++;
                }
                else if (Pins[i].SelectedType == PinType.AS5048A_CS)
                {
                    SensorCnt++;
                    _spiDevicesCnt++;
                    _spiFullDuplexCnt++;
                }
                else if (Pins[i].SelectedType == PinType.LED_Single)
                {
                    _singleLedCnt++;
                }
                else if (Pins[i].SelectedType == PinType.LED_Row)
                {
                    _ledRowCnt++;
                }
                else if (Pins[i].SelectedType == PinType.LED_Column)
                {
                    _ledColCnt++;
                }
                else if (Pins[i].SelectedType == PinType.I2C_SCL)
                {
                    _isI2cEnabled = true;
                }
            }
            for (int i=0; i<Config.AxisToButtonsConfig.Count;i++)
            {
                if (Config.AxisToButtonsConfig[i].ButtonsCnt > 0)
                {
                    AxesToButtonsCnt++;
                }
            }

            TotalLedCnt = _singleLedCnt + _ledColCnt * _ledRowCnt;

            if (SensorCnt > maxSensorCnt) AxesError = true;
            if (TotalLedCnt > maxLedCnt) LedsError = true;

            // SPI and I2C pins management
            if (_spiDevicesCnt <= 0)
            {
                if (!Pins[14].AllowedTypes.Contains(PinType.Not_Used)) Pins[14].AllowedTypes.Insert(0,PinType.Not_Used);
                if (!Pins[14].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[14].AllowedTypes.Insert(1, PinType.Button_Gnd);
                if (!Pins[14].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[14].AllowedTypes.Insert(2, PinType.Button_Vcc);
                if (!Pins[14].AllowedTypes.Contains(PinType.Button_Row)) Pins[14].AllowedTypes.Insert(3, PinType.Button_Row);
                if (!Pins[14].AllowedTypes.Contains(PinType.Button_Column)) Pins[14].AllowedTypes.Insert(4, PinType.Button_Column);
                if (!Pins[14].AllowedTypes.Contains(PinType.SPI_SCK)) Pins[14].AllowedTypes.Insert(5, PinType.SPI_SCK);

                //Pins[14].AllowedTypes.Remove(PinType.TLE5011_CS);
                //Pins[14].AllowedTypes.Remove(PinType.TLE5012B_CS);
                //Pins[14].AllowedTypes.Remove(PinType.MCP3201_CS);
                //Pins[14].AllowedTypes.Remove(PinType.MCP3202_CS);
                //Pins[14].AllowedTypes.Remove(PinType.MCP3204_CS);
                //Pins[14].AllowedTypes.Remove(PinType.MCP3208_CS);
                //Pins[14].AllowedTypes.Remove(PinType.MLX90393_CS);
                //Pins[14].AllowedTypes.Remove(PinType.AS5048A_CS);
                //Pins[14].AllowedTypes.Remove(PinType.ShiftReg_LATCH);
                //Pins[14].AllowedTypes.Remove(PinType.ShiftReg_DATA);
                

            }
            if (_spiFullDuplexCnt <= 0)
            {
                if (!Pins[15].AllowedTypes.Contains(PinType.Not_Used)) Pins[15].AllowedTypes.Insert(0, PinType.Not_Used);
                if (!Pins[15].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[15].AllowedTypes.Insert(1, PinType.Button_Gnd);
                if (!Pins[15].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[15].AllowedTypes.Insert(2, PinType.Button_Vcc);
                if (!Pins[15].AllowedTypes.Contains(PinType.Button_Row)) Pins[15].AllowedTypes.Insert(3, PinType.Button_Row);
                if (!Pins[15].AllowedTypes.Contains(PinType.Button_Column)) Pins[15].AllowedTypes.Insert(4, PinType.Button_Column);
                if (!Pins[15].AllowedTypes.Contains(PinType.SPI_MISO)) Pins[15].AllowedTypes.Insert(5, PinType.SPI_MISO);
            }
            if (_spiHalfDuplexCnt <= 0)
            {
                if (!Pins[16].AllowedTypes.Contains(PinType.Not_Used)) Pins[16].AllowedTypes.Insert(0, PinType.Not_Used);
                if (!Pins[16].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[16].AllowedTypes.Insert(1, PinType.Button_Gnd);
                if (!Pins[16].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[16].AllowedTypes.Insert(2, PinType.Button_Vcc);
                if (!Pins[16].AllowedTypes.Contains(PinType.Button_Row)) Pins[16].AllowedTypes.Insert(3, PinType.Button_Row);
                if (!Pins[16].AllowedTypes.Contains(PinType.Button_Column)) Pins[16].AllowedTypes.Insert(4, PinType.Button_Column);
                if (!Pins[16].AllowedTypes.Contains(PinType.SPI_MOSI)) Pins[16].AllowedTypes.Insert(5, PinType.SPI_MOSI);

                //Pins[16].AllowedTypes.Remove(PinType.TLE5011_CS);
                //Pins[16].AllowedTypes.Remove(PinType.TLE5012B_CS);
                //Pins[16].AllowedTypes.Remove(PinType.ShiftReg_LATCH);
                //Pins[16].AllowedTypes.Remove(PinType.ShiftReg_DATA);

                if (!Pins[17].AllowedTypes.Contains(PinType.Not_Used)) Pins[17].AllowedTypes.Insert(0, PinType.Not_Used);
                if (!Pins[17].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[17].AllowedTypes.Insert(1, PinType.Button_Gnd);
                if (!Pins[17].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[17].AllowedTypes.Insert(2, PinType.Button_Vcc);
                if (!Pins[17].AllowedTypes.Contains(PinType.Button_Row)) Pins[17].AllowedTypes.Insert(3, PinType.Button_Row);
                if (!Pins[17].AllowedTypes.Contains(PinType.Button_Column)) Pins[17].AllowedTypes.Insert(4, PinType.Button_Column);
                if (!Pins[17].AllowedTypes.Contains(PinType.TLE5011_GEN)) Pins[17].AllowedTypes.Insert(5, PinType.TLE5011_GEN);

                //Pins[17].AllowedTypes.Remove(PinType.TLE5011_CS);
            }
            if (!_isI2cEnabled)
            {
                if (!Pins[21].AllowedTypes.Contains(PinType.Not_Used)) Pins[21].AllowedTypes.Insert(0, PinType.Not_Used);
                if (!Pins[21].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[21].AllowedTypes.Insert(1, PinType.Button_Gnd);
                if (!Pins[21].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[21].AllowedTypes.Insert(2, PinType.Button_Vcc);
                if (!Pins[21].AllowedTypes.Contains(PinType.Button_Row)) Pins[21].AllowedTypes.Insert(3, PinType.Button_Row);
                if (!Pins[21].AllowedTypes.Contains(PinType.Button_Column)) Pins[21].AllowedTypes.Insert(4, PinType.Button_Column);
                if (!Pins[21].AllowedTypes.Contains(PinType.I2C_SCL)) Pins[21].AllowedTypes.Insert(5, PinType.I2C_SCL);

                if (!Pins[22].AllowedTypes.Contains(PinType.Not_Used)) Pins[22].AllowedTypes.Insert(0, PinType.Not_Used);
                if (!Pins[22].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[22].AllowedTypes.Insert(1, PinType.Button_Gnd);
                if (!Pins[22].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[22].AllowedTypes.Insert(2, PinType.Button_Vcc);
                if (!Pins[22].AllowedTypes.Contains(PinType.Button_Row)) Pins[22].AllowedTypes.Insert(3, PinType.Button_Row);
                if (!Pins[22].AllowedTypes.Contains(PinType.Button_Column)) Pins[22].AllowedTypes.Insert(4, PinType.Button_Column);
                if (!Pins[22].AllowedTypes.Contains(PinType.I2C_SDA)) Pins[22].AllowedTypes.Insert(5, PinType.I2C_SDA);
            }



            // section of disabling not allowed types
            if (maxBtnCnt - _singleBtnCnt < _rowCnt * (_colCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.Button_Column)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.Button_Column);
                    }
                }
            }
            if (maxBtnCnt - _singleBtnCnt < _colCnt * (_rowCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.Button_Row)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.Button_Row);
                    }
                }
            }
            if (maxBtnCnt - _singleBtnCnt <= _rowCnt * _colCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.Button_Gnd &&
                        Pins[i].SelectedType != PinType.Button_Row && Pins[i].SelectedType != PinType.Button_Column)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.Button_Gnd);
                    }
                    if (Pins[i].SelectedType != PinType.Button_Vcc &&
                        Pins[i].SelectedType != PinType.Button_Row && Pins[i].SelectedType != PinType.Button_Column)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.Button_Vcc);
                    }
                    
                }
            }
            if (maxLedCnt - _singleLedCnt < _ledRowCnt * (_ledColCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.LED_Column)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.LED_Column);
                    }
                }
            }
            if (maxLedCnt - _singleLedCnt < _ledColCnt * (_ledRowCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.LED_Row)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.LED_Row);
                    }
                }
            }
            if (maxLedCnt - _singleLedCnt <= _ledRowCnt * _ledRowCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.LED_Single &&
                        Pins[i].SelectedType != PinType.LED_Row && Pins[i].SelectedType != PinType.LED_Column)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.LED_Single);
                    }

                }
            }

            if (_shiftRegisterCsCnt >= maxShiftRegistersCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType != PinType.ShiftReg_LATCH)
                    {
                        Pins[i].AllowedTypes.Remove(PinType.ShiftReg_LATCH);
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
            //if (SensorCnt >= maxSensorCnt)
            //{
            //    for (int i = 0; i < Pins.Count; i++)
            //    {
            //        if (Pins[i].SelectedType != PinType.TLE5011_CS )
            //        {
            //            Pins[i].AllowedTypes.Remove(PinType.TLE5011_CS);
            //        }
            //}

            // section for enabling allowed types
            for (int i = 0; i < Pins.Count; i++)
                {
                    if (Pins[i].SelectedType == PinType.Button_Row && !Pins[i].AllowedTypes.Contains(PinType.Button_Column))
                    {
                        Pins[i].AllowedTypes.Add(PinType.Button_Column);
                    }
                    if (Pins[i].SelectedType == PinType.Button_Column && !Pins[i].AllowedTypes.Contains(PinType.Button_Row))
                    {
                        Pins[i].AllowedTypes.Add(PinType.Button_Row);
                    }
                }        
            if (maxBtnCnt - _singleBtnCnt >= _colCnt * (_rowCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[i].AllowedTypes.Add(PinType.Button_Gnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[i].AllowedTypes.Add(PinType.Button_Vcc);
                    if (!Pins[i].AllowedTypes.Contains(PinType.Button_Row)) Pins[i].AllowedTypes.Add(PinType.Button_Row);
                }
            }
            if (maxBtnCnt - _singleBtnCnt >= _rowCnt * (_colCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[i].AllowedTypes.Add(PinType.Button_Gnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[i].AllowedTypes.Add(PinType.Button_Vcc);
                    if (!Pins[i].AllowedTypes.Contains(PinType.Button_Column)) Pins[i].AllowedTypes.Add(PinType.Button_Column);
                }
            }
            if (maxBtnCnt - _singleBtnCnt > _rowCnt * _colCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.Button_Gnd)) Pins[i].AllowedTypes.Add(PinType.Button_Gnd);
                    if (!Pins[i].AllowedTypes.Contains(PinType.Button_Vcc)) Pins[i].AllowedTypes.Add(PinType.Button_Vcc);
                }
            }

            for (int i = 0; i < Pins.Count; i++)
            {
                if (Pins[i].SelectedType == PinType.LED_Row && !Pins[i].AllowedTypes.Contains(PinType.LED_Column))
                {
                    Pins[i].AllowedTypes.Add(PinType.LED_Column);
                }
                if (Pins[i].SelectedType == PinType.LED_Column && !Pins[i].AllowedTypes.Contains(PinType.LED_Row))
                {
                    Pins[i].AllowedTypes.Add(PinType.LED_Row);
                }
            }
            if (maxLedCnt - _singleLedCnt >= _ledColCnt * (_ledRowCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.LED_Single)) Pins[i].AllowedTypes.Add(PinType.LED_Single);
                    if (!Pins[i].AllowedTypes.Contains(PinType.LED_Row)) Pins[i].AllowedTypes.Add(PinType.LED_Row);
                }
            }
            if (maxLedCnt - _singleLedCnt >= _ledRowCnt * (_ledColCnt + 1))
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.LED_Single)) Pins[i].AllowedTypes.Add(PinType.LED_Single);
                    if (!Pins[i].AllowedTypes.Contains(PinType.LED_Column)) Pins[i].AllowedTypes.Add(PinType.LED_Column);
                }
            }
            if (maxLedCnt - _singleLedCnt > _ledRowCnt * _ledColCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.LED_Single)) Pins[i].AllowedTypes.Add(PinType.LED_Single);
                }
            }

            if (_shiftRegisterCsCnt < maxShiftRegistersCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.ShiftReg_LATCH) && i != 14)
                    {
                        Pins[i].AllowedTypes.Add(PinType.ShiftReg_LATCH);
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
            if (SensorCnt < maxSensorCnt)
            {
                for (int i = 0; i < Pins.Count; i++)
                {
                    if (!Pins[i].AllowedTypes.Contains(PinType.TLE5011_CS) && i != 14 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.TLE5011_CS);
                    }
                    if (!Pins[i].AllowedTypes.Contains(PinType.TLE5012B_CS) && i != 14 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.TLE5012B_CS);
                    }
                    if (!Pins[i].AllowedTypes.Contains(PinType.MCP3201_CS) && i != 14 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.MCP3201_CS);
                    }
                    if (!Pins[i].AllowedTypes.Contains(PinType.MCP3202_CS) && i != 14 && i != 15 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.MCP3202_CS);
                    }
                    if (!Pins[i].AllowedTypes.Contains(PinType.MCP3204_CS) && i != 14 && i != 15 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.MCP3204_CS);
                    }
                    if (!Pins[i].AllowedTypes.Contains(PinType.MCP3208_CS) && i != 14 && i != 15 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.MCP3208_CS);
                    }
                    if (!Pins[i].AllowedTypes.Contains(PinType.MLX90393_CS) && i != 14 && i != 15 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.MLX90393_CS);
                    }
                    if (!Pins[i].AllowedTypes.Contains(PinType.AS5048A_CS) && i != 14 && i != 15 && i != 16 && i != 17)
                    {
                        Pins[i].AllowedTypes.Add(PinType.AS5048A_CS);
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
            if (_spiFullDuplexCnt > 0)
            {
                Pins[15].AllowedTypes.Clear();
                Pins[15].AllowedTypes.Add(PinType.SPI_MISO);
                Pins[15].SelectedType = PinType.SPI_MISO;

                Pins[16].AllowedTypes.Clear();
                Pins[16].AllowedTypes.Add(PinType.SPI_MOSI);
                Pins[16].SelectedType = PinType.SPI_MOSI;
            }
            if (_spiHalfDuplexCnt > 0)
            {
                Pins[16].AllowedTypes.Clear();
                Pins[16].AllowedTypes.Add(PinType.SPI_MOSI);
                Pins[16].SelectedType = PinType.SPI_MOSI;

                
            }
            if (_genEnabled)
            {
                Pins[17].SelectedType = PinType.TLE5011_GEN;
            }
            if (_isI2cEnabled)
            {
                Pins[22].AllowedTypes.Clear();
                Pins[22].AllowedTypes.Add(PinType.I2C_SDA);
                Pins[22].SelectedType = PinType.I2C_SDA;
            }

            // PWM conflicting pin
            if (Pins[16].SelectedType == PinType.SPI_MOSI)
            {
                if (Pins[15].SelectedType == PinType.LED_PWM) Pins[15].SelectedType = PinType.Not_Used;
                Pins[15].AllowedTypes.Remove(PinType.LED_PWM);
            }
            else if (!Pins[15].AllowedTypes.Contains(PinType.LED_PWM)) Pins[15].AllowedTypes.Add(PinType.LED_PWM);


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
