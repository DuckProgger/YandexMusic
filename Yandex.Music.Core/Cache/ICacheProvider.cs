using Yandex.Api;

namespace Yandex.Music.Core.Cache;

public interface ICacheProvider
{
    public Task<string> GetStringAsync(RequestData requestData, CancellationToken cancellationToken);

    public Task<byte[]> GetBytesAsync(RequestData requestData, CancellationToken cancellationToken);

    public Task SetStringAsync(RequestData requestData, string content, CancellationToken cancellationToken);

    public Task SetBytesAsync(RequestData requestData, byte[] content, CancellationToken cancellationToken);
}
