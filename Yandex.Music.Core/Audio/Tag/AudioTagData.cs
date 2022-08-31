namespace Yandex.Music.Core.Audio.Tag;

public class AudioTagData
{
    /// <summary>
    /// № трека.
    /// </summary>
    public uint? TrackNumber { get; set; }

    /// <summary>
    /// Количество треков. 
    /// </summary>
    public uint? TrackCount { get; set; }

    /// <summary>
    /// № диска.
    /// </summary>
    public uint? DiskNumber { get; set; }

    /// <summary>
    /// Количество дисков.
    /// </summary>
    public uint? DiskCount { get; set; }

    /// <summary>
    /// Заголовок.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Исполнитель.
    /// </summary>
    public string[] Artists { get; set; }

    /// <summary>
    /// Альбом.
    /// </summary>
    public string Album { get; set; }

    /// <summary>
    /// Жанры.
    /// </summary>
    public string[] Genres { get; set; }

    /// <summary>
    /// Год.
    /// </summary>
    public uint? Year { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Ссылка.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Исполнитель альбома.
    /// </summary>
    public string[] AlbumArtists { get; set; }

    /// <summary>
    /// Авторские права.
    /// </summary>
    public string Copyright { get; set; }

    /// <summary>
    /// Издатель.
    /// </summary>
    public string Publisher { get; set; }

    /// <summary>
    /// Композитор.
    /// </summary>
    public string[] Composers { get; set; }

    /// <summary>
    /// Дирижёр.
    /// </summary>
    public string Conductor { get; set; }

    /// <summary>
    /// Кодировщик.
    /// </summary>
    public string Encoder { get; set; }

    /// <summary>
    /// Настроение.
    /// </summary>
    public string Mood { get; set; }

    /// <summary>
    /// Каталог.
    /// </summary>
    public string Catalog { get; set; }

    /// <summary>
    /// ISRC.
    /// </summary>
    public string ISRC { get; set; }

    /// <summary>
    /// BPM.
    /// </summary>
    public uint? BeatsPerMinute { get; set; }

    /// <summary>
    /// Оценка.
    /// </summary>
    public uint? Rate { get; set; }

    /// <summary>
    /// Track Gain (dB).
    /// </summary>
    public double? TrackGain { get; set; }

    /// <summary>
    /// Album Gain (dB).
    /// </summary>
    public double? AlbumGain { get; set; }

    /// <summary>
    /// Обложка.
    /// </summary>
    public byte[] Picture { get; set; }

    /// <summary>
    /// Текст песни.
    /// </summary>
    public string Lyrics { get; set; }

    /// <summary>
    /// Автор текста песни.
    /// </summary>
    public string LyricsAuthor { get; set; }

    /// <summary>
    /// Часть компиляции.
    /// </summary>
    public bool? CompilationPart { get; set; }
}
