using Yandex.Api;
using Yandex.Api.Music.Queries;
using Yandex.Api.Music.Web;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class ArtistEntityHandler : EntityHandler
{
    private readonly WebArtistData artistData;
    private readonly WebArtist artist;
    private bool liked;
    private bool disliked;

    public ArtistEntityHandler(IWebMusicEntity entity, CoreService coreService) : base(entity, coreService) {
        if (entity is WebArtistData artistData) {
            this.artistData = artistData;
            artist = artistData.Artist;
        }
        else {
            artist = entity as WebArtist;
        }
    }

    public override string GetUrl() {
        return artist.IsCompilation ? null : Service.MusicWebApi.Settings.MainUrl + MusicArtistQuery.ByEntity(artist);
    }

    public override string GetUrl(EntityHandlerUrlType urlType) {
        if (!artist.IsCompilation) {
            MusicArtistQuery query = MusicArtistQuery.ByEntity(artist);
            switch (urlType) {

                case EntityHandlerUrlType.Artist:
                    return Service.MusicWebApi.Settings.MainUrl + query;

                case EntityHandlerUrlType.ArtistTracks:
                    query.What = MusicArtistQuery.Whats.Tracks;
                    return Service.MusicWebApi.Settings.MainUrl + query;

                case EntityHandlerUrlType.ArtistAlbums:
                    query.What = MusicArtistQuery.Whats.Albums;
                    return Service.MusicWebApi.Settings.MainUrl + query;

                case EntityHandlerUrlType.ArtistSimilar:
                    query.What = MusicArtistQuery.Whats.Similar;
                    return Service.MusicWebApi.Settings.MainUrl + query;
            }
        }
        return base.GetUrl(urlType);
    }

    public override bool SupportUrlType(EntityHandlerUrlType urlType) {
        if (!artist.IsCompilation) {
            switch (urlType) {
                case EntityHandlerUrlType.Artist:
                case EntityHandlerUrlType.ArtistTracks:
                case EntityHandlerUrlType.ArtistAlbums:
                case EntityHandlerUrlType.ArtistSimilar:
                    return true;
            }
        }
        return base.SupportUrlType(urlType);
    }

    public override string CoverUri => artist.Cover?.Uri ?? artist.OgImage;

    public override EntityType EntityType => EntityType.Artist;

    public override List<Caption> Titles => new() {
        new Caption {
            Title = artist.Name,
            Query = Service.MusicWebApi.Settings.MainUrl + MusicArtistQuery.ByEntity(artist),
        }
    };

    public override string Description => artist.Description?.Text;

    public override bool SupportLike => true;

    public override bool SupportDislike => true;

    public override bool Liked => liked;

    public override bool Disliked => disliked;

    public override async Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new() {
            artist,
        };

        MusicArtistQuery artistQuery = MusicArtistQuery.ByQuery(Query);
        switch (artistQuery.What) {

            // Вкладка "Треки"
            case MusicArtistQuery.Whats.Tracks:

                ribbon.Add(new Caption {
                    Title = "Треки",
                });
                // Для вывода всех треков, иначе можно использовать artistData.Tracks
                WebTrack[] tracks = await Service.MusicWebApi.GetTracksAsync(
                   artistData.TrackIds.Select(trackId => MusicTrackQuery.ById(trackId)), Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                ribbon.AddRange(tracks);

                break;


            // Вкладка "Альбомы"
            case MusicArtistQuery.Whats.Albums:

                ribbon.Add(new Caption {
                    Title = "Альбомы",
                });
                ribbon.AddRange(artistData.Albums);

                if (artistData.AlsoAlbums?.Length > 0) {
                    ribbon.Add(new Caption {
                        Title = "Сборники"
                    });
                    ribbon.AddRange(artistData.AlsoAlbums);
                }

                break;


            // Вкладка "Похожие исполнители"
            case MusicArtistQuery.Whats.Similar:

                ribbon.Add(new Caption {
                    Title = "Похожие исполнители",
                });
                ribbon.AddRange(artistData.AllSimilarArtists);

                break;


            // Стандартная лента
            default:

                if (artistData.LastRelease != null) {
                    ribbon.Add(new Caption {
                        Title = "Недавний релиз",
                        Query = Service.EntityHandlerProvider.GetEntityHandler(artistData.LastRelease)?.GetUrl(),
                    });
                    ribbon.Add(artistData.LastRelease);
                }

                if (artistData.Tracks?.Length > 0) {
                    ribbon.Add(new Caption {
                        Title = "Популярные треки",
                        Query = Service.MusicWebApi.Settings.MainUrl + new MusicArtistQuery {
                            Id = artist.Id,
                            What = MusicArtistQuery.Whats.Tracks
                        },
                    });
                    ribbon.AddRange(artistData.Tracks);
                }

                if (artistData.Albums?.Length > 0) {
                    ribbon.Add(new Caption {
                        Title = "Популярные альбомы",
                        Query = Service.MusicWebApi.Settings.MainUrl + new MusicArtistQuery {
                            Id = artist.Id,
                            What = MusicArtistQuery.Whats.Albums
                        },
                    });
                    ribbon.AddRange(artistData.Albums);
                }

                if (artistData.Playlists.Length > 0) {
                    ribbon.Add(new Caption {
                        Title = "Плейлисты"
                    });
                    ribbon.AddRange(artistData.Playlists);
                }

                if (artistData.AlsoAlbums?.Length > 0) {
                    ribbon.Add(new Caption {
                        Title = "Сборники"
                    });
                    ribbon.AddRange(artistData.AlsoAlbums);
                }

                if (artistData.SimilarArtists.Length > 0) {
                    ribbon.Add(new Caption {
                        Title = "Похожие исполнители",
                        Query = Service.MusicWebApi.Settings.MainUrl + new MusicArtistQuery() {
                            Id = artist.Id,
                            What = MusicArtistQuery.Whats.Similar
                        },
                    });
                    ribbon.AddRange(artistData.SimilarArtists);
                }

                break;
        }

        return ribbon;
    }

    public override async Task SetLikeStateAsync(LikeState state, CancellationToken cancellationToken) {
        LikeState currentState = Service.UserLibrary.GetArtistState(artist.Id);
        if (currentState == state) {
            return;
        }

        WebStatus status;
        switch (state) {

            case LikeState.Like:
                status = await Service.MusicWebApi.LikeArtistAsync(
                    artist, true, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                Validate.IsTrue(status.Success, () => new Exception("Не удалось установить лайк исполнителю."));
                break;

            case LikeState.None:
                if (currentState == LikeState.Like) {
                    status = await Service.MusicWebApi.LikeArtistAsync(
                        artist, false, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                    Validate.IsTrue(status.Success, () => new Exception("Не удалось снять лайк исполнителю."));
                }
                else {
                    status = await Service.MusicWebApi.DislikeArtistAsync(
                           artist, false, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                    Validate.IsTrue(status.Success, () => new Exception("Не удалось снять дизлайк исполнителю."));
                }
                break;

            case LikeState.Dislike:
                status = await Service.MusicWebApi.DislikeArtistAsync(
                   artist, true, Service.UserData, Service.WebAuthData, cancellationToken).ConfigureAwait(false);
                Validate.IsTrue(status.Success, () => new Exception("Не удалось установить дизлайк исполнителю."));
                break;
        }

        Service.UserLibrary.SetArtistState(artist.Id, state);
        UpdateLikeStateFromLibrary();
    }

    public override void UpdateLikeStateFromLibrary() {
        LikeState state = Service.UserLibrary.GetArtistState(artist.Id);
        liked = state == LikeState.Like;
        disliked = state == LikeState.Dislike;
    }
}
