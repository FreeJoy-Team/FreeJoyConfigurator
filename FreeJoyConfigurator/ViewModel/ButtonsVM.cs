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
        private int _buttonsFromAxes;

        private DeviceConfig _config;
        private ObservableCollection<Button> _logicalButtons;
        private ObservableCollection<Button> _physicalButtons;

        public delegate void ButtonsChangedEvent();
        public event ButtonsChangedEvent ConfigChanged;

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

        public int ButtonsFromAxesCnt
        {
            get
            {
                return _buttonsFromAxes;
            }
            private set
            {
                SetProperty(ref _buttonsFromAxes, value);
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
                _logicalButtons.Add(new Button(i + 1, 0));
            }
            foreach (var button in _logicalButtons) button.Config.PropertyChanged += Button_PropertyChanged;

            _physicalButtons = new ObservableCollection<Button>();
        }

        public void Update(DeviceConfig config)
        {
            ColCnt = 0;
            RowCnt = 0;
            SingleBtnCnt = 0;
            TotalBtnCnt = 0;
            ButtonsFromAxesCnt = 0;

            Config = config;

            ObservableCollection<Button> tmp = new ObservableCollection<Button>();

            // matrix buttons
            for (int i = 0; i < config.PinConfig.Count; i++)
            {

                if (config.PinConfig[i] == PinType.Button_Column)
                {
                    ColCnt++;
                    for (int k = 0; k < config.PinConfig.Count; k++)
                    {
                        if (config.PinConfig[k] == PinType.Button_Row)
                        {
                            tmp.Add(new Button(TotalBtnCnt + 1));
                            tmp[TotalBtnCnt].SourceType = ButtonSourceType.MatrixButton;
                            TotalBtnCnt++;
                        }
                    }
                }
                else if (config.PinConfig[i] == PinType.Button_Row)
                {
                    RowCnt++;
                }
            }

            // Shift registers
            for (int i = 0, k = 0; i < config.PinConfig.Count; i++)
            {
                if (config.PinConfig[i] == PinType.ShiftReg_LATCH)
                {
                    for (int j = 0; j < config.ShiftRegistersConfig[k].ButtonCnt; j++)
                    {
                        tmp.Add(new Button(TotalBtnCnt + 1));
                        tmp[TotalBtnCnt].SourceType = ButtonSourceType.ShiftRegister;
                        TotalBtnCnt++;
                    }
                    k++;
                }
            }

            // axes to buttons
            for (int i = 0; i < config.AxisToButtonsConfig.Count; i++)
            {
                if (config.AxisToButtonsConfig[i].IsEnabled)
                {

                    for (int j = 0; j < config.AxisToButtonsConfig[i].ButtonsCnt; j++)
                    {
                        tmp.Add(new Button(TotalBtnCnt + 1));
                        tmp[TotalBtnCnt].SourceType = ButtonSourceType.AxisToButtons;
                        TotalBtnCnt++;
                        ButtonsFromAxesCnt++;
                    }
                }
            }
            // single buttons
            for (int i = 0; i < config.PinConfig.Count; i++)
            {

                if (config.PinConfig[i] == PinType.Button_Gnd || config.PinConfig[i] == PinType.Button_Vcc)
                {
                    tmp.Add(new Button(TotalBtnCnt + 1));
                    tmp[TotalBtnCnt].SourceType = ButtonSourceType.SingleButton;
                    TotalBtnCnt++;
                    SingleBtnCnt++;
                }
            }

            PhysicalButtons = tmp;
            RaisePropertyChanged(nameof(PhysicalButtons));

            for (int i = 0; i < LogicalButtons.Count; i++)
            {
                LogicalButtons[i].Config.PropertyChanged -= Button_PropertyChanged;
                LogicalButtons[i].Config.MaxPhysicalNumber = TotalBtnCnt;
                LogicalButtons[i].Config.PhysicalNumber = config.ButtonConfig[i].PhysicalNumber;
                LogicalButtons[i].Config.ShiftModificator = config.ButtonConfig[i].ShiftModificator;
                LogicalButtons[i].Config.Type = config.ButtonConfig[i].Type;
                if (PhysicalButtons.Count > 0)
                {
                    if (config.ButtonConfig[i].PhysicalNumber > 0)
                    {
                        LogicalButtons[i].SourceType = PhysicalButtons[config.ButtonConfig[i].PhysicalNumber - 1].SourceType;
                    }
                    else
                    {
                        LogicalButtons[i].SourceType = ButtonSourceType.NoSource;
                    }

                }
            }
            // shifts
            for (int i = 0; i < config.ShiftModificatorConfig.Count; i++)
            {
                if (config.ShiftModificatorConfig[i].Button > 0)
                {
                    LogicalButtons[config.ShiftModificatorConfig[i].Button].SourceType = ButtonSourceType.Shift;
                }
            }

            foreach (var button in LogicalButtons)
            {
                button.Config.PropertyChanged += Button_PropertyChanged;
            }

            Button_PropertyChanged(null, null);
        }

        private void Joystick_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            for (int i = 0; i < PhysicalButtons.Count; i++)
            {
                PhysicalButtons[i].State = Joystick.PhysicalButtons[i].State;
            }

            for (int i = 0; i < LogicalButtons.Count; i++)
            {
                // OV1
                if (LogicalButtons[i].Config.Type == ButtonType.Pov1_Down)
                {
                    LogicalButtons[i].State = (Joystick.Povs[0].State == 0x03 || Joystick.Povs[0].State == 0x04 || Joystick.Povs[0].State == 0x05) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov1_Left)
                {
                    LogicalButtons[i].State = (Joystick.Povs[0].State == 0x05 || Joystick.Povs[0].State == 0x06 || Joystick.Povs[0].State == 0x07) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov1_Right)
                {
                    LogicalButtons[i].State = (Joystick.Povs[0].State == 0x01 || Joystick.Povs[0].State == 0x02 || Joystick.Povs[0].State == 0x03) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov1_Up)
                {
                    LogicalButtons[i].State = (Joystick.Povs[0].State == 0x00 || Joystick.Povs[0].State == 0x01 || Joystick.Povs[0].State == 0x07) ? true : false;
                }
                // POV2
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov2_Down)
                {
                    LogicalButtons[i].State = (Joystick.Povs[1].State == 0x03 || Joystick.Povs[1].State == 0x04 || Joystick.Povs[1].State == 0x05) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov2_Left)
                {
                    LogicalButtons[i].State = (Joystick.Povs[1].State == 0x05 || Joystick.Povs[1].State == 0x06 || Joystick.Povs[1].State == 0x07) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov2_Right)
                {
                    LogicalButtons[i].State = (Joystick.Povs[1].State == 0x01 || Joystick.Povs[1].State == 0x02 || Joystick.Povs[1].State == 0x03) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov2_Up)
                {
                    LogicalButtons[i].State = (Joystick.Povs[1].State == 0x00 || Joystick.Povs[1].State == 0x01 || Joystick.Povs[1].State == 0x07) ? true : false;
                }
                // POV3
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov3_Down)
                {
                    LogicalButtons[i].State = (Joystick.Povs[2].State == 0x03 || Joystick.Povs[2].State == 0x04 || Joystick.Povs[2].State == 0x05) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov3_Left)
                {
                    LogicalButtons[i].State = (Joystick.Povs[2].State == 0x05 || Joystick.Povs[2].State == 0x06 || Joystick.Povs[2].State == 0x07) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov3_Right)
                {
                    LogicalButtons[i].State = (Joystick.Povs[2].State == 0x01 || Joystick.Povs[2].State == 0x02 || Joystick.Povs[2].State == 0x03) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov3_Up)
                {
                    LogicalButtons[i].State = (Joystick.Povs[2].State == 0x00 || Joystick.Povs[2].State == 0x01 || Joystick.Povs[2].State == 0x07) ? true : false;
                }
                // POV4
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov4_Down)
                {
                    LogicalButtons[i].State = (Joystick.Povs[3].State == 0x03 || Joystick.Povs[3].State == 0x04 || Joystick.Povs[3].State == 0x05) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov4_Left)
                {
                    LogicalButtons[i].State = (Joystick.Povs[3].State == 0x05 || Joystick.Povs[3].State == 0x06 || Joystick.Povs[3].State == 0x07) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov4_Right)
                {
                    LogicalButtons[i].State = (Joystick.Povs[3].State == 0x01 || Joystick.Povs[3].State == 0x02 || Joystick.Povs[3].State == 0x03) ? true : false;
                }
                else if (LogicalButtons[i].Config.Type == ButtonType.Pov4_Up)
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

            for (int i = 0; i < LogicalButtons.Count; i++)
            {
                if (LogicalButtons[i].Config.PhysicalNumber > 0 && TotalBtnCnt > 0)
                {
                    LogicalButtons[i].SourceType = PhysicalButtons[LogicalButtons[i].Config.PhysicalNumber - 1].SourceType;

                    for (int j = 0; j < Config.ShiftModificatorConfig.Count; j++)
                    {
                        if (Config.ShiftModificatorConfig[j].Button == i+1)
                            LogicalButtons[j].SourceType = ButtonSourceType.Shift;
                    }
                }
                else
                {
                    LogicalButtons[i].SourceType = ButtonSourceType.NoSource;
                }

                switch (LogicalButtons[i].SourceType)
                {
                    case ButtonSourceType.SingleButton:
                    case ButtonSourceType.ShiftRegister:
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Normal)) 
                            LogicalButtons[i].AllowedTypes.Insert(0, ButtonType.Button_Normal);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Inverted))
                            LogicalButtons[i].AllowedTypes.Insert(1, ButtonType.Button_Inverted);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(2, ButtonType.Button_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_OnOff))
                            LogicalButtons[i].AllowedTypes.Insert(3, ButtonType.ToggleSwitch_OnOff);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_On))
                            LogicalButtons[i].AllowedTypes.Insert(4, ButtonType.ToggleSwitch_On);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_Off))
                            LogicalButtons[i].AllowedTypes.Insert(5, ButtonType.ToggleSwitch_Off);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Down))
                            LogicalButtons[i].AllowedTypes.Insert(6, ButtonType.Pov1_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Up))
                            LogicalButtons[i].AllowedTypes.Insert(7, ButtonType.Pov1_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Left))
                            LogicalButtons[i].AllowedTypes.Insert(8, ButtonType.Pov1_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Right))
                            LogicalButtons[i].AllowedTypes.Insert(9, ButtonType.Pov1_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Down))
                            LogicalButtons[i].AllowedTypes.Insert(10, ButtonType.Pov2_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Up))
                            LogicalButtons[i].AllowedTypes.Insert(11, ButtonType.Pov2_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Left))
                            LogicalButtons[i].AllowedTypes.Insert(12, ButtonType.Pov2_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Right))
                            LogicalButtons[i].AllowedTypes.Insert(13, ButtonType.Pov2_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Down))
                            LogicalButtons[i].AllowedTypes.Insert(14, ButtonType.Pov3_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Up))
                            LogicalButtons[i].AllowedTypes.Insert(15, ButtonType.Pov3_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Left))
                            LogicalButtons[i].AllowedTypes.Insert(16, ButtonType.Pov3_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Right))
                            LogicalButtons[i].AllowedTypes.Insert(17, ButtonType.Pov3_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Down))
                            LogicalButtons[i].AllowedTypes.Insert(18, ButtonType.Pov4_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Up))
                            LogicalButtons[i].AllowedTypes.Insert(19, ButtonType.Pov4_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Left))
                            LogicalButtons[i].AllowedTypes.Insert(20, ButtonType.Pov4_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Right))
                            LogicalButtons[i].AllowedTypes.Insert(21, ButtonType.Pov4_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Encoder_A))
                            LogicalButtons[i].AllowedTypes.Insert(22, ButtonType.Encoder_A);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Encoder_B))
                            LogicalButtons[i].AllowedTypes.Insert(23, ButtonType.Encoder_B);
                        break;
                    case ButtonSourceType.MatrixButton:
                    case ButtonSourceType.AxisToButtons:
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Normal))
                            LogicalButtons[i].AllowedTypes.Insert(0, ButtonType.Button_Normal);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Inverted))
                            LogicalButtons[i].AllowedTypes.Insert(1, ButtonType.Button_Inverted);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(2, ButtonType.Button_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_OnOff))
                            LogicalButtons[i].AllowedTypes.Insert(3, ButtonType.ToggleSwitch_OnOff);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_On))
                            LogicalButtons[i].AllowedTypes.Insert(4, ButtonType.ToggleSwitch_On);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_Off))
                            LogicalButtons[i].AllowedTypes.Insert(5, ButtonType.ToggleSwitch_Off);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Down))
                            LogicalButtons[i].AllowedTypes.Insert(6, ButtonType.Pov1_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Up))
                            LogicalButtons[i].AllowedTypes.Insert(7, ButtonType.Pov1_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Left))
                            LogicalButtons[i].AllowedTypes.Insert(8, ButtonType.Pov1_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Right))
                            LogicalButtons[i].AllowedTypes.Insert(9, ButtonType.Pov1_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Down))
                            LogicalButtons[i].AllowedTypes.Insert(10, ButtonType.Pov2_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Up))
                            LogicalButtons[i].AllowedTypes.Insert(11, ButtonType.Pov2_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Left))
                            LogicalButtons[i].AllowedTypes.Insert(12, ButtonType.Pov2_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Right))
                            LogicalButtons[i].AllowedTypes.Insert(13, ButtonType.Pov2_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Down))
                            LogicalButtons[i].AllowedTypes.Insert(14, ButtonType.Pov3_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Up))
                            LogicalButtons[i].AllowedTypes.Insert(15, ButtonType.Pov3_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Left))
                            LogicalButtons[i].AllowedTypes.Insert(16, ButtonType.Pov3_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Right))
                            LogicalButtons[i].AllowedTypes.Insert(17, ButtonType.Pov3_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Down))
                            LogicalButtons[i].AllowedTypes.Insert(18, ButtonType.Pov4_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Up))
                            LogicalButtons[i].AllowedTypes.Insert(19, ButtonType.Pov4_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Left))
                            LogicalButtons[i].AllowedTypes.Insert(20, ButtonType.Pov4_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Right))
                            LogicalButtons[i].AllowedTypes.Insert(21, ButtonType.Pov4_Right);

                        if (LogicalButtons[i].Config.Type == ButtonType.Encoder_A ||
                            LogicalButtons[i].Config.Type == ButtonType.Encoder_B)
                        {
                            LogicalButtons[i].Config.Type = ButtonType.Button_Normal;
                        }

                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Encoder_A);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Encoder_B);
                        break;
                    case ButtonSourceType.Shift:
                    default:
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Normal))
                            LogicalButtons[i].AllowedTypes.Insert(0, ButtonType.Button_Normal);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Inverted))
                            LogicalButtons[i].AllowedTypes.Insert(1, ButtonType.Button_Inverted);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(2, ButtonType.Button_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_OnOff))
                            LogicalButtons[i].AllowedTypes.Insert(3, ButtonType.ToggleSwitch_OnOff);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_On))
                            LogicalButtons[i].AllowedTypes.Insert(4, ButtonType.ToggleSwitch_On);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_Off))
                            LogicalButtons[i].AllowedTypes.Insert(5, ButtonType.ToggleSwitch_Off);

                        if (LogicalButtons[i].Config.Type == ButtonType.Encoder_A ||
                            LogicalButtons[i].Config.Type == ButtonType.Encoder_B ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Down ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Up ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Left ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Right ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Down ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Up ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Left ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Right ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov3_Down ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov3_Up ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov3_Left ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov3_Right ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov4_Down ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov4_Up ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov4_Left ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov4_Right )
                        {
                            LogicalButtons[i].Config.Type = ButtonType.Button_Normal;
                        }

                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Encoder_A);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Encoder_B);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov1_Down);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov1_Up);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov1_Left);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov1_Right);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Down);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Up);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Left);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Right);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov3_Down);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov3_Up);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov3_Left);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov3_Right);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov4_Down);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov4_Up);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov4_Left);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov4_Right);


                        break;
                }
            }


            DeviceConfig tmp = Config;
            for (int i = 0; i < LogicalButtons.Count; i++)
            {
                tmp.ButtonConfig[i].PhysicalNumber = (sbyte)LogicalButtons[i].Config.PhysicalNumber;
                tmp.ButtonConfig[i].ShiftModificator = LogicalButtons[i].Config.ShiftModificator;
                tmp.ButtonConfig[i].Type = LogicalButtons[i].Config.Type;
            }
            Config = tmp;

            RaisePropertyChanged(nameof(LogicalButtons));
            ConfigChanged();
        }
    }

}