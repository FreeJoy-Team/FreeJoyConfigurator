using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FreeJoyConfigurator
{
    public class DeviceConfigVM : BindableBase
    {
        public DeviceConfig Config { get; set; }
        

        #region Commands
        public DelegateCommand GetDeviceConfig { get; }
        public DelegateCommand SendDeviceConfig { get; }
        #endregion

        public DeviceConfigVM( DeviceConfig config)
        {
            Config = config;
            GetDeviceConfig = new DelegateCommand(() => Config.GetConfigRequest());
            SendDeviceConfig = new DelegateCommand(() => Config.SendConfig());
        }

    }
}
