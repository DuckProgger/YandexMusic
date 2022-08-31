using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Yandex.Music.Core;

namespace Yandex.Music.Views.Converters;
internal class DownloadEntityNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        EntityHandler entity = (EntityHandler)value;
        if (entity is null)
            return null;
        StringBuilder sb = new();
        string artist = entity.SecondTitle;
        string track = entity.Title;
        sb.Append(artist);
        sb.Append(" - ");
        sb.Append(track);
        if (entity.SecondTitles.Count > 1) {
            sb.Append(" (feat. ");
            string featured = string.Join(", ", entity.SecondTitles.Skip(1).Select(l => l.Title));
            sb.Append(featured);
            sb.Append(')');
        }
        return sb.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
