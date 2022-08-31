using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Yandex.Music.Core.Cache;
using Yandex.Music.Infrastructure;
using Yandex.Music.Modules;
using Yandex.Music.Views;

namespace Yandex.Music;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void RegisterTypes(IContainerRegistry containerRegistry) {
        containerRegistry
            .RegisterSingleton<DialogService>()
            .AddCoreService()
            ;
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) {
        moduleCatalog
            .AddModule(typeof(MainModule))
            .AddModule(typeof(DialogModule))
            ;
    }

    protected override void OnExit(ExitEventArgs e) {
        CleanCache();
    }

    protected override Window CreateShell() {
        return Container.Resolve<MainWindow>();
    }

    private static void CleanCache() {
        ICacheProvider cacheProvider = ServiceLocator.GetService<ICacheProvider>();
        if (cacheProvider is ICleanableCacheProvider cleanableCacheProvider) 
            cleanableCacheProvider.CleanAsync();
    }
}
