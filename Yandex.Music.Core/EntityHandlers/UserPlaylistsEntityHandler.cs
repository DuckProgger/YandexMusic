using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class UserPlaylistsEntityHandler : EntityHandler
{
    private readonly WebUserPlaylists userPlaylists;

    public UserPlaylistsEntityHandler(IWebMusicEntity entity, CoreService service) : base(entity, service) {
        userPlaylists = (WebUserPlaylists)entity;
    }

    public override Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new();

        if (userPlaylists.Playlists.Length > 0) {
            ribbon.Add(new Caption {
                Title = "Плейлисты",
            });
            ribbon.AddRange(userPlaylists.Playlists);
        }
        if (userPlaylists.Bookmarks.Length > 0) {
            ribbon.Add(new Caption {
                Title = "Также вам понравились эти плейлисты",
            });
            ribbon.AddRange(userPlaylists.Bookmarks);
        }

        return Task.FromResult(ribbon);
    }
}
