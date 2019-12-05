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
        private DeviceConfig _config;

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
            _config = deviceConfig;
            Axes = new ObservableCollection<Axis>(_joystick.Axes);

            axesToButtons = new ObservableCollection<AxisToButtonsVM>();
            for (int i = 0; i < 16; i++)
            {
                axesToButtons.Add(new AxisToButtonsVM());
                if (_config.PinConfig[i] == PinType.AxisToButtons)
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

        public void Update(DeviceConfig config)
        {
            _config = config;
            for (int i = 0; i < AxesToButtons.Count; i++)
            {
                if (_config.PinConfig[i] == PinType.AxisToButtons)
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
