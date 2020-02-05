using MessageBoxServicing;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
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
            FirmwareUpdater.CrcError += FirmwareUpdater_CrcError;
            FirmwareUpdater.SizeError += FirmwareUpdater_SizeError;
            FirmwareUpdater.EraseError += FirmwareUpdater_EraseError;

            FlashFirmware = new DelegateCommand(() => StartUpdate());
        }

        private void FirmwareUpdater_EraseError()
        {
            MessageBoxService mbs = new MessageBoxService();

            mbs.ShowMessage("Memory erase error!\r\nFlashing aborted", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        private void FirmwareUpdater_SizeError()
        {
            MessageBoxService mbs = new MessageBoxService();

            mbs.ShowMessage("Firmware size error!\r\nFlashing aborted", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        private void FirmwareUpdater_CrcError()
        {
            MessageBoxService mbs = new MessageBoxService();

            mbs.ShowMessage("Firmware checksum error!\r\nFlashing aborted", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        private void FirmwareUpdater_Finished()
        {
            MessageBoxService mbs = new MessageBoxService();

            mbs.ShowMessage("Firmware downloading is done!\r\nNow wait until device disconnect, unplug it and plug again", "Info",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void StartUpdate ()
        {
            MessageBoxService mbs = new MessageBoxService();

            if (mbs.ShowMessage("This action may damage your device. Are you sure you want to continue?",
                "Warning", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
            {
                OpenFileDialog dlg = new OpenFileDialog();

                try
                {
                    File.WriteAllBytes(Environment.CurrentDirectory + "/FreeJoy.bin",
                        FreeJoyConfigurator.Properties.Resources.FreeJoy);
                    dlg.InitialDirectory = Environment.CurrentDirectory;
                }
                catch
                {

                }

                
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
