using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class DeviceConfigExchangerVM
    {
        static private byte configPacketNumber = 0;
        static private DeviceConfig _config; 

        public delegate void ConfigReceivedEventHandler(DeviceConfig deviceConfig);
        public delegate void ConfigSentEventHandler(DeviceConfig deviceConfig);

        public event ConfigReceivedEventHandler Received;
        public event ConfigSentEventHandler Sent;


        public DeviceConfigExchangerVM()
        {
            _config = new DeviceConfig();
            //Hid.Connect();
            Hid.PacketReceived += PacketReceivedEventHandler;
        }

        public void PacketReceivedEventHandler(HidReport report)
        {
            List<HidReport> hrs;
            HidReport hr = report;
            byte[] buffer = new byte[1];

            switch ((ReportID)hr.ReportId)
            {
                case ReportID.CONFIG_IN_REPORT:
                    configPacketNumber = hr.Data[0];
                    Console.WriteLine("Config packet received: {0}", configPacketNumber);

                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ReportConverter.ReportToConfig(ref _config, hr);
                    }));

                    if (configPacketNumber < 16)
                    {
                        buffer[0] = ++configPacketNumber;
                        Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
                        Console.WriteLine("Requesting config packet..: {0}", configPacketNumber);
                    }
                    else
                    {
                        App.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Received(_config);
                        }));
                    }
                    break;

                case ReportID.CONFIG_OUT_REPORT:
                    configPacketNumber = hr.Data[0];
                    hrs = ReportConverter.ConfigToReports(_config);

                    Console.WriteLine("Config packet requested: {0}", configPacketNumber);

                    Hid.ReportSend(hrs[configPacketNumber - 1]);
                    Console.WriteLine("Sending config packet..: {0}", configPacketNumber);

                    if (configPacketNumber >= 16)
                    {
                        App.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Sent(_config);
                        }));
                    }
                    break;

                default:
                    break;
            }
        }

        public void GetConfigRequest()
        {
            byte[] buffer = new byte[1];

            buffer[0] = 255;
            Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
            Console.WriteLine("Setting device into config mode");

            Task.Delay(250);

            buffer[0] = 1;
            Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
            Console.WriteLine("Requesting config packet..: 1");
        }

        public void SendConfig(DeviceConfig config)
        {
            byte[] buffer = new byte[1];

            buffer[0] = 255;
            Hid.ReportSend((byte)ReportID.CONFIG_IN_REPORT, buffer);
            Console.WriteLine("Setting device into config mode");

            Task.Delay(250);

            List<HidReport> hr;
            _config = config;

            hr = ReportConverter.ConfigToReports(_config);

            Hid.ReportSend(hr[0]);
            Console.WriteLine("Sending config packet..: 1");
        }
    }

}
