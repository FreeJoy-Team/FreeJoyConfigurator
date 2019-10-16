using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace FreeJoyConfigurator
{
    public class Hid
    {
        #region Fields
        private HidDevice hidDevice { get; set; }
        private int vid { get; set; }
        private int pid { get; set; }

        public delegate void PacketReceivedEventHandler(object sender, HidReport hr);
        public delegate void PacketSentEventHandler(object sender, HidReport hr);
        public delegate void DeviceAddedEventHandler(object sender, HidDevice hd);
        public delegate void DeviceRemovedEventHandler(object sender, HidDevice hd);

        public event PacketReceivedEventHandler PacketReceived;
        public event PacketSentEventHandler PacketSent;
        public event DeviceAddedEventHandler DeviceAdded;
        public event DeviceRemovedEventHandler DeviceRemoved;

        
        public bool IsConnected
        {
            get
            {
                if (hidDevice != null)
                    return hidDevice.IsConnected;
                else
                    return false;
            }
            private set
            {

            }
        }
        #endregion

        #region Constructor
        public Hid()
        {

            vid = 0x0483;
            pid = 0x2619;
            IsConnected = false;

            Task.Run(() =>
            {
                while (hidDevice == null)
                {
                    hidDevice = HidDevices.Enumerate(vid, pid).FirstOrDefault();
                    Thread.Sleep(500);
                }

                if (!hidDevice.IsOpen)
                {

                        hidDevice.OpenDevice();

                        hidDevice.Inserted += HidDeviceAddedEventHandler;
                        hidDevice.Removed += HidDeviceRemovedEventHandler;
                        hidDevice.MonitorDeviceEvents = true;
                }
                else
                {

                }
            });
        }
        #endregion

        #region HID Callbacks
        private void HidDeviceAddedEventHandler()
        {
            DeviceAdded(this, hidDevice);
            hidDevice.ReadReport(ReadReportCallback);
        }

        private void HidDeviceRemovedEventHandler()
        {
            DeviceRemoved(this, hidDevice);
        }

        private void ReadReportCallback(HidReport report)
        {
            HidReport hr = report;

            // raise event for received packet
            PacketReceived(this, hr);
            Console.WriteLine("Report received");
            // wait for new packet
            if (hidDevice.IsConnected)
            {
                hidDevice.ReadReport(ReadReportCallback);
            }
        }
        #endregion

        #region Hid data sending
        public void ReportSend(byte reportId, byte[] data)
        {
            HidReport hr;
            byte[] buffer = new byte[data.Length + 1];

            hr = new HidReport(buffer.Length, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success));
            hr.ReportId = (byte)reportId;

            hidDevice.WriteReport(hr);

            // raise event
            PacketSent(this, hr);
            Console.WriteLine("Report sent");
        }
        #endregion
    }
}
