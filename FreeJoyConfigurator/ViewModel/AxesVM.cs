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

        private DeviceConfig _config;
        private AxesCurvesVM _axesCurvesVM;
        private Joystick _joystick;
        private ObservableCollection<Axis> _axes;

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
        public AxesCurvesVM AxesCurvesVM
        {
            get
            {
                return _axesCurvesVM;
            }
            set
            {
                SetProperty(ref _axesCurvesVM, value);
            }
        }      

        public ObservableCollection<Axis> Axes
        {
            get
            {
                return _axes;
            }
            private set
            {
                SetProperty(ref _axes, value);
            }
        }

        public AxesVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            _joystick = joystick;
            _config = deviceConfig;

            AxesCurvesVM = new AxesCurvesVM(_config);

            Axes = new ObservableCollection<Axis>(_joystick.Axes);
        }

        public void Update(DeviceConfig config)
        {
            Config = config;
            for (int i = 0; i < 8; i++)
            {
                if (Config.PinConfig[i] == PinType.AxisAnalog || Config.PinConfig[i] == PinType.AxisToButtons)
                {
                    Axes[i].IsEnabled = true;
                }
                else Axes[i].IsEnabled = false;
            }
            AxesCurvesVM.Update(Config);
        }
    }
}
