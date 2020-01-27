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
        private ObservableCollection<Button> _logicalButtons;
        private ObservableCollection<Button> _physicalButtons;

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

        public ObservableCollection<Button> LogicalButtons
        {
            get { return _logicalButtons; }
            set { SetProperty(ref _logicalButtons, value); }
        }

        public ObservableCollection<Button> PhysicalButtons
        {
            get { return _physicalButtons; }
            set { SetProperty(ref _physicalButtons, value); }
        }

        public Joystick Joystick;

        public ButtonsVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            Joystick = joystick;
            Joystick.PropertyChanged += Joystick_PropertyChanged;
            _config = deviceConfig;

            _logicalButtons = new ObservableCollection<Button>();
            for (int i = 0; i < 128; i++)
            {
                _logicalButtons.Add(new Button(false, _config.ButtonConfig[i].Type,  i + 1, 0));
            }
            foreach (var button in _logicalButtons) button.PropertyChanged += Button_PropertyChanged;

            _physicalButtons = new ObservableCollection<Button>();
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
            
            PhysicalButtons = new ObservableCollection<Button>(tmp);
            RaisePropertyChanged(nameof(PhysicalButtons));

            tmp = new ObservableCollection<Button>();
            for (int i=0; i<128; i++)
            {
                tmp.Add(new Button(false, config.ButtonConfig[i].Type, i+1));
            }
            foreach (var button in LogicalButtons) button.PropertyChanged += Button_PropertyChanged;

            RaisePropertyChanged(nameof(LogicalButtons));
        }

        private void Joystick_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            for (int i=0;i<PhysicalButtons.Count;i++)
            {
                PhysicalButtons[i].State = Joystick.PhysicalButtons[i].State;
            }

            for (int i = 0; i < LogicalButtons.Count; i++)
            {
                // OV1
                if (LogicalButtons[i].Type == ButtonType.Pov1Down)
                {
                    LogicalButtons[i].State = (Joystick.Povs[0].State == 0x03 || Joystick.Povs[0].State == 0x04 || Joystick.Povs[0].State == 0x05) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov1Left)
                {
                    LogicalButtons[i].State = (Joystick.Povs[0].State == 0x05 || Joystick.Povs[0].State == 0x06 || Joystick.Povs[0].State == 0x07) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov1Right)
                {
                    LogicalButtons[i].State = (Joystick.Povs[0].State == 0x01 || Joystick.Povs[0].State == 0x02 || Joystick.Povs[0].State == 0x03) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov1Up)
                {
                    LogicalButtons[i].State = (Joystick.Povs[0].State == 0x00 || Joystick.Povs[0].State == 0x01 || Joystick.Povs[0].State == 0x07) ? true : false;
                }
                // POV2
                else if(LogicalButtons[i].Type == ButtonType.Pov2Down)
                {
                    LogicalButtons[i].State = (Joystick.Povs[1].State == 0x03 || Joystick.Povs[1].State == 0x04 || Joystick.Povs[1].State == 0x05) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov2Left)
                {
                    LogicalButtons[i].State = (Joystick.Povs[1].State == 0x05 || Joystick.Povs[1].State == 0x06 || Joystick.Povs[1].State == 0x07) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov2Right)
                {
                    LogicalButtons[i].State = (Joystick.Povs[1].State == 0x01 || Joystick.Povs[1].State == 0x02 || Joystick.Povs[1].State == 0x03) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov2Up)
                {
                    LogicalButtons[i].State = (Joystick.Povs[1].State == 0x00 || Joystick.Povs[1].State == 0x01 || Joystick.Povs[1].State == 0x07) ? true : false;
                }
                // POV3
                else if (LogicalButtons[i].Type == ButtonType.Pov3Down)
                {
                    LogicalButtons[i].State = (Joystick.Povs[2].State == 0x03 || Joystick.Povs[2].State == 0x04 || Joystick.Povs[2].State == 0x05) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov3Left)
                {
                    LogicalButtons[i].State = (Joystick.Povs[2].State == 0x05 || Joystick.Povs[2].State == 0x06 || Joystick.Povs[2].State == 0x07) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov3Right)
                {
                    LogicalButtons[i].State = (Joystick.Povs[2].State == 0x01 || Joystick.Povs[2].State == 0x02 || Joystick.Povs[2].State == 0x03) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov3Up)
                {
                    LogicalButtons[i].State = (Joystick.Povs[2].State == 0x00 || Joystick.Povs[2].State == 0x01 || Joystick.Povs[2].State == 0x07) ? true : false;
                }
                // POV4
                else if (LogicalButtons[i].Type == ButtonType.Pov4Down)
                {
                    LogicalButtons[i].State = (Joystick.Povs[3].State == 0x03 || Joystick.Povs[3].State == 0x04 || Joystick.Povs[3].State == 0x05) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov4Left)
                {
                    LogicalButtons[i].State = (Joystick.Povs[3].State == 0x05 || Joystick.Povs[3].State == 0x06 || Joystick.Povs[3].State == 0x07) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov4Right)
                {
                    LogicalButtons[i].State = (Joystick.Povs[3].State == 0x01 || Joystick.Povs[3].State == 0x02 || Joystick.Povs[3].State == 0x03) ? true : false;
                }
                else if (LogicalButtons[i].Type == ButtonType.Pov4Up)
                {
                    LogicalButtons[i].State = (Joystick.Povs[3].State == 0x00 || Joystick.Povs[3].State == 0x01 || Joystick.Povs[3].State == 0x07) ? true : false;
                }
                else 
                {
                    LogicalButtons[i].State = Joystick.LogicalButtons[i].State;
                }
            }
        }

        private void Button_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DeviceConfig tmp = Config;

            for (int i=0; i<LogicalButtons.Count;i++)
            {
                tmp.ButtonConfig[i].Type = LogicalButtons[i].Type;
            }
            Config = tmp;

            RaisePropertyChanged(nameof(LogicalButtons));
        }
    }

}
