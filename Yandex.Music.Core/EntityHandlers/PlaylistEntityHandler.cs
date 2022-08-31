using Yandex.Api;
using Yandex.Api.Music.Queries;
using Yandex.Api.Music.Web;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class PlaylistEntityHandler : EntityHandler
{
    private readonly WebPlaylist playlist;
    private bool liked;

    public PlaylistEntityHandler(IWebMusicEntity entity, CoreService coreService) : base(entity, coreService) {
        playlist = entity as WebPlaylist;
    }


    public override string GetUrl() {
        return Service.MusicWebApi.Settings.MainUrl + MusicPlaylistQuery.ByEntity(playlist);
    }

    public override string CoverUri => playlist.Title == "Мне нравится"
        ? Service.MusicWebApi.Settings.MainUrl + Service.MusicWebApi.Settings.LikePlaylistCoverPath
        : playlist.Cover?.Uri ?? playlist.OgImage;

    public override EntityType EntityType => EntityType.Playlist;

    public override List<Caption> Titles => new() { new Caption {
            Title = playlist.Title,
            Query = Service.MusicWebApi.Settings.MainUrl + MusicPlaylistQuery.ByEntity(playlist),
        }
    };

    public override string Description => playlist.Description;

    public override TimeSpan? Duration => playlist.Duration == 0 ? null : new TimeSpan(0, 0, 0, 0, playlist.Duration);

    public override bool SupportDownload => true;

    public override bool SupportPlay => true;

    public override bool SupportLike => true;

    public override bool Liked => liked;

    public override Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new();

        for (int i = 0; i < playlist.Tracks.Length; i++) {
            ExtendedWebTrack extTrack = new(playlist.Tracks[i]) {
                OrderNumber = i + 1
            };
            ribbon.Add(extTrack);
        }

        if (playlist.SimilarPlaylists?.Length > 0) {
            ribbon.Add(new Caption() { Title = "Похожие плейлисты" });
            ribbon.AddRange(playlist.SimilarPlaylists);
        }

        return Task.FromResult(ribbon);
    }

    public override async Task SetLikeStateAsync(LikeState state, CancellationToken cancellationToken) {
        if (state == LikeState.Dislike) {
            await base.SetLikeStateAsync(state, cancellationToken);
            return;
        }

        LikeState currentState = Service.UserLibrary.GetPlaylistState(playlist.Id);
        if (currentState == state) {
            return;
        }

        WebStatus status = await Service.MusicWebApi.LikePlaylistAsync(
            playlist, state == LikeState.Like, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
        Validate.IsTrue(status.Success, () => new Exception("Не удалось установить лайк плейлисту."));
        Service.UserLibrary.SetPlaylistState(playlist.Id, state);
        UpdateLikeStateFromLibrary();
    }

    public override void UpdateLikeStateFromLibrary() {
        LikeState state = Service.UserLibrary.GetPlaylistState(playlist.Id);
        liked = state == LikeState.Like;
    }

    public override async Task<List<StartDownloadInfo>> GetStartDownloadInfoAsync(CancellationToken cancellationToken) {
        List<StartDownloadInfo> downloadItems = new();

        WebPlaylist playlist = await Service.MusicWebApi.GetPlaylistAsync(Query, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
        for (int trackIndex = 0; trackIndex < playlist.Tracks.Length; trackIndex++) {
            WebTrack track = playlist.Tracks[trackIndex];
            StartDownloadInfo downloadItem = new() {
                Track = track,
                ParentEntity = playlist,
                Number = trackIndex + 1,
                TotalCount = playlist.Tracks.Length,
            };
            downloadItems.Add(downloadItem);
        }

        return downloadItems;
    }
}
