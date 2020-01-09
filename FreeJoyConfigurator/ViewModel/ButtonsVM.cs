using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FreeJoyConfigurator
{
    public class ButtonsVM : BindableBase
    {       
        public DeviceConfig _config;
        private ObservableCollection<Button> _buttons;

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

        public ObservableCollection<Button> Buttons
        {
            get { return _buttons; }
            set { SetProperty(ref _buttons, value); }
        }

        public Joystick Joystick;

        public ButtonsVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            Joystick = joystick;
            Joystick.PropertyChanged += Joystick_PropertyChanged;
            _config = deviceConfig;

            _buttons = new ObservableCollection<Button>();

        }

        public void Update(DeviceConfig config)
        {
            int buttonCnt = 0;
            Config = config;

            ObservableCollection<Button> tmp = new ObservableCollection<Button>();

            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                if (Config.PinConfig[i] == PinType.ButtonGnd || Config.PinConfig[i] == PinType.ButtonGnd)
                {
                    tmp.Add(new Button(false, config.ButtonConfig[buttonCnt++].Type, buttonCnt));
                }
                else if (Config.PinConfig[i] == PinType.ButtonRow)
                {
                    for (int k = 0; k < Config.PinConfig.Count; k++)
                    {
                        if (Config.PinConfig[k] == PinType.ButtonColumn)
                        {
                            tmp.Add(new Button(false, config.ButtonConfig[buttonCnt++].Type, buttonCnt));
                        }
                    }
                }
                
            }
            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                if (Config.PinConfig[i] == PinType.AxisToButtons)
                {
                    for (int j = 0; j < config.AxisToButtonsConfig[i].ButtonsCnt; j++)
                    {
                        ObservableCollection<ButtonType> tmpTypes = new ObservableCollection<ButtonType>()
                        {       ButtonType.BtnInverted,
                                ButtonType.BtnNormal,
                                ButtonType.BtnToggle,
                                ButtonType.ToggleSw,
                                ButtonType.ToggleSwOff,
                                ButtonType.ToggleSwOn,

                        };
                        config.ButtonConfig[buttonCnt].Type = ButtonType.ToggleSwOn;
                        tmp.Add(new Button(false, config.ButtonConfig[buttonCnt++].Type, tmpTypes, buttonCnt));

                    }
                }
            }

                Buttons = new ObservableCollection<Button>(tmp);

            foreach (var button in Buttons) button.PropertyChanged += Button_PropertyChanged;
            RaisePropertyChanged(nameof(Buttons));
        }

        private void Joystick_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].State = Joystick.Buttons[i].State;
            }
        }

        private void Button_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DeviceConfig tmp = Config;

            for (int i=0; i<Buttons.Count;i++)
            {
                tmp.ButtonConfig[i].Type = Buttons[i].Type;
            }
            Config = tmp;

            RaisePropertyChanged(nameof(Buttons));
        }
    }

}
