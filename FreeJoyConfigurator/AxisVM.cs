using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class AxisVM : INotifyPropertyChanged
    {
        public AxisConfig Config { get; set; }
        public double Value { get; set; }
        public double RawValue { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public AxisVM(AxisConfig config)
        {
            this.Config = config;
            this.Value = 0;
            this.RawValue = 0;
        }
    }
}
