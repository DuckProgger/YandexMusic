using Microsoft.Extensions.Logging;
using Yandex.Api.Logging;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.EntityHandlers;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core;

public class EntityHandlerProvider : IEntityHandlerProvider
{
    private readonly ILogger logger = LoggerService.Create<UnknownEntityHandler>();

    private readonly Dictionary<Type, Func<IWebMusicEntity, EntityHandler>> entityHandlers = new();
    private readonly CoreService coreService;

    public EntityHandlerProvider(CoreService coreService) {
        this.coreService = coreService;

        AddEntityHandler(typeof(WebArtist), entity => new ArtistEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebArtistData), entity => new ArtistEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebTrack), entity => new TrackEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebTrackData), entity => new TrackEntityHandler(entity, coreService));
        AddEntityHandler(typeof(ExtendedWebTrack), entity => new TrackEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebAlbum), entity => new AlbumEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebPlaylist), entity => new PlaylistEntityHandler(entity, coreService));
        AddEntityHandler(typeof(Caption), entity => new CaptionEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebSearchResult), entity => new SearchEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebBlockPage), entity => new BlockPageEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebPromotion), entity => new PromotionEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebLink), entity => new LinkEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebTag), entity => new TagEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebNewPlaylistsPageData), entity => new NewPlaylistsEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebChartPage), entity => new ChartPageEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebAlbumPromotion), entity => new AlbumPromotionEntityHandler(entity, coreService));
        AddEntityHandler(typeof(WebUserPlaylists), entity => new UserPlaylistsEntityHandler(entity, coreService));
    }


    public bool AddEntityHandler(Type handlerType, Func<IWebMusicEntity, EntityHandler> handlerCreator) {
        if (entityHandlers.ContainsKey(handlerType)) {
            entityHandlers.Remove(handlerType);
        }
        return entityHandlers.TryAdd(handlerType, handlerCreator);
    }

    public EntityHandler GetEntityHandler(IWebMusicEntity entity) {
        Func<IWebMusicEntity, EntityHandler> handlerCreator = entityHandlers.GetValueOrDefault(entity.GetType());
        if (handlerCreator != null) {
            return handlerCreator(entity);
        }
        else {
            logger.LogWarning($"Отсутствует обработчик для сущности \"{entity.GetType().FullName}\".");
            return new UnknownEntityHandler(entity, coreService);
        }
    }
}
