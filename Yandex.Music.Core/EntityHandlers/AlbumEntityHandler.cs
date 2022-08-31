using Yandex.Api;
using Yandex.Api.Music.Queries;
using Yandex.Api.Music.Web;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class AlbumEntityHandler : EntityHandler
{
    private readonly WebAlbum album;
    private bool liked;

    public AlbumEntityHandler(IWebMusicEntity entity, CoreService coreService) : base(entity, coreService) {
        album = entity as WebAlbum;
    }

    public override string GetUrl() {
        return Service.MusicWebApi.Settings.MainUrl + MusicAlbumQuery.ByEntity(album);
    }

    public override string CoverUri => album.CoverUri ?? album.OgImage;

    public override EntityType EntityType => EntityType.Album;

    public override List<Caption> Titles => new() {
        new Caption {
            Title = album.Title,
            Query = Service.MusicWebApi.Settings.MainUrl + MusicAlbumQuery.ByEntity(album),
        }
    };

    public override List<Caption> SecondTitles {
        get {
            List<Caption> artistsQueries = new(album.Artists.Length);
            foreach (WebArtist artist in album.Artists) {
                artistsQueries.Add(new() {
                    Title = artist.Name,
                    Query = artist.IsCompilation ? null : Service.MusicWebApi.Settings.MainUrl + MusicArtistQuery.ById(artist.Id),
                });
            }
            return artistsQueries;
        }
    }

    public override List<Caption> ThirdTitles => album.Year.HasValue
                ? (new() {
                    new Caption(){
                        Title = album.Year?.ToString(),
                    }
                })
                : base.ThirdTitles;

    public override string Description => album.Description;

    public override bool SupportDownload => true;

    public override TimeSpan? Duration {
        get {
            int durationMs = 0;
            if (album.Tracks != null) {
                foreach (WebTrack track in album.Tracks) {
                    durationMs += track.DurationMs;
                }
            }
            return durationMs == 0 ? null : new TimeSpan(0, 0, 0, 0, durationMs);
        }
    }

    public override bool SupportPlay => true;

    public override bool SupportLike => true;

    public override bool SupportDislike => false;

    public override bool Liked => liked;

    public override async Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new() {
            album,
        };

        bool singleArtist = album.Tracks
            .SelectMany(x => x.Artists)
            .Select(x => x.Id)
            .Distinct()
            .Count() == 1;

        if (album.Volumes?.Length > 1) {
            for (int albumIndex = 0; albumIndex < album.Volumes?.Length; albumIndex++) {
                ribbon.Add(new Caption { Title = $"Диск {albumIndex + 1}" });
                for (int trackIndex = 0; trackIndex < album.Volumes[albumIndex].Length; trackIndex++) {
                    ExtendedWebTrack extTrack = new(album.Volumes[albumIndex][trackIndex]) {
                        OrderNumber = trackIndex + 1,
                        IsAlbumTrack = true,
                        IsAlbumTrackWithSingleArtist = singleArtist,
                    };
                    ribbon.Add(extTrack);
                }
            }
        }
        else {
            ribbon.Add(new Caption { Title = $"Треки" });
            for (int trackIndex = 0; trackIndex < album.Tracks.Length; trackIndex++) {
                ExtendedWebTrack extTrack = new(album.Tracks[trackIndex]) {
                    OrderNumber = trackIndex + 1,
                    IsAlbumTrack = true,
                    IsAlbumTrackWithSingleArtist = singleArtist,
                };
                ribbon.Add(extTrack);
            }
        }

        if (album.Artists?.Length > 0) {
            ribbon.Add(new Caption { Title = "Исполнители" });
            ribbon.AddRange(album.Artists);
        }

        WebArtist artist = album.Artists.First();
        if (!artist.IsCompilation) {
            WebAlbum[] otherAlbums = await Service.MusicWebApi.GetOtherArtistAlbumsAsync(
                MusicArtistQuery.ByEntity(artist), Service.WebAuthData, cancellationToken).ConfigureAwait(false);
            if (otherAlbums.Length > 0) {
                ribbon.Add(new Caption {
                    Title = "Другие альбомы исполнителя",
                    Query = Service.MusicWebApi.Settings.MainUrl + new MusicArtistQuery() {
                        Id = artist.Id,
                        What = MusicArtistQuery.Whats.Albums
                    }
                });
                ribbon.AddRange(otherAlbums);
            }
        }

        return ribbon;
    }

    public override async Task SetLikeStateAsync(LikeState state, CancellationToken cancellationToken) {
        if (state == LikeState.Dislike) {
            await base.SetLikeStateAsync(state, cancellationToken);
            return;
        }

        LikeState currentState = Service.UserLibrary.GetAlbumState(album.Id);
        if (currentState == state) {
            return;
        }

        WebStatus status = await Service.MusicWebApi.LikeAlbumAsync(
            album, state == LikeState.Like, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
        Validate.IsTrue(status.Success, () => new Exception("Не удалось установить лайк альбому."));
        Service.UserLibrary.SetAlbumState(album.Id, state);
        UpdateLikeStateFromLibrary();
    }

    public override void UpdateLikeStateFromLibrary() {
        LikeState state = Service.UserLibrary.GetAlbumState(album.Id);
        liked = state == LikeState.Like;
    }

    public override async Task<List<StartDownloadInfo>> GetStartDownloadInfoAsync(CancellationToken cancellationToken) {
        List<StartDownloadInfo> downloadTrackDataList = new();
        WebAlbum album = await Service.MusicWebApi.GetAlbumAsync(Query, Service.WebAuthData, cancellationToken).ConfigureAwait(false);

        int trackIndex = 0;
        for (int volumeIndex = 0; volumeIndex < album.Volumes.Length; volumeIndex++) {
            WebTrack[] volume = album.Volumes[volumeIndex];
            for (int trackVolumeIndex = 0; trackVolumeIndex < volume.Length; trackVolumeIndex++) {
                WebTrack track = volume[trackVolumeIndex];

                downloadTrackDataList.Add(new StartDownloadInfo {
                    Track = track,
                    ParentEntity = album,

                    Number = trackIndex + 1,
                    TotalCount = album.Tracks.Length,

                    AlbumVolumeNumber = volumeIndex + 1,
                    AlbumVolumeTotalCount = album.Volumes.Length,

                    AlbumVolumeTrackNumber = trackVolumeIndex + 1,
                    AlbumVolumeTrackTotalCount = volume.Length,
                });

                trackIndex++;
            }
        }

        return downloadTrackDataList;
    }
}
