namespace Yandex.Music.Core;

public class CoreServiceSettings
{
    public int AuthCacheLifeTimeDays { get; set; } = 1;

    public bool UseAuthCache { get; set; } = true;

    public int CoverSizePx { get; set; } = 700;

    public bool LoadEntityCover { get; set; } = true;

    public int MaxDownloadsThreadsCount { get; set; } = 5;

    public int MaxDownloadCoversThreadsCount { get; set; } = 10;

    public AudioCodec PreferredAudioCodec { get; set; } = AudioCodec.AAC;

    public AudioBitrate PreferredAudioBitrate { get; set; } = AudioBitrate.B320;

    public bool SetAudioTags { get; set; } = true;

    public bool HideFolderCover { get; set; } = true;

    public string DownloadTempDirectoryPath { get; set; } = "temp";

    public string DownloadResultDirectoryPath { get; set; } = "downloads";

    public int DownloadSpeedUpdateTime { get; set; } = 1000;

    public int DownloadSpeedStatisticTime { get; set; } = 3000;

    public string DownloadTrackFilePath { get; set; } = "$artist_name() - $track_title()";

    public string DownloadAlbumTrackFilePath { get; set; } = "$artist_name()/[$album_year()] $album_title()/CD$album_volume_number()/$track_number_in_album_volume(). $track_title()";

    public string DownloadPlaylistTrackFilePath { get; set; } = "Плейлисты/$playlist_title()/$track_number(). $artist_name() - $track_title()";
}
