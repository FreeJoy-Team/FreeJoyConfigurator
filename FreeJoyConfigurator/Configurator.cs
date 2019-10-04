using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace FreeJoyConfigurator
{
    public class Configurator
    {
        #region Fields

        

        public delegate void PacketReceivedEventHandler(object sender, HidReport report);
        public delegate void PacketSentEventHandler(object sender, HidReport report);
        public delegate void DeviceAddedEventHandler(object sender, HidDevice device);
        public delegate void DeviceRemovedEventHandler(object sender, HidDevice device);

        public event PacketReceivedEventHandler PacketReceived;
        public event PacketSentEventHandler PacketSent;
        public event DeviceAddedEventHandler DeviceAdded;
        public event DeviceRemovedEventHandler DeviceRemoved;

        public HidDevice HidDevice { get; private set; }




        public bool IsConnected
        {
            get
            {
                if (HidDevice != null)
                    return HidDevice.IsConnected;
                else
                    return false;
            }
            private set
            {

            }
        }

        public int Vid { get; private set; }
        public int Pid { get; private set; }

        #endregion

        #region Constructor

        public Configurator()
        {
            Vid = 0x0483;
            Pid = 0x2619;

            IsConnected = false;

            Task.Run(() =>
            {
                while (HidDevice == null)
                {
                    HidDevice = HidDevices.Enumerate(Vid, Pid).FirstOrDefault();
                    Thread.Sleep(500);
                }
                HidDevice.OpenDevice();

                HidDevice.Inserted += HidDeviceAddedEventHandler;
                HidDevice.Removed += HidDeviceRemovedEventHandler;

                HidDevice.MonitorDeviceEvents = true;
            });
        }
        #endregion

        #region HID Callbacks

        private void HidDeviceAddedEventHandler()
        {
            DeviceAdded(this, this.HidDevice);
            HidDevice.ReadReport(ReadReportCallback);
        }

        private void HidDeviceRemovedEventHandler()
        {
            DeviceRemoved(this, this.HidDevice);
        }

        private void ReadReportCallback(HidReport report)
        {
            HidReport hr = report;

            // TODO: Read report callback logic
    

            // raise event for received packet
            PacketReceived(this, hr);
            // wait for new packet
            if (HidDevice.IsConnected)
            {
                HidDevice.ReadReport(ReadReportCallback);
            }
        }
        #endregion
    }
}
