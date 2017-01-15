using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using SurroundGameWPF.ViewModel;

namespace SurroundGameWPF.View
{
    [ValueConversion(typeof(Persistence.Players), typeof(SolidColorBrush))]
    class PlayerToFieldColorConverter : IValueConverter
    {

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)value;
            return Persistence.Players.None;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Persistence.Players player = (Persistence.Players)value;
            return PlayerColors.GetColor(Persistence.PlayerTileData.GetPlayer(player).Field.State,180);
        }
    }
}
