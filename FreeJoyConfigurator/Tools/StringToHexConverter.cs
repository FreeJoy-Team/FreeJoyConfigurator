using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FreeJoyConfigurator
{
    public class StringToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ushort original = (ushort)value;
            string ret = original.ToString("X4");

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            

            string original = (string)value;
            ushort ret = System.Convert.ToUInt16(original, 16);
            return ret;
        }
    }
}
