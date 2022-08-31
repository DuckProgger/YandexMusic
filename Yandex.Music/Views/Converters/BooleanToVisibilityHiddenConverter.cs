using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Yandex.Music.Views.Converters;
internal class BooleanToVisibilityHiddenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        bool bValue = false;
        if (value is bool b) {
            bValue = b;
        }
        else if (value is bool?) {
            bool? tmp = (bool?)value;
            bValue = tmp.HasValue ? tmp.Value : false;
        }
        return (bValue) ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is Visibility visibility) {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
