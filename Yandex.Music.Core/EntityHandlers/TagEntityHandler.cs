using Yandex.Api.Music.Web.Entities;

namespace Yandex.Music.Core.EntityHandlers;

internal class TagEntityHandler : EntityHandler
{
    private readonly WebTag tag;

    public TagEntityHandler(IWebMusicEntity entity, CoreService service) : base(entity, service) {
        tag = (WebTag)entity;
    }

    public override Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new();
        ribbon.AddRange(tag.Playlists);
        return Task.FromResult(ribbon);
    }

    public override EntityType EntityType => EntityType.Tag;
}
