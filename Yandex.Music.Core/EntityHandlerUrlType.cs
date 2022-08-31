namespace Yandex.Music.Core;

public enum EntityHandlerUrlType
{
    /// <summary>
    /// Исполнитель -> Главное.
    /// </summary>
    Artist,
    /// <summary>
    /// Исполнитель -> Треки.
    /// </summary>
    ArtistTracks,
    /// <summary>
    /// Исполнитель -> Альбомы.
    /// </summary>
    ArtistAlbums,
    /// <summary>
    /// Исполнитель -> Похожие исполнители.
    /// </summary>
    ArtistSimilar,
}
