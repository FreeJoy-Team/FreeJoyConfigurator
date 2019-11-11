using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using HidLibrary;
using MessageBoxServicing;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

namespace FreeJoyConfigurator
{
    public class MainVM : BindableBase
    { 
        private Joystick _joystick;
        private DeviceConfig _config;
        

        public DeviceConfig Config
        {
            get
            {
                return _config;
            }
            set
            {
                SetProperty(ref _config, value);
            }
        }
        public PinsVM PinsVM {get; set; }
        public AxesVM AxesVM { get; private set; }
        public ButtonsVM ButtonsVM { get; private set; }
        public FirmwareUpdaterVM FirmwareUpdaterVM { get; }

        public string ActivityLogVM { get; private set; }
        public string ConnectionStatusVM
        {
            get
            {
                return string.Format("{0}", Hid.IsConnected ? "Connected" : "Disconnected");
            }
        }
        public bool IsConnectedVM
        {
            get
            {
                return Hid.IsConnected;
            }
        }

        #region Commands
        public DelegateCommand GetDeviceConfig { get; }
        public DelegateCommand SendDeviceConfig { get; }
        public DelegateCommand ResetAllPins { get; }
        public DelegateCommand SaveConfig { get; }
        public DelegateCommand LoadConfig { get; }
        public DelegateCommand SetDefault { get; }
        
        #endregion


        public MainVM()
        {
            Hid.Connect();
            Hid.DeviceAdded += DeviceAddedEventHandler;
            Hid.DeviceRemoved += DeviceRemovedEventHandler;

            _joystick = new Joystick();
            _config = new DeviceConfig();
            

            Config.Received += ConfigReceived;
            Config.Sent += ConfigSent;

            PinsVM = new PinsVM(Config);
            PinsVM.ConfigChanged += PinConfigChanged;

            AxesVM = new AxesVM(_joystick, Config);
            ButtonsVM = new ButtonsVM(_joystick, Config);

            FirmwareUpdaterVM = new FirmwareUpdaterVM();

            GetDeviceConfig = new DelegateCommand(() =>
            {
                Config.GetConfigRequest();
                WriteLog("Requesting config..", false);
            });
            SendDeviceConfig = new DelegateCommand(() =>
            {
                Config.SendConfig();
                WriteLog("Writting config..", false);
            });

            ResetAllPins = new DelegateCommand(() => PinsVM.ResetPins());
            SaveConfig = new DelegateCommand(() => SaveConfigToFile());
            LoadConfig = new DelegateCommand(() => ReadConfigFromFile());
            SetDefault = new DelegateCommand(() => LoadDefaultConfig());
            

            WriteLog("Program started", true);
        }

        private void SaveConfigToFile()
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.FileName = "default";
            dlg.DefaultExt = ".conf";
            dlg.Filter = "Config files (.conf)|*.conf";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                if (File.Exists(dlg.FileName)) File.Delete(dlg.FileName);
                SerializeObject<DeviceConfig>( Config, dlg.FileName);
            }

        }

        private void ReadConfigFromFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".conf";
            dlg.Filter = "Config files (.conf)|*.conf";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                {   // TODO: fix serialization
                    DeviceConfig tmp = DeSerializeObject<DeviceConfig>(dlg.FileName);
                    for (int i = 0; i < 30; i++) tmp.PinConfig.RemoveAt(0);
                    for (int i = 0; i < 8; i++) tmp.AxisConfig.RemoveAt(0);
                    for (int i = 0; i < 128; i++) tmp.ButtonConfig.RemoveAt(0);
                    for (int i = 0; i < 12; i++) tmp.EncoderConfig.RemoveAt(0);

                    Config = tmp;  
                }
                PinsVM.Config = Config;
                AxesVM.Config = Config;
                ButtonsVM.Config = Config;

                PinsVM.Update();
                ButtonsVM.Update();
            }

        }

        private void LoadDefaultConfig()
        {
            {   // TODO: fix serialization
                DeviceConfig tmp = DeSerializeObject<DeviceConfig>("./default.conf");
                for (int i = 0; i < 30; i++) tmp.PinConfig.RemoveAt(0);
                for (int i = 0; i < 8; i++) tmp.AxisConfig.RemoveAt(0);
                for (int i = 0; i < 128; i++) tmp.ButtonConfig.RemoveAt(0);
                for (int i = 0; i < 12; i++) tmp.EncoderConfig.RemoveAt(0);

                Config = tmp;
            }
            PinsVM.Config = Config;
            AxesVM.Config = Config;
            ButtonsVM.Config = Config;

            PinsVM.Update();
            ButtonsVM.Update();
        }

        private void PinConfigChanged()
        {
            ButtonsVM.Update();
        }

        private void ConfigSent(DeviceConfig deviceConfig)
        {
            WriteLog("Config written", false);
        }

        private void ConfigReceived(DeviceConfig deviceConfig)
        {
            WriteLog("Config received", false);
            RaisePropertyChanged(nameof(Config));
        }


        #region HidEvents
        public void DeviceAddedEventHandler(HidDevice hd)
        {
            WriteLog("Device added", false);
            RaisePropertyChanged(nameof(ConnectionStatusVM));
            RaisePropertyChanged(nameof(IsConnectedVM));
        }

        public void DeviceRemovedEventHandler(HidDevice hd)
        {
            WriteLog("Device removed", false);
            RaisePropertyChanged(nameof(ConnectionStatusVM));
            RaisePropertyChanged(nameof(IsConnectedVM));
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

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }

            return objectOut;
        }
        #endregion
    }

}

