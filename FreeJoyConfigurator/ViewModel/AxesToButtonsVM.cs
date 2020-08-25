﻿using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class AxesToButtonsVM : BindableBase
    {
        public delegate void AxesToButtonChangedEvent();
        public event AxesToButtonChangedEvent ConfigChanged;

        private ObservableCollection<AxisToButtons> axesToButtons;
        private Joystick _joystick;
        private DeviceConfig _config;
        public DeviceConfig Config
        {
            get { return _config; }
            set { SetProperty(ref _config, value); }
        }

        public ObservableCollection<Axis> Axes { get; private set; }
        public ObservableCollection<AxisToButtons> AxesToButtons
        {
            get
            {
                return axesToButtons;
            }
            set
            {
                //for (int i = 0; i < AxesToButtons.Count; i++)
                //{
                //    axesToButtons[i].IsEnabled = AxesToButtons[i].IsEnabled;
                //    axesToButtons[i].ButtonCnt = AxesToButtons[i].ButtonCnt;

                //    for (int j = 0; j < AxesToButtons[i].RangeItems.Count; j++)
                //    {
                //        axesToButtons[i].RangeItems[j] = AxesToButtons[i].RangeItems[j];
                //    }
                //}
                //RaisePropertyChanged(nameof(AxesToButtons));

                SetProperty(ref axesToButtons, value);
            }
        }

        public AxesToButtonsVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            _joystick = joystick;
            _config = deviceConfig;
            Axes = new ObservableCollection<Axis>(_joystick.Axes);

            axesToButtons = new ObservableCollection<AxisToButtons>();

            for (int i = 0; i < Config.AxisToButtonsConfig.Count; i++)
            {
                axesToButtons.Add(new AxisToButtons());
            }

            for (int i = 0; i < Config.AxisToButtonsConfig.Count; i++)
            {
                AxesToButtons[i].PropertyChanged += AxesToButtonsVM_PropertyChanged;
                for (int k = 0; k < AxesToButtons[i].RangeItems.Count; k++)
                {
                    AxesToButtons[i].RangeItems[k].PropertyChanged += AxesToButtonsVM_Range_PropertyChanged;
                }
            }
        }

        private void AxesToButtonsVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            for (int i = 0; i < AxesToButtons.Count; i++)
            {
                //// disable range changed notification
                for (int k = 0; k < axesToButtons[i].RangeItems.Count; k++)
                {
                    AxesToButtons[i].RangeItems[k].PropertyChanged -= AxesToButtonsVM_Range_PropertyChanged;
                }

                while (AxesToButtons[i].ButtonCnt > AxesToButtons[i].RangeItems.Count)
                {
                    for (int j = 0; j < AxesToButtons[i].RangeItems.Count; j++)
                    {
                        AxesToButtons[i].RangeItems[j].From = (int)(j * (255.0 / (AxesToButtons[i].RangeItems.Count + 1)));
                        AxesToButtons[i].RangeItems[j].To = (int)((j + 1) * (255.0 / (AxesToButtons[i].RangeItems.Count + 1)));
                    }
                    AxesToButtons[i].RangeItems.Add(new RangeItem { From = AxesToButtons[i].RangeItems.Last().To, To = 255 });
                }
                while (AxesToButtons[i].ButtonCnt < AxesToButtons[i].RangeItems.Count)
                {
                    for (int j = AxesToButtons[i].RangeItems.Count - 2; j >= 0; j--)
                    {
                        AxesToButtons[i].RangeItems[j].From = (int)(j * (255.0 / (AxesToButtons[i].RangeItems.Count - 1)));
                        AxesToButtons[i].RangeItems[j].To = (int)((j + 1) * (255.0 / (AxesToButtons[i].RangeItems.Count - 1)));
                    }
                    AxesToButtons[i].RangeItems.Remove(AxesToButtons[i].RangeItems.Last());
                    AxesToButtons[i].RangeItems[AxesToButtons[i].RangeItems.Count - 1].To = 255;
                }


                //// enable range changed notification
                for (int k = 0; k < axesToButtons[i].RangeItems.Count; k++)
                {
                    AxesToButtons[i].RangeItems[k].PropertyChanged += AxesToButtonsVM_Range_PropertyChanged;
                }
            }

            AxesToButtonsVM_Range_PropertyChanged(null, null);
        }

        private void AxesToButtonsVM_Range_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DeviceConfig conf = Config;

            for (int i = 0; i < conf.AxisToButtonsConfig.Count; i++)
            {
                conf.AxisToButtonsConfig[i].ButtonsCnt = (byte)AxesToButtons[i].ButtonCnt;

                for (int j = 0; j < AxesToButtons[i].ButtonCnt; j++)
                {
                    conf.AxisToButtonsConfig[i].Points[j] = (byte)AxesToButtons[i].RangeItems[j].From;
                }
                conf.AxisToButtonsConfig[i].Points[AxesToButtons[i].ButtonCnt] =
                    (byte)AxesToButtons[i].RangeItems.Last().To;
            }
            Config = conf;

            ConfigChanged();
        }

        public void Update(DeviceConfig config)
        {
            Config = config;

            // disabling events
            for (int i = 0; i < Config.AxisToButtonsConfig.Count; i++)
            {
                AxesToButtons[i].PropertyChanged -= AxesToButtonsVM_PropertyChanged;
                for (int k = 0; k < AxesToButtons[i].RangeItems.Count; k++)
                {
                    AxesToButtons[i].RangeItems[k].PropertyChanged -= AxesToButtonsVM_Range_PropertyChanged;
                }
            }

            for (int i = 0; i < Config.AxisToButtonsConfig.Count; i++)
            {

                if (Config.AxisToButtonsConfig[i].IsEnabled)
                {
                    AxesToButtons[i].IsEnabled = true;
                }
                else
                {
                    AxesToButtons[i].IsEnabled = false;
                    AxesToButtons[i].ButtonCnt = 2;
                }
                // change button count
                AxesToButtons[i].ButtonCnt = Config.AxisToButtonsConfig[i].ButtonsCnt;

                // change range count
                while (AxesToButtons[i].ButtonCnt > AxesToButtons[i].RangeItems.Count)
                {
                    for (int j = 0; j < AxesToButtons[i].RangeItems.Count; j++)
                    {
                        AxesToButtons[i].RangeItems[j].From = (int)(j * (255.0 / (AxesToButtons[i].RangeItems.Count + 1)));
                        AxesToButtons[i].RangeItems[j].To = (int)((j + 1) * (255.0 / (AxesToButtons[i].RangeItems.Count + 1)));
                    }
                    AxesToButtons[i].RangeItems.Add(new RangeItem { From = AxesToButtons[i].RangeItems.Last().To, To = 255 });
                }
                while (AxesToButtons[i].ButtonCnt < AxesToButtons[i].RangeItems.Count && AxesToButtons[i].ButtonCnt > 0)
                {
                    for (int j = AxesToButtons[i].RangeItems.Count - 2; j >= 0; j--)
                    {
                        AxesToButtons[i].RangeItems[j].From = (int)(j * (255.0 / (AxesToButtons[i].RangeItems.Count - 1)));
                        AxesToButtons[i].RangeItems[j].To = (int)((j + 1) * (255.0 / (AxesToButtons[i].RangeItems.Count - 1)));
                    }
                    AxesToButtons[i].RangeItems[AxesToButtons[i].RangeItems.Count - 1].To = 255;
                    AxesToButtons[i].RangeItems.Remove(AxesToButtons[i].RangeItems.Last());
                }

                // change ranges
                for (int j = 0; j < AxesToButtons[i].RangeItems.Count; j++)
                {
                    AxesToButtons[i].RangeItems[j].From = Config.AxisToButtonsConfig[i].Points[j];
                    AxesToButtons[i].RangeItems[j].To = Config.AxisToButtonsConfig[i].Points[j + 1];
                }


            }

            // enabling events
            for (int i = 0; i < Config.AxisToButtonsConfig.Count; i++)
            {
                AxesToButtons[i].PropertyChanged += AxesToButtonsVM_PropertyChanged;
                for (int k = 0; k < AxesToButtons[i].RangeItems.Count; k++)
                {
                    AxesToButtons[i].RangeItems[k].PropertyChanged += AxesToButtonsVM_Range_PropertyChanged;
                }
            }

            RaisePropertyChanged(nameof(AxesToButtons));
        }
    }
}
