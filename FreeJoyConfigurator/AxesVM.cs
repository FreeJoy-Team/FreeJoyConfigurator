using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class AxesVM : BindableBase
    {
        
        public Joystick Joystick;
        public DeviceConfig Config { get; set; }

        public ObservableCollection<Axis> Axes { get; private set; }

        public AxesVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            Joystick = joystick;
            Config = deviceConfig;

            Axes = new ObservableCollection<Axis>(joystick.Axes);

        }
    }
}
