using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class BlockPageEntityHandler : EntityHandler
{
    private readonly WebBlockPage blockPage;

    public BlockPageEntityHandler(IWebMusicEntity entity, CoreService coreService) : base(entity, coreService) {
        blockPage = (WebBlockPage)entity;
    }

    public override Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new();
        foreach (WebPageBlock block in blockPage.Blocks) {
            ribbon.Add(new Caption {
                Title = block.Title,
            });
            ribbon.AddRange(block.Entities);
        }
        return Task.FromResult(ribbon);
    }
}
