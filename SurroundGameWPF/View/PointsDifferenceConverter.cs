using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SurroundGameWPF.View
{
    [ValueConversion(typeof(int), typeof(string))]
    class PointDifferenceConverter : IValueConverter
    {

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string inString = (string)value;
            return inString == "" ? 0 : Int32.Parse(inString);
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int intVal = (int)value;
            return intVal == 0 ? "" : intVal > 0 ? "+" + intVal.ToString() : intVal.ToString();
        }
    }
}
