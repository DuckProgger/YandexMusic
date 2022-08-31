namespace Yandex.Music.Core.Cache;

public interface ICleanableCacheProvider : ICacheProvider
{
    Task CleanAsync();
}
