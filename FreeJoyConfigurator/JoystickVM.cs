using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class JoystickVM : BindableBase
    {
        public Joystick Joystick;

        public ObservableCollection<Axis> Axes { get; private set; }
        public ObservableCollection<Button> Buttons { get; private set; }

        public JoystickVM(Joystick joystick)
        {
            Joystick = joystick;

            Axes = new ObservableCollection<Axis>(joystick.Axes);
            Buttons = new ObservableCollection<Button>(joystick.Buttons);

            joystick.PropertyChanged += (s, a) => { RaisePropertyChanged(nameof(Joystick.Axes)); };

        }
    }
}
