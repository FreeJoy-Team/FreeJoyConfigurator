using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FreeJoyConfigurator
{
    public class EncoderInputConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret;
            int original = (int)value;
            if (original >= 0)
            {
                ret = "Button №" + original.ToString();
            }
            else if (original == -1)
            {
                ret = "Not defined";
            }
            else if (original == -2)
            {
                ret = "Fast encoder pin";
            }
            else ret = "";

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
