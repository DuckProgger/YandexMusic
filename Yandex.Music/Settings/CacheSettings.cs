using Yandex.Music.Core.Cache;

namespace Yandex.Music.Settings;

public class CacheSettings
{
    public CacheProviderType ProviderType { get; set; } = CacheProviderType.Sqlite;

    public FileSystemCacheProviderSettings FileSystem { get; set; } = new();

    public SqliteCacheProviderSettings Sqlite { get; set; } = new();
}
