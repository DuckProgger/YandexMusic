using Yandex.Api.Music.Web.Entities;

namespace Yandex.Music.Core.MusicEntities;

public class ExtendedWebTrack : IWebMusicEntity
{
    public ExtendedWebTrack(WebTrack track) {
        Track = track;
    }

    public WebTrack Track { get; set; }

    public int? OrderNumber { get; set; }

    public bool IsAlbumTrack { get; set; }

    public bool IsAlbumTrackWithSingleArtist { get; set; }
}
