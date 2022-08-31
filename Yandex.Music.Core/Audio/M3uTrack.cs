using System.Text;

namespace Yandex.Music.Core.Audio;

public class M3uTrack
{
    /// <summary>
    /// Длина трека (в секундах).
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// Заголовок трека.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Путь к треку (локальный путь или URL).
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Создание строки с данными формата M3U.
    /// </summary>
    public static string ToString(IEnumerable<M3uTrack> items) {
        StringBuilder str = new();
        str.AppendLine("#EXTM3U");
        str.AppendLine();
        foreach (M3uTrack item in items) {
            str.AppendLine(item.ToString());
            str.AppendLine();
        }
        return str.ToString();
    }

    public override string ToString() {
        StringBuilder str = new();
        str.Append("#EXTINF:");
        str.Append(Duration.HasValue ? Duration.Value : -1);
        str.Append(",");
        str.AppendLine(Title);
        str.Append(Path);
        return str.ToString();
    }
}
