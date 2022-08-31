using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class LinkEntityHandler : EntityHandler
{
    private readonly WebLink link;

    public LinkEntityHandler(IWebMusicEntity entity, CoreService service) : base(entity, service) {
        link = (WebLink)entity;
    }

    public override string GetUrl() {
        return Service.MusicWebApi.Settings.MainUrl + link.Url;
    }

    public override string CoverUri =>
        link.BackgroundImageUri;

    public override EntityType EntityType => EntityType.Link;

    public override List<Caption> Titles => new() {
        new Caption{
            Title = link.Title,
            Query = GetUrl(),
        }
    };
}
