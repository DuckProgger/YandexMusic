using System;
using System.Globalization;
using System.Windows.Data;
using Yandex.Music.Core;

namespace Yandex.Music.Views.Converters;
internal class StatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        var status = (DownloadEntityHandlerStatus)value;
        return status switch {
            DownloadEntityHandlerStatus.Pending => "Ожидание",
            DownloadEntityHandlerStatus.Downloading => "Загрузка...",
            DownloadEntityHandlerStatus.Error => "Ошибка",
            DownloadEntityHandlerStatus.Finished => "Готово",
            DownloadEntityHandlerStatus.Starting => "Подготовка",
            DownloadEntityHandlerStatus.Stopped => "Остановлено",
            DownloadEntityHandlerStatus.GetTrackInfo => "Получение данных...",
            DownloadEntityHandlerStatus.GetDirectUrl => "Получение прямой ссылки...",
            DownloadEntityHandlerStatus.Tagging => "Тэгирование...",
            DownloadEntityHandlerStatus.Stopping => "Остановка...",
            DownloadEntityHandlerStatus.ResultFileExists => "Файл уже существует",
            _ => status.ToString(),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
