using System.Globalization;
using Avalonia.Data.Converters;

namespace Pixelizer.Converters;

public class TimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int minutes)
        {
            var hours = minutes / 60;
            if (hours == 0)
                return $"{minutes} min";

            var remainingMinutes = minutes % 60;
            return $"{hours}:{remainingMinutes} h";
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}