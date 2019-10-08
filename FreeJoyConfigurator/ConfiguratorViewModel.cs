using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using HidLibrary;
using MessageBoxServicing;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace FreeJoyConfigurator
{



    public class ConfiguratorViewModel : INotifyPropertyChanged
    {
        
        private Configurator configurator;
        private DispatcherTimer timer;

        private UiCommand GetConfigCommand;

        private DeviceConfig receivedConfig;
        private byte configPacketNumber = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        #region VM Properties

        public string ActivityLogVM { get; private set; }
        public string ConnectionStatusVM
        {
            get
            {
                return string.Format("{0}", configurator.IsConnected ? "Connected" : "Disconnected");
            }
        }
        public bool IsConnectedVM
        {
            get
            {
                return configurator.IsConnected;
            }
        }

        public ObservableCollection<ButtonVM> ButtonCollectonVM { get; set; }
        public ObservableCollection<AxisVM> AxisCollectionVM { get; set; }

        #endregion

        #region Commands

        public ICommand ReadConfigButton_Click
        {
            get
            {
                if (GetConfigCommand == null)
                {
                    GetConfigCommand = new UiCommand((obj) => this.GetConfigRequest(obj));
                }
                return GetConfigCommand;
            }
        }

        #endregion

        #region Constructor

        public ConfiguratorViewModel()
        {
            configurator = new Configurator();
            configurator.DeviceAdded += DeviceAddedEventHandler;
            configurator.DeviceRemoved += DeviceRemovedEventHandler;
            configurator.PacketReceived += PacketReceivedEventHandler;
            configurator.PacketSent += PacketSentEventHandler;

            receivedConfig = new DeviceConfig();

            ButtonCollectonVM = new ObservableCollection<ButtonVM>();
            for (int i=0; i<128; i++)
                ButtonCollectonVM.Add(new ButtonVM(false, ButtonType.ButtonNormal));

            AxisCollectionVM = new ObservableCollection<AxisVM>();
            AxisCollectionVM.Add(new AxisVM(new AxisConfig()));
            AxisCollectionVM.Add(new AxisVM(new AxisConfig()));
            AxisCollectionVM.Add(new AxisVM(new AxisConfig()));
            AxisCollectionVM.Add(new AxisVM(new AxisConfig()));
            AxisCollectionVM.Add(new AxisVM(new AxisConfig()));
            AxisCollectionVM.Add(new AxisVM(new AxisConfig()));

            WriteLog("Program started", true);

            //Configure and start timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += TimerTickHandler;
            timer.Start();
        }

        #endregion

        #region HidEvents

        public void DeviceAddedEventHandler(object sender, HidDevice device)
        {
            WriteLog("Device added", false);
            if (PropertyChanged != null)
            {
                OnPropertyChanged("ConnectionStatusVM");
            }
        }

        public void DeviceRemovedEventHandler(object sender, HidDevice device)
        {
            WriteLog("Device removed", false);
            if (PropertyChanged != null)
            {
                OnPropertyChanged("ConnectionStatusVM");
            }

        }
        public void PacketReceivedEventHandler(object sender, HidReport report)
        {
            MessageBoxService mbs = new MessageBoxService();
            HidReport hr = report;
            

            switch ((ReportID)hr.ReportId)
            {
                case ReportID.JOY_REPORT:
                    int i = 0;
                    JoyReport joyReport = new JoyReport(hr);

                    foreach (var item in ButtonCollectonVM)
                    {
                        item.State = (bool)joyReport.Joystick.Buttons[i++];
                    }

                    i = 0;
                    foreach (var item in AxisCollectionVM)
                    {
                        item.Value = (double)joyReport.Joystick.Axis[i++];
                        item.OnPropertyChanged("Value");
                    }
                    break;

                case ReportID.CONFIG_REPORT:

                    configPacketNumber = hr.Data[0];
                    ConfigReport configReport = new ConfigReport(ref receivedConfig, hr);
                    if (configPacketNumber < 10)
                    {
                        configurator.GetConfigSend(++configPacketNumber);
                    }
                    break;

                default:
                    break;
            }
            
        }

        public void PacketSentEventHandler(object sender, HidReport report)
        {
            HidReport hr = report;

            WriteLog(string.Format("Report sent: {0}, Data = {1}", (ReportID)hr.ReportId,
                                    string.Join(", ", Array.ConvertAll(hr.Data, x => "0x" + x.ToString("X2")))), false);
        }

        #endregion

        #region Hid Requests

        void GetConfigRequest (object parameter)
        {
            if (configurator.HidDevice.IsConnected)
            {
                WriteLog("Getting configuration..", false);

                configPacketNumber = 1;
                configurator.GetConfigSend(configPacketNumber);

                // TODO: timeout
            }
        }

        #endregion


        #region Local functions

        // Add a line to the activity log text box
        private void WriteLog(string message, bool clear)
        {
            // Replace content
            if (clear)
            {
                ActivityLogVM = string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), message);
            }
            // Add new line
            else
            {
                ActivityLogVM += Environment.NewLine + string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), message);
            }
            if (PropertyChanged != null)
            {
                OnPropertyChanged("ActivityLogVM");
            }
        }

        // Update window with received data
        private void TimerTickHandler(object sender, EventArgs e)
        {
            // update View
            OnPropertyChanged("IsConnectedVM");
            OnPropertyChanged("ButtonCollectonVM");
            OnPropertyChanged("AxisCollectionVM");
        }



        #endregion

    }
}
