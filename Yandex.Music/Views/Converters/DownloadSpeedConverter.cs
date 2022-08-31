using System;
using System.Globalization;
using System.Windows.Data;

namespace Yandex.Music.Views.Converters;
internal class DownloadSpeedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        double speedKbPerSec = (double)value / 1024.0;
        return speedKbPerSec switch {
            >= 1024.0 => $"{speedKbPerSec / 1024.0:0.00} МБ/сек",
            _ => $"{speedKbPerSec:0.00} КБ/сек"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
