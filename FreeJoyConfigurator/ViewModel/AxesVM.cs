using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Prism.Commands;
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

            for (int i = 0; i < Config.PinConfig.Count; i++)
            {
                if (Config.PinConfig[i] == PinType.Axis_Analog ||
                    Config.PinConfig[i] == PinType.TLE5011_CS ||
                    Config.PinConfig[i] == PinType.TLE5012B_CS ||
                    Config.PinConfig[i] == PinType.MCP3201_CS ||
                    Config.PinConfig[i] == PinType.MCP3202_CS ||
                    Config.PinConfig[i] == PinType.MCP3204_CS ||
                    Config.PinConfig[i] == PinType.MCP3208_CS ||
                    Config.PinConfig[i] == PinType.MLX90393_CS ||
                    Config.PinConfig[i] == PinType.AS5048A_CS)
                {
                    foreach (var axis in Axes)
                    {
                        if (!axis.AllowedSources.Contains((AxisSourceType)i))
                        {
                            axis.AllowedSources.Add((AxisSourceType)i);
                        }
                    }
                }
                else
                {
                    foreach (var axis in Axes)
                    {
                        if (axis.AxisConfig.SourceMain == (AxisSourceType)i) axis.AxisConfig.SourceMain = AxisSourceType.None;
                        axis.AllowedSources.Remove((AxisSourceType)i);
                        //
                    }

                }
            }

            if (Config.PinConfig[21] == PinType.I2C_SCL && Config.PinConfig[22] == PinType.I2C_SDA)    // PB8 and PB9
            {
                foreach (var axis in Axes)
                {
                    if (!axis.AllowedSources.Contains(AxisSourceType.I2C))
                    {
                        axis.AllowedSources.Add(AxisSourceType.I2C);
                    }
                }
            }
            else
            {
                foreach (var axis in Axes)
                {
                    if (axis.AxisConfig.SourceMain == AxisSourceType.I2C) axis.AxisConfig.SourceMain = AxisSourceType.None;
                    if (axis.AllowedSources.Contains(AxisSourceType.I2C)) axis.AllowedSources.Remove(AxisSourceType.I2C);
                }
            }

            for (int i = 0; i < Config.AxisConfig.Count; i++)
            {
                Axes[i].AxisConfig = Config.AxisConfig[i];
            }


            AxesCurvesVM.Update(Config);
        }

        
    }
}
