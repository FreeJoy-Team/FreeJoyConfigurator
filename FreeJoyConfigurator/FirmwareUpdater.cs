using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class FirmwareUpdater : BindableBase
    {
        private string _filepath;

        private int _updatePercent;

        public delegate void FlashFinishedEventHandler();
        public event FlashFinishedEventHandler Finished;

        public int UpdatePercent
        {
            get
            {
                return _updatePercent;
            }
            private set
            {
                SetProperty(ref _updatePercent, value);
            }
        }
        

        public FirmwareUpdater()
        {
            _filepath = "./FreeJoy.bin";
            _updatePercent = 0;

            Hid.PacketReceived += PacketReceivedEventHandler;
        }

        public void PacketReceivedEventHandler(HidReport report)
        {
            
            HidReport hr = report;
            byte[] fileArray;
            byte[] buffer = new byte[63];            

            if (hr.ReportId == (byte)ReportID.FIRMWARE_REPORT)
            {
                ushort cnt = (ushort)(hr.Data[0] << 8 | hr.Data[1]);
                Console.WriteLine("Firmware packet requested: {0}", cnt);

                fileArray = File.ReadAllBytes(_filepath);


                buffer[0] = (byte)(cnt>>8);
                buffer[1] = (byte)(cnt&0xFF);
                buffer[2] = 0;
                if (cnt * 60 < fileArray.Length)
                {
                    Array.ConstrainedCopy(fileArray, (cnt - 1) * 60, buffer, 3, 60);
                    UpdatePercent = (((cnt - 1) * 60 * 100 / fileArray.Length) );
                }
                else
                {
                    Array.ConstrainedCopy(fileArray, (cnt - 1) * 60, buffer, 3, fileArray.Length-(cnt-1)*60);
                    UpdatePercent = 0;
                    Finished();
                }

                

                Hid.ReportSend((byte)ReportID.FIRMWARE_REPORT, buffer);
                Console.WriteLine("Firmware packet sent: {0}", cnt);
            }
        }

        public void SendFirmware()
        {
            byte[] buffer = new byte[63];
            ushort length = (ushort) new FileInfo(_filepath).Length;

            UpdatePercent = 0;

            buffer[0] = 0;
            buffer[1] = 0;
            buffer[2] = 0;
            buffer[3] = (byte)(length & 0xFF);
            buffer[4] = (byte)(length >> 8);
            

            Hid.ReportSend((byte)ReportID.FIRMWARE_REPORT, buffer);
            Console.WriteLine("Firmware packet sent: 0");
        }
    }
}
