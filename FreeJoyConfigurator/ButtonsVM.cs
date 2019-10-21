using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class ButtonsVM : BindableBase
    {
       //private DeviceConfig _deviceConfig;

        public Joystick Joystick;
        public DeviceConfig Config { get; set; }

        public ObservableCollection<Button> Buttons { get; set; }

        public ButtonsVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            Joystick = joystick;
            Config = deviceConfig;

            Buttons = new ObservableCollection<Button>(joystick.Buttons);
        }
    }

}
