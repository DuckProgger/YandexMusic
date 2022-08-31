using Yandex.Api.Music.Web.Entities;

namespace Yandex.Music.Core.EntityHandlers;

internal class AlbumPromotionEntityHandler : EntityHandler
{
    private readonly WebAlbumPromotion albumPromotion;

    public AlbumPromotionEntityHandler(IWebMusicEntity entity, CoreService service) : base(entity, service) {
        albumPromotion = (WebAlbumPromotion)entity;
    }

    public override Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new();
        if (albumPromotion.Albums != null) {
            ribbon.AddRange(albumPromotion.Albums);
        }
        return Task.FromResult(ribbon);
    }
}
