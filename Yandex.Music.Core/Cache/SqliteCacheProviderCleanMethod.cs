namespace Yandex.Music.Core.Cache;

public enum SqliteCacheProviderCleanMethod
{
    None,
    ByCreationDate,
    ByStringRowsCount,
    ByDataRowsCount,
    Vacuum,
}
