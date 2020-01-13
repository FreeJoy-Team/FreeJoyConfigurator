using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class AxesToButtonsVM : BindableBase
    {
        public delegate void PinConfigChangedEvent();
        public event PinConfigChangedEvent ConfigChanged;

        private ObservableCollection<AxisToButtons> axesToButtons;
        private Joystick _joystick;
        private DeviceConfig _config;
        public DeviceConfig Config
        {
            get { return _config; }
            set { SetProperty(ref _config, value); }
        }

        public ObservableCollection<Axis> Axes { get; private set; }
        public ObservableCollection<AxisToButtons> AxesToButtons
        {
            get
            {
                return axesToButtons;
            }
            set
            {
                SetProperty(ref axesToButtons, value);
            }
        }

        public AxesToButtonsVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            _joystick = joystick;
            _config = deviceConfig;
            Axes = new ObservableCollection<Axis>(_joystick.Axes);

            axesToButtons = new ObservableCollection<AxisToButtons>();
            for (int i = 0; i < 8; i++)
            {
                axesToButtons.Add(new AxisToButtons());
                if (Config.PinConfig[i] == PinType.AxisToButtons)
                {
                    AxesToButtons[i].IsEnabled = true;
                }
                else
                {
                    AxesToButtons[i].IsEnabled = false;
                    AxesToButtons[i].ButtonCnt = 2;
                }

                AxesToButtons[i].PropertyChanged += AxesToButtonsVM_PropertyChanged;
                for (int k =0; k<AxesToButtons[i].RangeItems.Count; k++)
                {
                    AxesToButtons[i].RangeItems[k].PropertyChanged += AxesToButtonsVM_Range_PropertyChanged;
                }
            }
            
        }

        private void AxesToButtonsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            for (int i = 0; i < AxesToButtons.Count; i++)
            {
                // disable range changed notification
                for (int k = 0; k < AxesToButtons[i].RangeItems.Count; k++)
                {
                    AxesToButtons[i].RangeItems[k].PropertyChanged -= AxesToButtonsVM_Range_PropertyChanged;
                }

                while (AxesToButtons[i].ButtonCnt > AxesToButtons[i].RangeItems.Count)
                {
                    for (int j = 0; j < AxesToButtons[i].RangeItems.Count; j++)
                    {
                        AxesToButtons[i].RangeItems[j].From = j * (100 / (AxesToButtons[i].RangeItems.Count + 1));
                        AxesToButtons[i].RangeItems[j].To = (j + 1) * (100 / (AxesToButtons[i].RangeItems.Count + 1));
                    }
                    AxesToButtons[i].RangeItems.Add(new RangeItem { From = AxesToButtons[i].RangeItems.Last().To, To = 100 });
                }
                while (AxesToButtons[i].ButtonCnt < AxesToButtons[i].RangeItems.Count)
                {
                    for (int j = AxesToButtons[i].RangeItems.Count - 2; j >= 0; j--)
                    {
                        AxesToButtons[i].RangeItems[j].From = j * (100 / (AxesToButtons[i].RangeItems.Count - 1));
                        AxesToButtons[i].RangeItems[j].To = (j + 1) * (100 / (AxesToButtons[i].RangeItems.Count - 1));
                    }
                    AxesToButtons[i].RangeItems[AxesToButtons[i].RangeItems.Count - 1].To = 100;
                    AxesToButtons[i].RangeItems.Remove(AxesToButtons[i].RangeItems.Last());
                }

                // enable range changed notification
                for (int k = 0; k < AxesToButtons[i].RangeItems.Count; k++)
                {
                    AxesToButtons[i].RangeItems[k].PropertyChanged += AxesToButtonsVM_Range_PropertyChanged;
                }
            }

            AxesToButtonsVM_Range_PropertyChanged(null, null);
        }

        private void AxesToButtonsVM_Range_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DeviceConfig conf = Config;

            for (int i = 0; i < conf.AxisToButtonsConfig.Count; i++)
            {
                conf.AxisToButtonsConfig[i].ButtonsCnt = (byte)AxesToButtons[i].ButtonCnt;

                for (int j = 0; j < AxesToButtons[i].ButtonCnt; j++)
                {
                    conf.AxisToButtonsConfig[i].Points[j] = (sbyte)AxesToButtons[i].RangeItems[j].From;
                }
                conf.AxisToButtonsConfig[i].Points[AxesToButtons[i].ButtonCnt] =
                    (sbyte)AxesToButtons[i].RangeItems.Last().To;
            }
            Config = conf;

            ConfigChanged();
        }

        public void Update(DeviceConfig config)
        {
            Config = config;

            ObservableCollection<AxisToButtons> tmp = new ObservableCollection<AxisToButtons>();

            for (int i = 0; i < Config.AxisToButtonsConfig.Count; i++)
            {
                tmp.Add(new AxisToButtons());

                if (Config.PinConfig[i] == PinType.AxisToButtons)
                {
                    tmp[i].IsEnabled = true;

                    // change button count
                    tmp[i].ButtonCnt = Config.AxisToButtonsConfig[i].ButtonsCnt;

                    // change range count
                    while (tmp[i].ButtonCnt > tmp[i].RangeItems.Count)
                    {
                        for (int j = 0; j < tmp[i].RangeItems.Count; j++)
                        {
                            tmp[i].RangeItems[j].From = j * (100 / (tmp[i].RangeItems.Count + 1));
                            tmp[i].RangeItems[j].To = (j + 1) * (100 / (tmp[i].RangeItems.Count + 1));
                        }
                        tmp[i].RangeItems.Add(new RangeItem { From = tmp[i].RangeItems.Last().To, To = 100 });
                    }
                    while (tmp[i].ButtonCnt < tmp[i].RangeItems.Count)
                    {
                        for (int j = tmp[i].RangeItems.Count - 2; j >= 0; j--)
                        {
                            tmp[i].RangeItems[j].From = j * (100 / (tmp[i].RangeItems.Count - 1));
                            tmp[i].RangeItems[j].To = (j + 1) * (100 / (tmp[i].RangeItems.Count - 1));
                        }
                        tmp[i].RangeItems[tmp[i].RangeItems.Count - 1].To = 100;
                        tmp[i].RangeItems.Remove(tmp[i].RangeItems.Last());
                    }

                    // change ranges
                    for (int j=0; j < tmp[i].RangeItems.Count; j++)
                    {
                        tmp[i].RangeItems[j].From = Config.AxisToButtonsConfig[i].Points[j];
                        tmp[i].RangeItems[j].To = Config.AxisToButtonsConfig[i].Points[j+1];
                    }  
                }
                else
                {
                    tmp[i].IsEnabled = false;
                    tmp[i].ButtonCnt = 2;
                }
            }

            for (int i = 0; i < AxesToButtons.Count; i++)
            {
                AxesToButtons[i].IsEnabled = tmp[i].IsEnabled;
                AxesToButtons[i].ButtonCnt = tmp[i].ButtonCnt;

                for (int j = 0; j < AxesToButtons[i].RangeItems.Count; j++)
                {
                    AxesToButtons[i].RangeItems[j].PropertyChanged -= AxesToButtonsVM_Range_PropertyChanged;
                    AxesToButtons[i].RangeItems[j] = tmp[i].RangeItems[j];
                    AxesToButtons[i].RangeItems[j].PropertyChanged += AxesToButtonsVM_Range_PropertyChanged;                    
                }
            }     
        }
    }
}
