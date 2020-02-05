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

        public delegate void FlashEventHandler();
        public event FlashEventHandler Finished;
        public event FlashEventHandler SizeError;
        public event FlashEventHandler CrcError;
        public event FlashEventHandler EraseError;

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

                if ((cnt & 0xF000) == 0xF000)  // status packet
                {
                    if (cnt == 0xF001)  // firmware size error
                    {
                        SizeError();
                    }
                    else if (cnt == 0xF002) // CRC error
                    {
                        CrcError();
                    }
                    else if (cnt == 0xF003) // flash erase error
                    {
                        EraseError();
                    }
                    else if (cnt == 0xF000) // OK
                    {
                        Finished();
                    }
                }
                else
                {
                    Console.WriteLine("Firmware packet requested: {0}", cnt);

                    fileArray = File.ReadAllBytes(_filepath);

                    buffer[0] = (byte)(cnt >> 8);
                    buffer[1] = (byte)(cnt & 0xFF);
                    buffer[2] = 0;
                    if (cnt * 60 < fileArray.Length)
                    {
                        Array.ConstrainedCopy(fileArray, (cnt - 1) * 60, buffer, 3, 60);
                        UpdatePercent = (((cnt - 1) * 60 * 100 / fileArray.Length));

                        Hid.ReportSend((byte)ReportID.FIRMWARE_REPORT, buffer);
                        Console.WriteLine("Firmware packet sent: {0}", cnt);
                    }
                    else
                    {
                        Array.ConstrainedCopy(fileArray, (cnt - 1) * 60, buffer, 3, fileArray.Length - (cnt - 1) * 60);
                        UpdatePercent = 0;

                        Hid.ReportSend((byte)ReportID.FIRMWARE_REPORT, buffer);
                        Console.WriteLine("Firmware packet sent: {0}", cnt);

                    }

                    
                }
            }
        }

        public void SendFirmware(string filepath)
        {
            byte[] buffer = new byte[63];

            if (filepath != null)
            {
                _filepath = filepath;

                ushort length = (ushort)new FileInfo(_filepath).Length;

                ushort crc16 = Crc16.ComputeChecksum(File.ReadAllBytes(_filepath));

                UpdatePercent = 0;

                buffer[0] = 0;
                buffer[1] = 0;
                buffer[2] = 0;
                buffer[3] = (byte)(length & 0xFF);
                buffer[4] = (byte)(length >> 8);
                buffer[5] = (byte)(crc16 & 0xFF);
                buffer[6] = (byte)(crc16 >> 8);


                Hid.ReportSend((byte)ReportID.FIRMWARE_REPORT, buffer);
                Console.WriteLine("Firmware packet sent: 0");
            }
        }
    }
}
