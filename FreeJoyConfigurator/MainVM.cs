using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using HidLibrary;
using MessageBoxServicing;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class MainVM : BindableBase
    {
        public JoystickVM JoystickVM { get; private set; }

        public string ActivityLogVM { get; private set; }
        public string ConnectionStatusVM
        {
            get
            {
                return string.Format("{0}", hid.IsConnected ? "Connected" : "Disconnected");
            }
        }
        public bool IsConnectedVM
        {
            get
            {
                return hid.IsConnected;
            }
        }

        private Hid hid;
        //private UiCommand GetConfigCommand;

        //#region Commands
        //public ICommand ReadConfigButton_Click
        //{
        //    get
        //    {
        //        if (GetConfigCommand == null)
        //        {
        //            GetConfigCommand = new UiCommand((obj) => this.GetConfigRequest(obj));
        //        }
        //        return GetConfigCommand;
        //    }
        //}
        //#endregion


        public MainVM()
        {
            hid = new Hid();
            hid.DeviceAdded += DeviceAddedEventHandler;
            hid.DeviceRemoved += DeviceRemovedEventHandler;
            hid.PacketReceived += PacketReceivedEventHandler;
            hid.PacketSent += PacketSentEventHandler;

            JoystickVM = new JoystickVM(new Joystick());
            

            WriteLog("Program started", true);
        }

        private static void Watch<T, T2>(ReadOnlyObservableCollection<T> collToWatch, ObservableCollection<T2> collToUpdate,
                Func<T2, object> modelProperty)
        {
            ((INotifyCollectionChanged)collToWatch).CollectionChanged += (s, a) =>
            {
                if (a.NewItems?.Count == 1) collToUpdate.Add((T2)Activator.CreateInstance(typeof(T2), (T)a.NewItems[0], null));
                if (a.OldItems?.Count == 1) collToUpdate.Remove(collToUpdate.First(mv => modelProperty(mv) == a.NewItems[0]));
            };
        }

        #region HidEvents
        public void DeviceAddedEventHandler(object sender, HidDevice hd)
        {
            WriteLog("Device added", false);
            RaisePropertyChanged(nameof(ConnectionStatusVM));
        }

        public void DeviceRemovedEventHandler(object sender, HidDevice hd)
        {
            WriteLog("Device removed", false);
            RaisePropertyChanged(nameof(ConnectionStatusVM));
        }
        public void PacketReceivedEventHandler(object sender, HidReport hr)
        {
            //MessageBoxService mbs = new MessageBoxService();
            //WriteLog("Report received", false);
        }

        public void PacketSentEventHandler(object sender, HidReport hr)
        {
            WriteLog("Report sent", false);
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
            RaisePropertyChanged("ActivityLogVM");
        }
        #endregion
    }
}

