using System;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Api;
using Yandex.Music.Core.Cache;

namespace Yandex.Tests.Internal;

public class TestDataProvider : YandexHttpClientDataProvider
{
    public ICacheProvider CacheProvider { get; set; } = new FileSystemCacheProvider();

    public override async Task<string> GetStringAsync(RequestData requestData, CancellationToken cancellationToken) {
        string content;

        if (requestData.SupportCache && TestFactory.Configuration.LoadFromCache) {
            content = await CacheProvider.GetStringAsync(requestData, cancellationToken);
            if (content != null) {
                return content;
            }
        }

        if (TestFactory.Configuration.OfflineMode) {
            throw new Exception("Установлен автономный режим.");
        }

        content = await base.GetStringAsync(requestData, cancellationToken).ConfigureAwait(false);
        if (requestData.SupportCache && TestFactory.Configuration.UpdateCache && !YandexApiUtils.IsCaptcha(content)) {
            await CacheProvider.SetStringAsync(requestData, content, cancellationToken);
        }
        return content;
    }
}
