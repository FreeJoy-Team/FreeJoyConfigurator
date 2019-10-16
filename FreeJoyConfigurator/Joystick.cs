using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HidLibrary;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class Joystick : BindableBase
    {
        private JoyReport joyReport;

        public ObservableCollection<Axis> Axes { get; private set; }
        public ObservableCollection<Button> Buttons { get; private set; }
        //public byte[] Povs { get; private set; }

        private Hid hid;

        public Joystick()
        {
            Axes = new ObservableCollection<Axis>();
            for (int i = 0; i < 8; i++)
            {
                Axes.Add(new Axis());
            }
            Buttons = new ObservableCollection<Button>();
            for (int i = 0; i < 128; i++)
            {
                Buttons.Add(new Button());
            }
            hid = new Hid();
            hid.DeviceAdded += DeviceAddedEventHandler;
            hid.DeviceRemoved += DeviceRemovedEventHandler;
            hid.PacketReceived += PacketReceivedEventHandler;
            hid.PacketSent += PacketSentEventHandler;
        }

        public void DeviceAddedEventHandler(object sender, HidDevice hd)
        {

        }

        public void DeviceRemovedEventHandler(object sender, HidDevice hd)
        {

        }

        public void PacketSentEventHandler(object sender, HidReport hr)
        {

        }

        public void PacketReceivedEventHandler(object sender, HidReport report)
        {
            HidReport hr = report;

            if ((ReportID)hr.ReportId == (ReportID.JOY_REPORT))
            {
                int i = 0;
                joyReport = new JoyReport(hr);

                foreach (var item in joyReport.Buttons)
                {
                    if (Buttons[i] != null)
                        Buttons[i++].State = item.State;
                }

                i = 0;
                foreach (var item in joyReport.Axes)
                {
                    if (Axes[i] != null)
                    {
                        Axes[i].Value = item.Value;
                        Axes[i++].RawValue = item.RawValue;
                    }                      
                }
            }          
        }

    }

    public class Button : BindableBase
    {
        private bool _state;
        public bool State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public Button()
        {
            _state = false;
        }

        public Button(bool state)
        {
            _state = state;
        }
    }

    public class Axis : BindableBase
    {
        private ushort _value;
        private ushort _rawValue;

        public ushort Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
        public ushort RawValue
        {
            get { return _rawValue; }
            set { SetProperty(ref _rawValue, value); }
        }

        public Axis()
        {
            _value = 0;
            _rawValue = 0;
        }

        public Axis (ushort value)
        {
            _value = value;
            _rawValue = 0;
        }

        public Axis(ushort value, ushort rawValue)
        {
            _value = value;
            _rawValue = rawValue;
        }
    }
}
