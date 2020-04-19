﻿using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class ShiftRegistersVM : BindableBase
    {
        private Joystick _joystick;
        private DeviceConfig _config;
        private ObservableCollection<ShiftRegister> _shiftRegisters;

        public delegate void ShiftRegistersChangedEvent();
        public event ShiftRegistersChangedEvent ConfigChanged;

        public DeviceConfig Config
        {
            get { return _config; }
            set { SetProperty(ref _config, value); }
        }

        public ObservableCollection<ShiftRegister> ShiftRegisters
        {
            get { return _shiftRegisters; }
            set { SetProperty(ref _shiftRegisters, value); }
        }

        public ShiftRegistersVM(Joystick joystick, DeviceConfig deviceConfig)
        {
            _joystick = joystick;
            _config = deviceConfig;

            _shiftRegisters = new ObservableCollection<ShiftRegister>();

            for (int i = 0; i < 4; i++)
            {
                ShiftRegisters.Add(new ShiftRegister(i + 1, ShiftRegisterType.HC165_PullUp));
                ShiftRegisters[i].PropertyChanged += ShiftRegistersVM_PropertyChanged;
            }

        }

        private void ShiftRegistersVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DeviceConfig tmp = Config;

            for (int i = 0; i < tmp.ShiftRegistersConfig.Count; i++)
            {
                tmp.ShiftRegistersConfig[i].Type = ShiftRegisters[i].Type;
                tmp.ShiftRegistersConfig[i].ButtonCnt = (byte)ShiftRegisters[i].ButtonCnt;
            }

            Config = tmp;
            ConfigChanged();
        }

        public void Update(DeviceConfig deviceConfig)
        {
            Config = deviceConfig;

            ObservableCollection<ShiftRegister> tmp = new ObservableCollection<ShiftRegister>();

            for (int i = 0; i < ShiftRegisters.Count; i++)
            {
                tmp.Add(new ShiftRegister(i + 1, Config.ShiftRegistersConfig[i].ButtonCnt, Config.ShiftRegistersConfig[i].Type));
            }

            ShiftRegisters = tmp;
            foreach (var item in ShiftRegisters) item.PropertyChanged += ShiftRegistersVM_PropertyChanged;

            // check pins and enable register
            int prevData = -1;
            int prevLatch = -1;
            for (int k = 0; k < Config.ShiftRegistersConfig.Count; k++)
            {

                for (int j = prevData + 1; j < Config.PinConfig.Count; j++)
                {
                    if (Config.PinConfig[j] == PinType.ShiftReg_DATA)
                    {
                        ShiftRegisters[k].DataPin = (ShiftRegSourceType)j;
                        prevData = j;
                        break;
                    }
                }
                for (int j = prevLatch + 1; j < Config.PinConfig.Count; j++)
                {
                    if (Config.PinConfig[j] == PinType.ShiftReg_LATCH)
                    {
                        ShiftRegisters[k].LatchPin = (ShiftRegSourceType)j;
                        prevLatch = j;
                        break;
                    }
                }
            }
            for (int k = 0; k < Config.ShiftRegistersConfig.Count; k++)
            {
                if (ShiftRegisters[k].LatchPin == ShiftRegSourceType.NotDefined && 
                    prevLatch >= 0)
                {
                    ShiftRegisters[k].LatchPin = (ShiftRegSourceType)prevLatch;
                }
            }
            for (int k = 0; k < Config.ShiftRegistersConfig.Count; k++)
            {
                if (ShiftRegisters[k].LatchPin != ShiftRegSourceType.NotDefined &&
                    ShiftRegisters[k].DataPin != ShiftRegSourceType.NotDefined)
                {
                    ShiftRegisters[k].IsEnabled = true;
                }
            }
        }
    }
}
