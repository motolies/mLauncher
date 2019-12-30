using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace mFileSearch
{
    [ValueConversion(typeof(double), typeof(double))]
    public class ControlWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(double))
                throw new InvalidOperationException("The target must be a double");

            if (parameter == null)
                parameter = 0;

            double val;
            double param;

            if (double.TryParse(value.ToString(), out val) && double.TryParse(parameter.ToString(), out param))
            {
                return val - param;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
