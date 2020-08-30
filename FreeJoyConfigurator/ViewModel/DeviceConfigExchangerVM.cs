using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using MessageBoxServicing;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class DeviceConfigExchangerVM
    {
        static private byte configPacketNumber = 0;
        static private DeviceConfig _config;
        static private byte _requestedNumber = 1;

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
            DeviceConfig tmpConfig = new DeviceConfig();
            byte[] buffer = new byte[1];

            switch ((ReportID)hr.ReportId)
            {
                case ReportID.CONFIG_IN_REPORT:
                    configPacketNumber = hr.Data[0];
                    Console.WriteLine("Config packet received: {0}", configPacketNumber);

                    // exit if received packet wrong type
                    if (configPacketNumber != _requestedNumber)
                    {
                        Console.WriteLine("Unexpected packet received!");
                        return;
                    }

                    if (configPacketNumber == 1)
                    {
                        ReportConverter.ReportToConfig(ref tmpConfig, hr);
                        Version ver = Assembly.GetEntryAssembly().GetName().Version;

                        if ((tmpConfig.FirmwareVersion & 0xFFF0) != (ushort)((ver.Major<<12)|(ver.Minor<<8)|(ver.Build<<4)))
                        {
                            MessageBoxService mbs = new MessageBoxService();

                            mbs.ShowMessage("Device firmware is not compatible with this version of the Configurator\r\n\n" +
                                "Device firmware: v" + 
                                tmpConfig.FirmwareVersion.ToString("X3").Insert(1, ".").Insert(3, ".").Insert(5, "b") + 
                                "\r\nCofigurator version: " +
                                string.Format("v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build, Assembly.GetEntryAssembly().GetName().Name), 
                                "Error",
                                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                            return;
                        }
                    }

                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ReportConverter.ReportToConfig(ref _config, hr);
                    }));

                    if (configPacketNumber < 16)
                    {
                        _requestedNumber = ++configPacketNumber;
                        buffer[0] = _requestedNumber;
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

                    // Error reported
                    if (configPacketNumber == 0xFE)
                    {
                        MessageBoxService mbs = new MessageBoxService();
                        Version ver = Assembly.GetEntryAssembly().GetName().Version;

                        mbs.ShowMessage("Device firmware is not compatible with this version of the Configurator",
                            "Error",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }

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

            _requestedNumber = 1;
            buffer[0] = _requestedNumber;

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
