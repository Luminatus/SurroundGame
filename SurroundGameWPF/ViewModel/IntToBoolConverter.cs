using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SurroundGameWPF.ViewModel
{
    [ValueConversion(typeof(bool), typeof(int))]
    class IntToBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool inBool = (bool)value;
            return inBool ? 90 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value == 90 ? true : false;
        }
    }
}
