using Microsoft.Extensions.Logging;
using Yandex.Api;
using Yandex.Api.Logging;
using Yandex.Music.Core.Cache;

namespace Yandex.Music.Core;

public class CoreServiceDataProvider : YandexHttpClientDataProvider
{
    private readonly ILogger logger = LoggerService.Create<CoreServiceDataProvider>();

    public ICacheProvider CacheProvider { get; set; }

    public override async Task<string> GetStringAsync(RequestData requestData, CancellationToken cancellationToken) {
        string content;

        if (requestData.SupportCache && CacheProvider != null) {
            content = await CacheProvider.GetStringAsync(requestData, cancellationToken).ConfigureAwait(false);
            if (content != null) {
                return content;
            }
        }

        content = await base.GetStringAsync(requestData, cancellationToken).ConfigureAwait(false);

        if (requestData.SupportCache && CacheProvider != null) {
            await CacheProvider.SetStringAsync(requestData, content, cancellationToken).ConfigureAwait(false);
        }

        return content;
    }

    public override async Task<byte[]> GetBytesAsync(RequestData requestData, CancellationToken cancellationToken) {
        byte[] content;
        if (requestData.SupportCache && CacheProvider != null) {
            content = await CacheProvider.GetBytesAsync(requestData, cancellationToken).ConfigureAwait(false);
            if (content != null) {
                return content;
            }
        }

        content = await base.GetBytesAsync(requestData, cancellationToken).ConfigureAwait(false);

        if (requestData.SupportCache && CacheProvider != null) {
            await CacheProvider.SetBytesAsync(requestData, content, cancellationToken).ConfigureAwait(false);
        }

        return content;
    }
}
