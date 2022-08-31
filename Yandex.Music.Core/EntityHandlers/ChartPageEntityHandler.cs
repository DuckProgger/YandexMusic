using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.MusicEntities;

namespace Yandex.Music.Core.EntityHandlers;

internal class ChartPageEntityHandler : EntityHandler
{
    private readonly WebChartPage chartPage;

    public ChartPageEntityHandler(IWebMusicEntity entity, CoreService service) : base(entity, service) {
        chartPage = (WebChartPage)entity;
    }

    public override Task<List<IWebMusicEntity>> GetRibbonAsync(CancellationToken cancellationToken) {
        List<IWebMusicEntity> ribbon = new() {
            new Caption {
                Title = "Чарт",
            }
        };
        for (int i = 0; i < chartPage.ChartPositions.Length; i++) {
            ribbon.Add(new ExtendedWebTrack(chartPage.ChartPositions[i].Track) {
                OrderNumber = i + 1
            });
        }

        ribbon.Add(new Caption {
            Title = "Плейлисты с другими чартами",
        });
        ribbon.AddRange(chartPage.OtherChartPlaylists);

        return Task.FromResult(ribbon);
    }
}
