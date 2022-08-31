using Yandex.Api.Music.Web.Entities;

namespace Yandex.Music.Core.EntityHandlers;

internal class NewPlaylistsEntityHandler : EntityHandler
{
    private readonly WebNewPlaylistsPageData newPlaylists;

    public NewPlaylistsEntityHandler(IWebMusicEntity entity, CoreService service) : base(entity, service) {
        newPlaylists = (WebNewPlaylistsPageData)entity;
    }

    public override async Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new();

        WebPlaylist[] playlists = await Service.MusicWebApi.GetPlaylistsAsync(
            newPlaylists.NewPlaylists.Select(x => x.ToString()),
            Service.WebAuthData,
            cancellationToken).ConfigureAwait(false);
        ribbon.AddRange(playlists);

        return ribbon;
    }
}
