using Yandex.Api;
using Yandex.Api.Music.Queries;
using Yandex.Api.Music.Web;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class TrackEntityHandler : EntityHandler
{
    private readonly WebTrack track;
    private readonly WebTrackData trackData;
    private readonly ExtendedWebTrack extendedTrack;
    private bool liked;
    private bool disliked;

    public TrackEntityHandler(IWebMusicEntity entity, CoreService coreService) : base(entity, coreService) {
        if (entity is WebTrackData trackData) {
            this.trackData = trackData;
            track = trackData.Track;
        }
        else if (entity is ExtendedWebTrack extendedTrack) {
            this.extendedTrack = extendedTrack;
            track = extendedTrack.Track;
        }
        else {
            track = (WebTrack)entity;
        }
    }


    public override string GetUrl() {
        return Service.MusicWebApi.Settings.MainUrl + MusicTrackQuery.ByEntity(track);
    }

    public override string CoverUri => extendedTrack != null && extendedTrack.IsAlbumTrack
        ? null
        : track.CoverUri ?? track.OgImage;

    public override EntityType EntityType => EntityType.Track;

    public override List<Caption> Titles => new() {
        new Caption {
            Title = track.Title,
            Query = Service.MusicWebApi.Settings.MainUrl + MusicTrackQuery.ByEntity(track)
        }
    };

    public override List<Caption> SecondTitles {
        get {
            if (extendedTrack != null && extendedTrack.IsAlbumTrackWithSingleArtist) {
                return null;
            }
            List<Caption> artistsQueries = new(track.Artists.Length);
            foreach (WebArtist artist in track.Artists) {
                Caption artistQuery = new() {
                    Title = artist.Name,
                };
                if (!artist.IsCompilation) {
                    artistQuery.Query = Service.MusicWebApi.Settings.MainUrl + MusicArtistQuery.ByEntity(artist);
                }
                artistsQueries.Add(artistQuery);
            }
            return artistsQueries;
        }
    }

    public override List<Caption> ThirdTitles {
        get {
            if (extendedTrack != null && extendedTrack.IsAlbumTrack) {
                return null;
            }
            List<Caption> items = track.Albums
                .Select(album => new Caption() {
                    Title = album.Title,
                    Query = Service.MusicWebApi.Settings.MainUrl + MusicAlbumQuery.ByEntity(album),
                })
                .ToList();
            return items;
        }
    }

    public override TimeSpan? Duration => track.DurationMs == 0 ? null : new TimeSpan(0, 0, 0, 0, track.DurationMs);

    public override bool SupportDownload => true;

    public override bool SupportPlay => true;

    public override bool SupportLike => true;

    public override bool SupportDislike => true;

    public override bool Liked => liked;

    public override bool Disliked => disliked;

    public override bool IsBest => track.Best;

    public override int? OrderNumber => extendedTrack?.OrderNumber;


    public override Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new() {
                track
            };

        if (track.Artists?.Length > 0) {
            ribbon.Add(new Caption { Title = "Исполнители" });
            ribbon.AddRange(track.Artists);
        }

        if (track.Albums?.Length > 0) {
            ribbon.Add(new Caption { Title = "Альбомы" });
            ribbon.AddRange(track.Albums);
        }

        if (trackData.OtherVersions?.Count > 0) {
            ribbon.Add(new Caption { Title = "Другие версии" });
            foreach (KeyValuePair<string, List<WebTrack>> otherVersion in trackData.OtherVersions) {
                //ribbon.Add(new Caption { Title = otherVersion.Key });
                ribbon.AddRange(otherVersion.Value);
            }
        }

        if (trackData.SimilarTracks?.Count > 0) {
            ribbon.Add(new Caption { Title = "Похожие треки" });
            ribbon.AddRange(trackData.SimilarTracks);
        }

        if (trackData.AlsoInAlbums?.Count > 0) {
            ribbon.Add(new Caption { Title = "Также в альбомах" });
            ribbon.AddRange(trackData.AlsoInAlbums);
        }

        return Task.FromResult(ribbon);
    }

    public override async Task SetLikeStateAsync(LikeState state, CancellationToken cancellationToken) {
        LikeState currentState = Service.UserLibrary.GetTrackState(track.Id);
        if (currentState == state) {
            return;
        }

        WebStatus status;
        switch (state) {

            case LikeState.Like:
                status = await Service.MusicWebApi.LikeTrackAsync(
                    track, true, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                Validate.IsTrue(status.Success, () => new Exception("Не удалось установить лайк треку."));
                break;

            case LikeState.None:
                if (currentState == LikeState.Like) {
                    status = await Service.MusicWebApi.LikeTrackAsync(
                        track, false, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                    Validate.IsTrue(status.Success, () => new Exception("Не удалось снять лайк треку."));
                }
                else {
                    status = await Service.MusicWebApi.DislikeTrackAsync(
                        track, false, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                    Validate.IsTrue(status.Success, () => new Exception("Не удалось снять дизлайк треку."));
                }
                break;

            case LikeState.Dislike:
                status = await Service.MusicWebApi.DislikeTrackAsync(
                   track, true, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                Validate.IsTrue(status.Success, () => new Exception("Не удалось установить дизлайк треку."));
                break;
        }

        Service.UserLibrary.SetTrackState(track.Id, state);
        UpdateLikeStateFromLibrary();
    }

    public override void UpdateLikeStateFromLibrary() {
        LikeState state = Service.UserLibrary.GetTrackState(track.Id);
        liked = state == LikeState.Like;
        disliked = state == LikeState.Dislike;
    }

    public override Task<List<StartDownloadInfo>> GetStartDownloadInfoAsync(CancellationToken cancellationToken) {
        return Task.FromResult(new List<StartDownloadInfo>() { new StartDownloadInfo {
            Track = track,
            Number = 1,
            TotalCount = 1,
        } });
    }
}
