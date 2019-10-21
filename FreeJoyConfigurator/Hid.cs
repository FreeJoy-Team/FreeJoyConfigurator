using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace FreeJoyConfigurator
{
    static class Hid
    {
        #region Fields
        private const int vid = 0x0483;
        private const int pid = 0x2619;

        private static HidDevice hidDevice { get; set; }

        public delegate void PacketReceivedEventHandler(HidReport hr);
        public delegate void PacketSentEventHandler(HidReport hr);
        public delegate void DeviceAddedEventHandler(HidDevice hd);
        public delegate void DeviceRemovedEventHandler(HidDevice hd);

        public static event PacketReceivedEventHandler PacketReceived;
        public static event PacketSentEventHandler PacketSent;
        public static event DeviceAddedEventHandler DeviceAdded;
        public static event DeviceRemovedEventHandler DeviceRemoved;

        
        public static bool IsConnected
        {
            get
            {
                if (hidDevice != null)
                    return hidDevice.IsConnected;
                else
                    return false;
            }
        }
        #endregion

        static public void Connect()
        {
            if (!IsConnected)
            {
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
                });
            }
        }

        #region HID Callbacks
        static private void HidDeviceAddedEventHandler()
        {
            DeviceAdded?.Invoke(hidDevice);
            hidDevice.ReadReport(ReadReportCallback);
        }

        static private void HidDeviceRemovedEventHandler()
        {
            DeviceRemoved?.Invoke(hidDevice);
        }

        static private void ReadReportCallback(HidReport report)
        {
            HidReport hr = report;

            // raise event for received packet
            PacketReceived?.Invoke(hr);
            
            // wait for new packet
            if (hidDevice.IsConnected)
            {
                hidDevice.ReadReport(ReadReportCallback);
            }
        }
        #endregion

        #region Hid data sending
        static public void ReportSend(byte reportId, byte[] data)
        {
            HidReport hr;
            byte[] buffer = new byte[data.Length + 1];
            Array.ConstrainedCopy(data, 0, buffer, 1, data.Length);


            hr = new HidReport(buffer.Length, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success));
            hr.ReportId = (byte)reportId;

            hidDevice.WriteReport(hr, 1000000);

            // raise event
            PacketSent?.Invoke(hr);
        }

        static public void ReportSend(HidReport report)
        {
            HidReport hr = report;

            hidDevice.WriteReport(hr, 1000000);

            // raise event
            PacketSent?.Invoke(hr);
        }
        #endregion
    }
}
