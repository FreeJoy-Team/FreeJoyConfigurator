using Prism.Mvvm;
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

        public ShiftRegistersVM(Joystick joystick, DeviceConfig config)
        {
            _joystick = joystick;
            _config = config;


            _shiftRegisters = new ObservableCollection<ShiftRegister>();
            _shiftRegisters.Add(new ShiftRegister("74HC165"));
            _shiftRegisters.Add(new ShiftRegister("CD4021"));

        }

        public void Update(Joystick joystick, DeviceConfig deviceConfig)
        {
            _joystick = joystick;
            Config = deviceConfig;
        }

    }
}
