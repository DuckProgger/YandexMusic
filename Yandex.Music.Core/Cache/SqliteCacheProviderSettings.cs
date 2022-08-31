namespace Yandex.Music.Core.Cache;

public class SqliteCacheProviderSettings
{
    public string DatabasePath { get; set; } = "cache.db3";

    public bool UseStringDataCompression { get; set; } = true;

    public bool DeleteCacheAfterSession { get; set; } = true;
}
