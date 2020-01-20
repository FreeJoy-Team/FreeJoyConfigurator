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
        private int _rowCnt;
        private int _colCnt;
        private int _singleBtnCnt;
        private int _totalBtnCnt;

        private DeviceConfig _config;
        private ObservableCollection<Button> _buttons;

        public int RowCnt
        {
            get
            {
                return _rowCnt;
            }
            private set
            {
                SetProperty(ref _rowCnt, value);
                //TotalBtnCnt = _rowCnt * _colCnt + _singleBtnCnt;
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
                //TotalBtnCnt = _rowCnt * _colCnt + _singleBtnCnt;
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
                //TotalBtnCnt = _rowCnt * _colCnt + _singleBtnCnt;
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
            ColCnt = 0;
            RowCnt = 0;
            SingleBtnCnt = 0;
            TotalBtnCnt = 0;

            Config = config;

            ObservableCollection<Button> tmp = new ObservableCollection<Button>();

            // matrix buttons
            for (int i = 0; i < Config.PinConfig.Count; i++)
            {

                if (Config.PinConfig[i] == PinType.ButtonColumn)
                {
                    ColCnt++;
                    for (int k = 0; k < Config.PinConfig.Count; k++)
                    {
                        if (Config.PinConfig[k] == PinType.ButtonRow)
                        {
                            ObservableCollection<ButtonType> tmpTypes = new ObservableCollection<ButtonType>()
                            {       ButtonType.BtnInverted,
                                    ButtonType.BtnNormal,
                                    ButtonType.BtnToggle,
                                    ButtonType.ToggleSw,
                                    ButtonType.ToggleSwOff,
                                    ButtonType.ToggleSwOn,
                                    ButtonType.Pov1Up,
                                    ButtonType.Pov1Down,
                                    ButtonType.Pov1Left,
                                    ButtonType.Pov1Right,
                                    ButtonType.Pov2Up,
                                    ButtonType.Pov2Down,
                                    ButtonType.Pov2Left,
                                    ButtonType.Pov2Right,
                                    ButtonType.Pov3Up,
                                    ButtonType.Pov3Down,
                                    ButtonType.Pov3Left,
                                    ButtonType.Pov3Right,
                                    ButtonType.Pov4Up,
                                    ButtonType.Pov4Down,
                                    ButtonType.Pov4Left,
                                    ButtonType.Pov4Right,

                            };
                            config.ButtonConfig[TotalBtnCnt].Type = ButtonType.BtnNormal;
                            tmp.Add(new Button(false, config.ButtonConfig[TotalBtnCnt++].Type, tmpTypes, TotalBtnCnt));
                        }
                    }
                }
                else if (Config.PinConfig[i] == PinType.ButtonRow)
                {
                    RowCnt++;
                }
            }

            // Shift registers
            for (int i = 0, k = 0; i < Config.PinConfig.Count; i++)
            {
                if (Config.PinConfig[i] == PinType.ShiftReg_LATCH)
                {
                    for (int j=0; j<Config.ShiftRegistersConfig[k].ButtonCnt; j++)
                    {
                        tmp.Add(new Button(false, config.ButtonConfig[TotalBtnCnt++].Type, TotalBtnCnt));
                    }
                    k++;
                }
            } 

             // axes to buttons
            for (int i = 0; i < Config.AxisToButtonsConfig.Count; i++)
            {
                if (Config.AxisToButtonsConfig[i].IsEnabled)
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
                                ButtonType.Pov1Up,
                                ButtonType.Pov1Down,
                                ButtonType.Pov1Left,
                                ButtonType.Pov1Right,
                                ButtonType.Pov2Up,
                                ButtonType.Pov2Down,
                                ButtonType.Pov2Left,
                                ButtonType.Pov2Right,
                                ButtonType.Pov3Up,
                                ButtonType.Pov3Down,
                                ButtonType.Pov3Left,
                                ButtonType.Pov3Right,
                                ButtonType.Pov4Up,
                                ButtonType.Pov4Down,
                                ButtonType.Pov4Left,
                                ButtonType.Pov4Right,

                        };
                        config.ButtonConfig[TotalBtnCnt].Type = ButtonType.BtnNormal;
                        tmp.Add(new Button(false, config.ButtonConfig[TotalBtnCnt++].Type, tmpTypes, TotalBtnCnt));
                    }
                }
            }
            // single buttons
            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                
                if (Config.PinConfig[i] == PinType.ButtonGnd || Config.PinConfig[i] == PinType.ButtonGnd)
                {
                    tmp.Add(new Button(false, config.ButtonConfig[TotalBtnCnt++].Type, TotalBtnCnt));
                    SingleBtnCnt++;
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
                // OV1
                if (Buttons[i].Type == ButtonType.Pov1Down)
                {
                    Buttons[i].State = (Joystick.Povs[0].State == 0x03 || Joystick.Povs[0].State == 0x04 || Joystick.Povs[0].State == 0x05) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov1Left)
                {
                    Buttons[i].State = (Joystick.Povs[0].State == 0x05 || Joystick.Povs[0].State == 0x06 || Joystick.Povs[0].State == 0x07) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov1Right)
                {
                    Buttons[i].State = (Joystick.Povs[0].State == 0x01 || Joystick.Povs[0].State == 0x02 || Joystick.Povs[0].State == 0x03) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov1Up)
                {
                    Buttons[i].State = (Joystick.Povs[0].State == 0x00 || Joystick.Povs[0].State == 0x01 || Joystick.Povs[0].State == 0x07) ? true : false;
                }
                // POV2
                else if(Buttons[i].Type == ButtonType.Pov2Down)
                {
                    Buttons[i].State = (Joystick.Povs[1].State == 0x03 || Joystick.Povs[1].State == 0x04 || Joystick.Povs[1].State == 0x05) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov2Left)
                {
                    Buttons[i].State = (Joystick.Povs[1].State == 0x05 || Joystick.Povs[1].State == 0x06 || Joystick.Povs[1].State == 0x07) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov2Right)
                {
                    Buttons[i].State = (Joystick.Povs[1].State == 0x01 || Joystick.Povs[1].State == 0x02 || Joystick.Povs[1].State == 0x03) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov2Up)
                {
                    Buttons[i].State = (Joystick.Povs[1].State == 0x00 || Joystick.Povs[1].State == 0x01 || Joystick.Povs[1].State == 0x07) ? true : false;
                }
                // POV3
                else if (Buttons[i].Type == ButtonType.Pov3Down)
                {
                    Buttons[i].State = (Joystick.Povs[2].State == 0x03 || Joystick.Povs[2].State == 0x04 || Joystick.Povs[2].State == 0x05) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov3Left)
                {
                    Buttons[i].State = (Joystick.Povs[2].State == 0x05 || Joystick.Povs[2].State == 0x06 || Joystick.Povs[2].State == 0x07) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov3Right)
                {
                    Buttons[i].State = (Joystick.Povs[2].State == 0x01 || Joystick.Povs[2].State == 0x02 || Joystick.Povs[2].State == 0x03) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov3Up)
                {
                    Buttons[i].State = (Joystick.Povs[2].State == 0x00 || Joystick.Povs[2].State == 0x01 || Joystick.Povs[2].State == 0x07) ? true : false;
                }
                // POV4
                else if (Buttons[i].Type == ButtonType.Pov4Down)
                {
                    Buttons[i].State = (Joystick.Povs[3].State == 0x03 || Joystick.Povs[3].State == 0x04 || Joystick.Povs[3].State == 0x05) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov4Left)
                {
                    Buttons[i].State = (Joystick.Povs[3].State == 0x05 || Joystick.Povs[3].State == 0x06 || Joystick.Povs[3].State == 0x07) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov4Right)
                {
                    Buttons[i].State = (Joystick.Povs[3].State == 0x01 || Joystick.Povs[3].State == 0x02 || Joystick.Povs[3].State == 0x03) ? true : false;
                }
                else if (Buttons[i].Type == ButtonType.Pov4Up)
                {
                    Buttons[i].State = (Joystick.Povs[3].State == 0x00 || Joystick.Povs[3].State == 0x01 || Joystick.Povs[3].State == 0x07) ? true : false;
                }
                else 
                {
                    Buttons[i].State = Joystick.Buttons[i].State;
                }
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
