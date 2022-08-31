using System;
using System.Globalization;
using System.Windows.Data;
using Yandex.Music.Core;

namespace Yandex.Music.Views.Converters;
internal class BitrateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        var bitrate = (AudioBitrate)value;
        return bitrate switch {
            AudioBitrate.B128 => "128 КБ/сек",
            AudioBitrate.B192 => "192 КБ/сек",
            AudioBitrate.B320 => "320 КБ/сек",
            _ => bitrate.ToString(),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        var bitrate = (string)value;
        return bitrate switch {
            "128 КБ/сек" => AudioBitrate.B128,
            "192 КБ/сек" => AudioBitrate.B192,
            "320 КБ/сек" => AudioBitrate.B320,
            _ => throw new ArgumentException("Несуществующий битрейт", nameof(bitrate)),
        };
    }
}
