using Yandex.Api.Music.Web.Entities;

namespace Yandex.Music.Core;

public class StartDownloadInfo
{
    public WebTrack Track { get; set; }

    public IWebMusicEntity ParentEntity { get; set; }


    public int Number { get; set; }

    public int TotalCount { get; set; }


    public int? AlbumVolumeNumber { get; set; }

    public int? AlbumVolumeTotalCount { get; set; }

    public int? AlbumVolumeTrackNumber { get; set; }

    public int? AlbumVolumeTrackTotalCount { get; set; }
}
