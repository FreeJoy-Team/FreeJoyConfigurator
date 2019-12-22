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
        private ObservableCollection<AxisToButtonsVM> axesToButtons;
        private Joystick _joystick;
        public DeviceConfig Config { get; set; }

        public ObservableCollection<Axis> Axes { get; private set; }
        public ObservableCollection<AxisToButtonsVM> AxesToButtons
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
            Config = deviceConfig;
            Axes = new ObservableCollection<Axis>(_joystick.Axes);

            axesToButtons = new ObservableCollection<AxisToButtonsVM>();
            for (int i = 0; i < 8; i++)
            {
                axesToButtons.Add(new AxisToButtonsVM());
                if (Config.PinConfig[i] == PinType.AxisToButtons)
                {
                    AxesToButtons[i].IsEnabled = true;
                }
                else
                {
                    AxesToButtons[i].IsEnabled = false;
                    AxesToButtons[i].ButtonCnt = 2;
                }
                AxesToButtons[i].RangeItems.CollectionChanged += RangeItems_CollectionChanged;
            }
            
        }

        private void RangeItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DeviceConfig conf = Config;

            for (int i = 0; i < conf.AxisToButtonsConfig.Count; i++)
            {
                conf.AxisToButtonsConfig[i].ButtonsCnt = (byte) AxesToButtons[i].ButtonCnt;

                for (int j = 0; j < AxesToButtons[i].ButtonCnt; j++)
                {
                    conf.AxisToButtonsConfig[i].Points[j] = (sbyte)AxesToButtons[i].RangeItems[j].From;
                }
                conf.AxisToButtonsConfig[i].Points[AxesToButtons[i].ButtonCnt] = 
                    (sbyte)AxesToButtons[i].RangeItems.Last().To;
            }
            Config = conf;
        }

        public void Update(DeviceConfig config)
        {
            Config = config;
            for (int i = 0; i < AxesToButtons.Count; i++)
            {
                if (Config.PinConfig[i] == PinType.AxisToButtons)
                {
                    AxesToButtons[i].IsEnabled = true;
                }
                else
                {
                    AxesToButtons[i].IsEnabled = false;
                    AxesToButtons[i].ButtonCnt = 2;
                }
            }
        }
    }
}
