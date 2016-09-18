// Copyrights 2016 Sameer Khandekar
// MIT License

using System;

using Windows.UI;
using Windows.UI.Xaml.Data;

namespace MaintenanceAlert.Converters
{
    /// <summary>
    /// Converter to convert percent of oil remaining to color
    /// </summary>
    public class PercentToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double percent = 1.0 - (double)value;
            if (percent <= 0.2)
            {
                return Colors.Red;
            }

            if (percent <= 0.5)
            {
                return Colors.Yellow;
            }

            return Colors.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
