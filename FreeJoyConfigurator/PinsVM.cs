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
    public class PinsVM : BindableBase
    {
        public DeviceConfig Config { get; set; }   

        public PinsVM( DeviceConfig deviceConfig)
        {
            Config = deviceConfig;
        }

    }
}
