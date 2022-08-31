using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class CaptionEntityHandler : EntityHandler
{
    private readonly Caption caption;

    public CaptionEntityHandler(IWebMusicEntity entity, CoreService coreService) : base(entity, coreService) {
        caption = entity as Caption;
    }


    public override string GetUrl() {
        return caption.Query;
    }

    public override bool IsCaption => true;

    public override List<Caption> Titles => new() { 
        new Caption { 
            Title = caption.Title, 
            Query = Query
        }
    };
}
