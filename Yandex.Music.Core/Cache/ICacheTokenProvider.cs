using Yandex.Api;

namespace Yandex.Music.Core.Cache;

public interface ICacheTokenProvider
{
    string GetToken(RequestData requestData);
}
