using System;
using System.Globalization;
using System.Windows.Data;

namespace Yandex.Music.Views.Converters;
internal class DurationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value == null) {
            return null;
        }
        TimeSpan duration = (TimeSpan)value;
        return duration.Hours > 0
            ? duration.ToString("hh\\:mm\\:ss")
            : duration.ToString("mm\\:ss");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
