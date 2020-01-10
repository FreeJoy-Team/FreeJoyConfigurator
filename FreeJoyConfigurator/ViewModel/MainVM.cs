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
        private DeviceConfigExchangerVM _configExchanger;

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
        public AxesToButtonsVM AxesToButtonsVM { get; private set; }
        public FirmwareUpdaterVM FirmwareUpdaterVM { get; }

        public string HidName { get; private set; }

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
        public DelegateCommand UpdateDeviceList { get; }
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

            
            _config = new DeviceConfig();
            _configExchanger = new DeviceConfigExchangerVM();

            _configExchanger.Received += ConfigReceived;
            _configExchanger.Sent += ConfigSent;

            PinsVM = new PinsVM(Config);
            PinsVM.ConfigChanged += PinConfigChanged;

            _joystick = new Joystick(Config);
            AxesVM = new AxesVM(_joystick, Config);
            ButtonsVM = new ButtonsVM(_joystick, Config);
            AxesToButtonsVM = new AxesToButtonsVM(_joystick, Config);
            AxesToButtonsVM.ConfigChanged += AxesToButtonsVM_ConfigChanged;

            FirmwareUpdaterVM = new FirmwareUpdaterVM();

            GetDeviceConfig = new DelegateCommand(() =>
            {
                _configExchanger.GetConfigRequest();
                WriteLog("Requesting config..", false);
            });
            SendDeviceConfig = new DelegateCommand(() =>
            {
                _configExchanger.SendConfig(Config);
                WriteLog("Writting config..", false);
            });

            UpdateDeviceList = new DelegateCommand(() => GetHidDevices());
            ResetAllPins = new DelegateCommand(() => PinsVM.ResetPins());
            SaveConfig = new DelegateCommand(() => SaveConfigToFile());
            LoadConfig = new DelegateCommand(() => ReadConfigFromFile());
            SetDefault = new DelegateCommand(() => LoadDefaultConfig());

            GetHidDevices();
            LoadDefaultConfig();

            WriteLog("Program started", true);
        }

        

        private void GetHidDevices()
        {
            var devices = Hid.GetDevices();

            if (devices.Count > 0)
            {
                byte[] tmp = new byte[20];
                devices[0].ReadProduct(out tmp);
                HidName = Encoding.Unicode.GetString(tmp).TrimEnd('\0');
            }
            else
            {
                HidName = " ";
            }
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
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 10; j++) tmp.AxisConfig[i].CurveShape.RemoveAt(0);
                    }
                    for (int i = 0; i < 128; i++) tmp.ButtonConfig.RemoveAt(0);
                    for (int i = 0; i < 8; i++) tmp.AxisToButtonsConfig.RemoveAt(0);

                    Config = tmp;
                }
                PinsVM.Config = Config;
                AxesVM.Config = Config;
                ButtonsVM.Config = Config;

                PinsVM.Update(Config);
                ButtonsVM.Update(Config);
                AxesVM.Update(Config);
                AxesToButtonsVM.Update(Config);
            }

        }

        private void LoadDefaultConfig()
        {
            {   // TODO: fix serialization
                var xmlStr = Properties.Resources.default_config;
                

                DeviceConfig tmp = Config;
                tmp = DeSerializeObject<DeviceConfig>(xmlStr, xmlStr.Length);
                for (int i = 0; i < 30; i++) tmp.PinConfig.RemoveAt(0);
                for (int i = 0; i < 8; i++) tmp.AxisConfig.RemoveAt(0);
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 10; j++) tmp.AxisConfig[i].CurveShape.RemoveAt(0);
                }
                for (int i = 0; i < 128; i++) tmp.ButtonConfig.RemoveAt(0);
                for (int i = 0; i < 8; i++) tmp.AxisToButtonsConfig.RemoveAt(0);

                Config = tmp;


            }
            PinsVM.Config = Config;
            AxesVM.Config = Config;
            ButtonsVM.Config = Config;

            PinsVM.Update(Config);
            ButtonsVM.Update(Config);
            AxesVM.Update(Config);
            AxesToButtonsVM.Update(Config);
        }

        private void PinConfigChanged()
        {
            ButtonsVM.Update(Config);
            AxesVM.Update(Config);
            AxesToButtonsVM.Update(Config);
        }

        private void AxesToButtonsVM_ConfigChanged()
        {
            ButtonsVM.Update(Config);
            AxesVM.Update(Config);
            //AxesToButtonsVM.Update(Config);
        }

        private void ConfigSent(DeviceConfig deviceConfig)
        {
            WriteLog("Config written", false);
        }

        private void ConfigReceived(DeviceConfig deviceConfig)
        {
            Config = deviceConfig;

            PinsVM.Update(Config);
            ButtonsVM.Update(Config);
            AxesVM.Update(Config);
            AxesToButtonsVM.Update(Config);

            WriteLog("Config received", false);
            RaisePropertyChanged(nameof(Config));
        }


        #region HidEvents
        public void DeviceAddedEventHandler(HidDevice hd)
        {
            GetHidDevices();

            WriteLog("Device added", false);
            RaisePropertyChanged(nameof(ConnectionStatusVM));
            RaisePropertyChanged(nameof(IsConnectedVM));
            RaisePropertyChanged(nameof(HidName));
        }

        public void DeviceRemovedEventHandler(HidDevice hd)
        {
            GetHidDevices();

            WriteLog("Device removed", false);
            RaisePropertyChanged(nameof(ConnectionStatusVM));
            RaisePropertyChanged(nameof(IsConnectedVM));
            RaisePropertyChanged(nameof(HidName));
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

        public T DeSerializeObject<T>(string xmlString, int lenght)
        {
            T objectOut = default(T);

            try
            {
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

