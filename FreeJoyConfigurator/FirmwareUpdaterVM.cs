using MessageBoxServicing;
using Microsoft.Win32;
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

            FlashFirmware = new DelegateCommand(() => StartUpdate());
        }

        private void FirmwareUpdater_Finished()
        {
            MessageBoxService mbs = new MessageBoxService();

            mbs.ShowMessage("Firmware downloading is done!\r\nNow wait until device disconnect, unplug it and plug again", "Info");
        }

        private void StartUpdate ()
        {
            MessageBoxService mbs = new MessageBoxService();

            if (mbs.ShowMessage("This action may damage your device. Are you sure you want to continue?",
                "Warning", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.DefaultExt = ".bin";
                dlg.Filter = "Binary files (.bin)|*.bin";
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    FirmwareUpdater.SendFirmware(dlg.FileName);
                }
            }
        }
    }
}
