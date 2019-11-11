using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class FirmwareUpdaterVM : BindableBase
    {
        
        public FirmwareUpdater FirmwareUpdater { get; set; }

        public DelegateCommand FlashFirmware { get; }

        public FirmwareUpdaterVM()
        {
            FirmwareUpdater = new FirmwareUpdater();
            FirmwareUpdater.Finished += FirmwareUpdater_Finished;

            FlashFirmware = new DelegateCommand(() => FirmwareUpdater.SendFirmware());
        }

        private void FirmwareUpdater_Finished()
        {
            // TODO: Flash finished message box
        }
    }
}
