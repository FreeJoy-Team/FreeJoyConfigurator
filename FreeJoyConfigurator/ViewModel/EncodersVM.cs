using System;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace FreeJoyConfigurator
{
    public class EncodersVM : BindableBase
    {
        private DeviceConfig _config;
        private ObservableCollection<Encoder> _encoders;

        public delegate void EncodersChangedEvent();
        public event EncodersChangedEvent ConfigChanged;

        public DeviceConfig Config
        {
            get { return _config; }
            set { SetProperty(ref _config, value); }
        }

        public ObservableCollection<Encoder> Encoders
        {
            get { return _encoders; }
            set { SetProperty(ref _encoders, value); }
        }

        public EncodersVM(DeviceConfig deviceConfig)
        {
            _config = deviceConfig;

            _encoders = new ObservableCollection<Encoder>();

            // fast encoder
            _encoders.Add(new Encoder(0, EncoderType.Encoder_1x, -1, -1));
            
            // polling encoders
            for (int i = 1; i < 16; i++)
            {
                _encoders.Add(new Encoder(i, EncoderType.Encoder_1x));
                _encoders[i].PropertyChanged += EncodersVM_PropertyChanged;
            }

        }

        private void EncodersVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DeviceConfig tmp = Config;

            for (int i = 0; i < tmp.EncodersConfig.Count; i++)
            {
                tmp.EncodersConfig[i].Type = Encoders[i].Type;
            }

            // fast encoder can not be x1 encoder
            if (tmp.EncodersConfig[0].Type == EncoderType.Encoder_1x)
            {
                tmp.EncodersConfig[0].Type = EncoderType.Encoder_2x;
                Encoders[0].Type = EncoderType.Encoder_2x;
                Update(tmp);
            }

            Config = tmp;
            ConfigChanged();
        }

        public void Update(DeviceConfig deviceConfig)
        {
            Config = deviceConfig;

            ObservableCollection<Encoder> tmp = new ObservableCollection<Encoder>();

            for (int i = 0; i < Encoders.Count; i++)
            {
                tmp.Add(new Encoder(i, Config.EncodersConfig[i].Type));
            }

            Encoders = tmp;
            foreach (var item in Encoders) item.PropertyChanged += EncodersVM_PropertyChanged;

            // fast encoder enabled
            if (Config.PinConfig[8] == PinType.Fast_Encoder && Config.PinConfig[9] == PinType.Fast_Encoder)
            {
                Encoders[0].InputA = -2;
                Encoders[0].InputB = -2;
                Encoders[0].IsEnabled = true;
            }

            // check pins
            int prevA = -1;
            int prevB = -1;
            for (int k = 1; k < Config.EncodersConfig.Count; k++)
            {

                for (int j = prevA + 1; j < Config.ButtonConfig.Count; j++)
                {
                    if (Config.ButtonConfig[j].Type == ButtonType.Encoder_A)
                    {
                        Encoders[k].InputA = j+1;
                        prevA = j;
                        break;
                    }
                }
                for (int j = prevB + 1; j < Config.PinConfig.Count; j++)
                {
                    if (Config.ButtonConfig[j].Type == ButtonType.Encoder_B)
                    {
                        Encoders[k].InputB = j+1;
                        prevB = j;
                        break;
                    }
                }
            }

            for (int k = 1; k < Config.EncodersConfig.Count; k++)
            {
                if (Encoders[k].InputB >= 0 &&
                    Encoders[k].InputA >= 0)
                {
                    Encoders[k].IsEnabled = true;
                }
            }
        }
    }
}
