using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class LedVM : BindableBase
    {
        private int _totalLedCnt;

        private DeviceConfig _config;
        private ObservableCollection<Led> _leds;
        private Joystick _joystick;

        public delegate void ButtonsChangedEvent();
        public event ButtonsChangedEvent ConfigChanged;

        public DeviceConfig Config
        {
            get { return _config; }
            set { SetProperty(ref _config, value); }
        }

        public ObservableCollection<Led> Leds
        {
            get { return _leds; }
            set { SetProperty(ref _leds, value); }
        }

        public LedVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            _joystick = joystick;
            _config = deviceConfig;

            _leds = new ObservableCollection<Led>();
            foreach (var led in _leds) led.PropertyChanged += Led_PropertyChanged;
        }

        public void Update(DeviceConfig config)
        {
            Config = config;

            ObservableCollection<Led> tmp = new ObservableCollection<Led>();

            _totalLedCnt = 0;
            for (int i=0; i<config.PinConfig.Count;i++)
            {
                // led matrix
                if (config.PinConfig[i] == PinType.LED_Row)
                {
                    for (int k = 0; k < config.PinConfig.Count; k++)
                    {
                        if (config.PinConfig[k] == PinType.LED_Column)
                        {
                            tmp.Add(new Led(_totalLedCnt + 1, config.LedConfig[_totalLedCnt].InputNumber, config.LedConfig[_totalLedCnt].Type));
                            _totalLedCnt++;
                        }
                    }
                }
                else if (config.PinConfig[i] == PinType.LED_Single)
                {
                    tmp.Add(new Led(_totalLedCnt + 1, config.LedConfig[_totalLedCnt].InputNumber, config.LedConfig[_totalLedCnt].Type));
                    _totalLedCnt++;
                }
            }

            Leds = tmp;

            for (int i = 0; i < Leds.Count; i++)
            {
                Leds[i].PropertyChanged += Led_PropertyChanged;
            }

            Led_PropertyChanged(null, null);
        }

        private void Led_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DeviceConfig tmp = Config;
            for (int i = 0; i < Leds.Count; i++)
            {
                tmp.LedConfig[i].InputNumber = (sbyte)Leds[i].InputNumber;
                tmp.LedConfig[i].Type = Leds[i].Type;
            }
            Config = tmp;

            RaisePropertyChanged(nameof(Leds));
            ConfigChanged();
        }
    }  
}
