using Yandex.Api.Music.Queries;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class SearchEntityHandler : EntityHandler
{
    private readonly WebSearchResult searchResult;

    public SearchEntityHandler(IWebMusicEntity entity, CoreService coreService) : base(entity, coreService) {
        searchResult = (WebSearchResult)entity;
    }

    public override Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new();

        MusicSearchQuery searchQuery = MusicSearchQuery.ByQuery(Query);

        List<IWebSearchResultItems> resultItems = new() {
            searchResult.Albums,
            searchResult.Tracks,
            searchResult.Artists,
            //searchResult.Videos,
            searchResult.Playlists,
            //searchResult.Users
        };
        resultItems = resultItems.OrderBy(x => x.Order).ToList();

        foreach (IWebSearchResultItems resultItem in resultItems) {
            IWebMusicEntity[] resultItemsArray = resultItem.GetItems();
            if (resultItemsArray?.Length > 0) {
                IWebMusicEntity item = resultItemsArray.First();

                if (resultItem == searchResult.Albums) {
                    ribbon.Add(new Caption {
                        Title = $"Альбомы (всего: {resultItem.Total})",
                        Query = Service.MusicWebApi.Settings.MainUrl + new MusicSearchQuery() {
                            Text = searchQuery.Text,
                            Type = MusicSearchQuery.Types.Albums
                        },
                    });
                }

                if (resultItem == searchResult.Tracks) {
                    ribbon.Add(new Caption {
                        Title = $"Треки (всего: {resultItem.Total})",
                        Query = Service.MusicWebApi.Settings.MainUrl + new MusicSearchQuery {
                            Text = searchQuery.Text,
                            Type = MusicSearchQuery.Types.Tracks
                        },
                    });
                }

                if (resultItem == searchResult.Artists) {
                    ribbon.Add(new Caption {
                        Title = $"Исполнители (всего: {resultItem.Total})",
                        Query = Service.MusicWebApi.Settings.MainUrl + new MusicSearchQuery() {
                            Text = searchQuery.Text,
                            Type = MusicSearchQuery.Types.Artists
                        },
                    });
                }

                if (resultItem == searchResult.Playlists) {
                    ribbon.Add(new Caption {
                        Title = $"Плейлисты (всего: {resultItem.Total})",
                        Query = Service.MusicWebApi.Settings.MainUrl + new MusicSearchQuery() {
                            Text = searchQuery.Text,
                            Type = MusicSearchQuery.Types.Playlists
                        },
                    });
                }

                ribbon.AddRange(resultItemsArray);

                if (searchResult.Pager != null) {
                    int pagesCount = searchResult.Pager.Total / searchResult.Pager.PerPage;
                    if (searchResult.Pager.Page < pagesCount) {
                        ribbon.Add(new Caption {
                            Title = $"Страница {searchResult.Pager.Page + 2} >>",
                            Query = Service.MusicWebApi.Settings.MainUrl + MusicSearchQuery.ByQuery(Query).NextPage(),
                        });
                    }
                }
            }
        }

        return Task.FromResult(ribbon);
    }
}
