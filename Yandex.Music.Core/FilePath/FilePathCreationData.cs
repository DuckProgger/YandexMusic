using Yandex.Api.Music.Mobile.Entities;

namespace Yandex.Music.Core.FilePath;

public class FilePathCreationData
{
    public StartDownloadInfo StartDownloadInfo { get; set; }

    public MobileTrackDownloadData MobileTrackDownloadData { get; set; }
}
