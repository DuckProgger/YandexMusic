using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class PromotionEntityHandler : EntityHandler
{
    private readonly WebPromotion promotion;

    public override EntityType EntityType => EntityType.Promotion;

    public PromotionEntityHandler(IWebMusicEntity entity, CoreService service) : base(entity, service) {
        promotion = (WebPromotion)entity;
    }

    public override string CoverUri => promotion.Image;

    public override string GetUrl() {
        return promotion.Url;
    }

    public override List<Caption> Titles => new List<Caption> {
        new Caption {
            Title = promotion.Title,
            Query = promotion.Url,
        }
    };

    public override List<Caption> SecondTitles => new List<Caption> {
        new Caption {
            Title = promotion.Heading,
            Query = promotion.Url,
        }
    };

    public override string Description => promotion.Subtitle;
}
