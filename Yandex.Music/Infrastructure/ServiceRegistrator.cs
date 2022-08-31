using Prism.Ioc;
using System;
using Yandex.Api;
using Yandex.Music.Core;
using Yandex.Music.Core.Cache;
using Yandex.Music.Settings;

namespace Yandex.Music.Infrastructure;
internal static class ServiceRegistrator
{
    public static IContainerRegistry AddCoreService(this IContainerRegistry containerRegistry) {
        RootSettings settings = ConfigService.GetSettings();

        // Реализация провайдера кэша
        switch (settings.Cache.ProviderType) {
            case CacheProviderType.FileSystem:
                containerRegistry.RegisterSingleton<ICacheProvider>(() => new FileSystemCacheProvider() {
                    Settings = settings.Cache.FileSystem,
                });
                break;

            case CacheProviderType.Sqlite:
                containerRegistry.RegisterSingleton<ICacheProvider>(() => new SqliteCacheProvider(settings.Cache.Sqlite));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(settings.Cache.ProviderType), $"Отсутствует реализация провайдера кэша {settings.Cache.ProviderType}");
        }

        // Регистрация IDataProvider
        containerRegistry.RegisterSingleton<IDataProvider>(() => new CoreServiceDataProvider() {
            CacheProvider = ServiceLocator.GetService<ICacheProvider>(),
        });

        // Регистрация CoreService
        containerRegistry.RegisterSingleton<CoreService>(() => {
            IDataProvider dataProvider = ServiceLocator.GetService<IDataProvider>();
            CoreService coreService = new(dataProvider) {
                Settings = settings.CoreService,
                MusicWebApi = { Settings = settings.MusicWebApi },
                MusicMobileApi = { Settings = settings.MusicMobileApi },
                PassportApi = { Settings = settings.PassportApi }
            };
            coreService.ApplySettings();
            return coreService;
        });

        return containerRegistry;
    }
}
