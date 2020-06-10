﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        //private const int vid = 0x0483;
        //private const int pid = 0x5750;

        private static HidDevice hidDevice { get; set; }
        public static List<HidDevice> HidDevicesList { get; set; }

        public delegate void PacketReceivedEventHandler(HidReport hr);
        public delegate void PacketSentEventHandler(HidReport hr);
        public delegate void DeviceAddedEventHandler(HidDevice hd);
        public delegate void DeviceRemovedEventHandler(HidDevice hd);
        public delegate void DeviceListUpdatedEventHandler();

        public static event PacketReceivedEventHandler PacketReceived;
        public static event PacketSentEventHandler PacketSent;
        public static event DeviceAddedEventHandler DeviceAdded;
        public static event DeviceRemovedEventHandler DeviceRemoved;
        public static event DeviceListUpdatedEventHandler DeviceListUpdated;

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

        static public void Start(int vid)
        {
            int lastDeviceCnt = 0;
            HidDevicesList = new List<HidDevice>();

            if (!IsConnected)
            {
                var _hidTask = Task.Factory.StartNew(() =>
                {
                    //while (hidDevice == null)
                    while (true)
                    {
                        HidDevicesList = HidDevices.Enumerate(vid).ToList();

                        if (HidDevicesList.Count != lastDeviceCnt)
                        {
                            Console.WriteLine("Device list updated");
                            DeviceListUpdated?.Invoke();
                        }
                        lastDeviceCnt = HidDevicesList.Count;
                        Thread.Sleep(50);
                    } 
                });
            }
        }

        static public void Connect(HidDevice device)
        {
            hidDevice = device;

            if (!hidDevice.IsOpen)
            {
                hidDevice.OpenDevice();

                hidDevice.Inserted += HidDeviceAddedEventHandler;
                hidDevice.Removed += HidDeviceRemovedEventHandler;
                hidDevice.MonitorDeviceEvents = true;
            }

        }


        static public ObservableCollection<HidDevice> GetDevices(int vid)
        {

            ObservableCollection<HidDevice> devices = new ObservableCollection<HidDevice>( HidDevices.Enumerate(vid));

            return devices;
        }

        #region HID Callbacks
        static private void HidDeviceAddedEventHandler(HidDevice hd)
        {
            Console.WriteLine("Device added");
            DeviceAdded?.Invoke(hd);
            hd.ReadReport(ReadReportCallback);
        }

        static private void HidDeviceRemovedEventHandler(HidDevice hd)
        {
            hd.Inserted -= HidDeviceAddedEventHandler;
            hd.Removed -= HidDeviceRemovedEventHandler;
            hd.MonitorDeviceEvents = false;
            hd.CloseDevice();

            Console.WriteLine("Device removed");
            DeviceRemoved?.Invoke(hd);
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
