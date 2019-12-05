using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class AxesVM : BindableBase
    {
        public DeviceConfig Config { get; set; }
        public AxesCurvesVM AxesCurvesVM { get; set; }
        public Joystick Joystick;       

        public ObservableCollection<Axis> Axes { get; private set; }

        public AxesVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            Joystick = joystick;
            Config = deviceConfig;

            AxesCurvesVM = new AxesCurvesVM(Config);

            Axes = new ObservableCollection<Axis>(joystick.Axes);
        }

        public void Update(DeviceConfig config)
        {
            Config = config;
            for (int i = 0; i < 8; i++)
            {
                if (Config.PinConfig[i] == PinType.AxisAnalog)
                {
                    Axes[i].IsEnabled = true;
                }
                else Axes[i].IsEnabled = false;
            }
            AxesCurvesVM.Update(Config);
        }
    }
}
