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
        const int maxBtnCnt = 128;

        private int _rowCnt;
        private int _colCnt;
        private int _singleBtnCnt;
        private int _totalBtnCnt;
        private int _buttonsFromAxes;
        private bool _buttonsError;

        private DeviceConfig _config;
        private ObservableCollection<Button> _prevLogicalButtons;
        private ObservableCollection<Button> _logicalButtons;
        private ObservableCollection<Button> _physicalButtons;
        private ObservableCollection<Button> _shiftButtons;

        public delegate void ButtonsChangedEvent();
        public event ButtonsChangedEvent ConfigChanged;

        public string ButtonsLog { get; private set; }

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
        public bool ButtonsError
        {
            get
            {
                return _buttonsError;
            }
            private set
            {
                SetProperty(ref _buttonsError, value);
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

        public ObservableCollection<Button> ShiftButtons
        {
            get { return _shiftButtons; }
            set { SetProperty(ref _shiftButtons, value); }
        }

        public Joystick Joystick;

        public ButtonsVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            Joystick = joystick;
            Joystick.PropertyChanged += Joystick_PropertyChanged;
            _config = deviceConfig;

            _prevLogicalButtons = new ObservableCollection<Button>();
            for (int i = 0; i < 128; i++)
            {
                _prevLogicalButtons.Add(new Button(i + 1, 0));
            }

            _logicalButtons = new ObservableCollection<Button>();
            for (int i = 0; i < 128; i++)
            {
                _logicalButtons.Add(new Button(i + 1, 0));
            }
            foreach (var button in _logicalButtons) button.Config.PropertyChanged += Button_PropertyChanged;

            _physicalButtons = new ObservableCollection<Button>();
            _shiftButtons = new ObservableCollection<Button>(Joystick.ShiftButtons);
        }

        public void Update(DeviceConfig config)
        {
            ColCnt = 0;
            RowCnt = 0;
            SingleBtnCnt = 0;
            TotalBtnCnt = 0;
            ButtonsFromAxesCnt = 0;
            ButtonsError = false;

            Config = config;

            ObservableCollection<Button> tmp = new ObservableCollection<Button>();

            // matrix buttons
            for (int i = 0; i < config.PinConfig.Count; i++)
            {

                if (config.PinConfig[i] == PinType.Button_Row)
                {
                    RowCnt++;
                    for (int k = 0; k < config.PinConfig.Count; k++)
                    {
                        if (config.PinConfig[k] == PinType.Button_Column)
                        {
                            if (TotalBtnCnt < maxBtnCnt)
                            {
                                tmp.Add(new Button(TotalBtnCnt + 1));
                                tmp[TotalBtnCnt].SourceType = ButtonSourceType.MatrixButton;
                            }
                            TotalBtnCnt++;
                        }
                        
                    }
                }
                else if (config.PinConfig[i] == PinType.Button_Column)
                {
                    ColCnt++;
                }
            }

            // Shift registers
            for (int i = 0, k = 0; i < config.PinConfig.Count; i++)
            {
                if (config.PinConfig[i] == PinType.ShiftReg_DATA)
                {
                    for (int j = 0; j < config.ShiftRegistersConfig[k].ButtonCnt; j++)
                    {
                        if (TotalBtnCnt < maxBtnCnt)
                        {
                            tmp.Add(new Button(TotalBtnCnt + 1));
                            tmp[TotalBtnCnt].SourceType = ButtonSourceType.ShiftRegister;
                            
                        }
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
                        if (TotalBtnCnt < maxBtnCnt)
                        {
                            tmp.Add(new Button(TotalBtnCnt + 1));
                            tmp[TotalBtnCnt].SourceType = ButtonSourceType.AxisToButtons;
                            
                            ButtonsFromAxesCnt++;
                        }
                        TotalBtnCnt++;
                    }
                }
            }
            // single buttons
            for (int i = 0; i < config.PinConfig.Count; i++)
            {

                if (config.PinConfig[i] == PinType.Button_Gnd || config.PinConfig[i] == PinType.Button_Vcc)
                {
                    if (TotalBtnCnt < maxBtnCnt)
                    {
                        tmp.Add(new Button(TotalBtnCnt + 1));
                        tmp[TotalBtnCnt].SourceType = ButtonSourceType.SingleButton;
                        
                        SingleBtnCnt++;
                    }
                    TotalBtnCnt++;
                }
            }

            PhysicalButtons = tmp;
            RaisePropertyChanged(nameof(PhysicalButtons));

            for (int i = 0; i < LogicalButtons.Count; i++)
            {
                LogicalButtons[i].Config.PropertyChanged -= Button_PropertyChanged;
                LogicalButtons[i].MaxPhysicalNumber = TotalBtnCnt;
                LogicalButtons[i].Config.PhysicalNumber = config.ButtonConfig[i].PhysicalNumber;
                LogicalButtons[i].Config.ShiftModificator = config.ButtonConfig[i].ShiftModificator;
                LogicalButtons[i].Config.Type = config.ButtonConfig[i].Type;
                LogicalButtons[i].Config.IsInverted = config.ButtonConfig[i].IsInverted;
                LogicalButtons[i].Config.IsDisabled = config.ButtonConfig[i].IsDisabled;   
                LogicalButtons[i].Config.ButtonDelayNumber = config.ButtonConfig[i].ButtonDelayNumber;          //!!!!!
                LogicalButtons[i].Config.ButtonToggleNumber = config.ButtonConfig[i].ButtonToggleNumber;

                if (PhysicalButtons.Count > 0)
                {
                    if (config.ButtonConfig[i].PhysicalNumber > 0 && config.ButtonConfig[i].PhysicalNumber <= TotalBtnCnt)
                    {
                        LogicalButtons[i].SourceType = PhysicalButtons[config.ButtonConfig[i].PhysicalNumber - 1].SourceType;
                    }
                    else
                    {
                        if (LogicalButtons[i].Config.PhysicalNumber >= TotalBtnCnt) LogicalButtons[i].Config.PhysicalNumber = 0;
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

            if (TotalBtnCnt > maxBtnCnt) ButtonsError = true;

            Button_PropertyChanged(null, null);
        }

        private void Joystick_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            for (int i=0; i< ShiftButtons.Count; i++)
            {
                ShiftButtons[i].State = Joystick.ShiftButtons[i].State;
            }

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

                if (LogicalButtons[i].State != _prevLogicalButtons[i].State)
                {
                    if (LogicalButtons[i].State) WriteLog("Logical button №" + (i + 1).ToString() + " pressed", false);
                    else WriteLog("Logical button №" + (i + 1).ToString() + " unpressed", false);
                    
                }
                _prevLogicalButtons[i].State = LogicalButtons[i].State;
            }
        }

        private void Button_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            for (int i = 0; i < LogicalButtons.Count; i++)
            {
                if (LogicalButtons[i].Config.PhysicalNumber > 0 && LogicalButtons[i].Config.PhysicalNumber <= TotalBtnCnt)
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
                    if (LogicalButtons[i].Config.PhysicalNumber >= TotalBtnCnt) LogicalButtons[i].Config.PhysicalNumber = 0;
                    LogicalButtons[i].SourceType = ButtonSourceType.NoSource;
                }

                switch (LogicalButtons[i].SourceType)
                {
                    case ButtonSourceType.SingleButton:
                    case ButtonSourceType.ShiftRegister:
                    case ButtonSourceType.MatrixButton:
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Normal)) 
                            LogicalButtons[i].AllowedTypes.Insert(0, ButtonType.Button_Normal);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(1, ButtonType.Button_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_OnOff))
                            LogicalButtons[i].AllowedTypes.Insert(2, ButtonType.ToggleSwitch_OnOff);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_On))
                            LogicalButtons[i].AllowedTypes.Insert(3, ButtonType.ToggleSwitch_On);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_Off))
                            LogicalButtons[i].AllowedTypes.Insert(4, ButtonType.ToggleSwitch_Off);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Down))
                            LogicalButtons[i].AllowedTypes.Insert(5, ButtonType.Pov1_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Up))
                            LogicalButtons[i].AllowedTypes.Insert(6, ButtonType.Pov1_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Left))
                            LogicalButtons[i].AllowedTypes.Insert(7, ButtonType.Pov1_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Right))
                            LogicalButtons[i].AllowedTypes.Insert(8, ButtonType.Pov1_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Center))
                            LogicalButtons[i].AllowedTypes.Insert(9, ButtonType.Pov1_Center);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Down))
                            LogicalButtons[i].AllowedTypes.Insert(10, ButtonType.Pov2_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Up))
                            LogicalButtons[i].AllowedTypes.Insert(11, ButtonType.Pov2_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Left))
                            LogicalButtons[i].AllowedTypes.Insert(12, ButtonType.Pov2_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Right))
                            LogicalButtons[i].AllowedTypes.Insert(13, ButtonType.Pov2_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Center))
                            LogicalButtons[i].AllowedTypes.Insert(14, ButtonType.Pov2_Center);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Down))
                            LogicalButtons[i].AllowedTypes.Insert(15, ButtonType.Pov3_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Up))
                            LogicalButtons[i].AllowedTypes.Insert(16, ButtonType.Pov3_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Left))
                            LogicalButtons[i].AllowedTypes.Insert(17, ButtonType.Pov3_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Right))
                            LogicalButtons[i].AllowedTypes.Insert(18, ButtonType.Pov3_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Down))
                            LogicalButtons[i].AllowedTypes.Insert(19, ButtonType.Pov4_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Up))
                            LogicalButtons[i].AllowedTypes.Insert(20, ButtonType.Pov4_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Left))
                            LogicalButtons[i].AllowedTypes.Insert(21, ButtonType.Pov4_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Right))
                            LogicalButtons[i].AllowedTypes.Insert(22, ButtonType.Pov4_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton1))
                            LogicalButtons[i].AllowedTypes.Insert(23, ButtonType.RadioButton1);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton2))
                            LogicalButtons[i].AllowedTypes.Insert(24, ButtonType.RadioButton2);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton3))
                            LogicalButtons[i].AllowedTypes.Insert(25, ButtonType.RadioButton3);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton4))
                            LogicalButtons[i].AllowedTypes.Insert(26, ButtonType.RadioButton4);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Sequential_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(27, ButtonType.Sequential_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Sequential_Button))
                            LogicalButtons[i].AllowedTypes.Insert(28, ButtonType.Sequential_Button);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Encoder_A))
                            LogicalButtons[i].AllowedTypes.Insert(29, ButtonType.Encoder_A);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Encoder_B))
                            LogicalButtons[i].AllowedTypes.Insert(30, ButtonType.Encoder_B);
                        break;
                    
                    case ButtonSourceType.AxisToButtons:
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Normal))
                            LogicalButtons[i].AllowedTypes.Insert(0, ButtonType.Button_Normal);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(1, ButtonType.Button_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_OnOff))
                            LogicalButtons[i].AllowedTypes.Insert(2, ButtonType.ToggleSwitch_OnOff);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_On))
                            LogicalButtons[i].AllowedTypes.Insert(3, ButtonType.ToggleSwitch_On);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_Off))
                            LogicalButtons[i].AllowedTypes.Insert(4, ButtonType.ToggleSwitch_Off);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Down))
                            LogicalButtons[i].AllowedTypes.Insert(5, ButtonType.Pov1_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Up))
                            LogicalButtons[i].AllowedTypes.Insert(6, ButtonType.Pov1_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Left))
                            LogicalButtons[i].AllowedTypes.Insert(7, ButtonType.Pov1_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Right))
                            LogicalButtons[i].AllowedTypes.Insert(8, ButtonType.Pov1_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov1_Center))
                            LogicalButtons[i].AllowedTypes.Insert(9, ButtonType.Pov1_Center);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Down))
                            LogicalButtons[i].AllowedTypes.Insert(10, ButtonType.Pov2_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Up))
                            LogicalButtons[i].AllowedTypes.Insert(11, ButtonType.Pov2_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Left))
                            LogicalButtons[i].AllowedTypes.Insert(12, ButtonType.Pov2_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Right))
                            LogicalButtons[i].AllowedTypes.Insert(13, ButtonType.Pov2_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov2_Center))
                            LogicalButtons[i].AllowedTypes.Insert(14, ButtonType.Pov2_Center);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Down))
                            LogicalButtons[i].AllowedTypes.Insert(15, ButtonType.Pov3_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Up))
                            LogicalButtons[i].AllowedTypes.Insert(16, ButtonType.Pov3_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Left))
                            LogicalButtons[i].AllowedTypes.Insert(17, ButtonType.Pov3_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov3_Right))
                            LogicalButtons[i].AllowedTypes.Insert(18, ButtonType.Pov3_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Down))
                            LogicalButtons[i].AllowedTypes.Insert(19, ButtonType.Pov4_Down);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Up))
                            LogicalButtons[i].AllowedTypes.Insert(20, ButtonType.Pov4_Up);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Left))
                            LogicalButtons[i].AllowedTypes.Insert(21, ButtonType.Pov4_Left);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Pov4_Right))
                            LogicalButtons[i].AllowedTypes.Insert(22, ButtonType.Pov4_Right);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton1))
                            LogicalButtons[i].AllowedTypes.Insert(23, ButtonType.RadioButton1);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton2))
                            LogicalButtons[i].AllowedTypes.Insert(24, ButtonType.RadioButton2);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton3))
                            LogicalButtons[i].AllowedTypes.Insert(25, ButtonType.RadioButton3);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton4))
                            LogicalButtons[i].AllowedTypes.Insert(26, ButtonType.RadioButton4);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Sequential_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(27, ButtonType.Sequential_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Sequential_Button))
                            LogicalButtons[i].AllowedTypes.Insert(28, ButtonType.Sequential_Button);

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
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Button_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(1, ButtonType.Button_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_OnOff))
                            LogicalButtons[i].AllowedTypes.Insert(2, ButtonType.ToggleSwitch_OnOff);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_On))
                            LogicalButtons[i].AllowedTypes.Insert(3, ButtonType.ToggleSwitch_On);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.ToggleSwitch_Off))
                            LogicalButtons[i].AllowedTypes.Insert(4, ButtonType.ToggleSwitch_Off);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton1))
                            LogicalButtons[i].AllowedTypes.Insert(5, ButtonType.RadioButton1);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton2))
                            LogicalButtons[i].AllowedTypes.Insert(6, ButtonType.RadioButton2);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton3))
                            LogicalButtons[i].AllowedTypes.Insert(7, ButtonType.RadioButton3);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.RadioButton4))
                            LogicalButtons[i].AllowedTypes.Insert(8, ButtonType.RadioButton4);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Sequential_Toggle))
                            LogicalButtons[i].AllowedTypes.Insert(9, ButtonType.Sequential_Toggle);
                        if (!LogicalButtons[i].AllowedTypes.Contains(ButtonType.Sequential_Button))
                            LogicalButtons[i].AllowedTypes.Insert(10, ButtonType.Sequential_Button);

                        if (LogicalButtons[i].Config.Type == ButtonType.Encoder_A ||
                            LogicalButtons[i].Config.Type == ButtonType.Encoder_B ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Down ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Up ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Left ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Right ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov1_Center ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Down ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Up ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Left ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Right ||
                            LogicalButtons[i].Config.Type == ButtonType.Pov2_Center ||
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
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov1_Center);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Down);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Up);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Left);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Right);
                        LogicalButtons[i].AllowedTypes.Remove(ButtonType.Pov2_Center);
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
                tmp.ButtonConfig[i].IsInverted = LogicalButtons[i].Config.IsInverted;
                tmp.ButtonConfig[i].IsDisabled = LogicalButtons[i].Config.IsDisabled;  
                tmp.ButtonConfig[i].ButtonDelayNumber = LogicalButtons[i].Config.ButtonDelayNumber;             //!!!!!
                tmp.ButtonConfig[i].ButtonToggleNumber = LogicalButtons[i].Config.ButtonToggleNumber;
            }
            Config = tmp;

            RaisePropertyChanged(nameof(LogicalButtons));
            ConfigChanged();
        }

        // Add a line to the activity log text box
        private void WriteLog(string message, bool clear)
        {
            // Replace content
            if (clear)
            {
                ButtonsLog = string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), message);
            }
            // Add new line
            else
            {
                ButtonsLog += Environment.NewLine + string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), message);
            }
            while (ButtonsLog.Length > 5000)
            {
                int i;
                for (i = 0; i < 100; i++)
                {
                    if (ButtonsLog.ElementAt(i) == '\n') break;
                }
              ButtonsLog = ButtonsLog.Remove(0, i+1);
            }
            RaisePropertyChanged("ButtonsLog");
        }
    }

}