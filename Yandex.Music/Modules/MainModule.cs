using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Yandex.Music.Infrastructure.Constants;
using Yandex.Music.Views;

namespace Yandex.Music.Modules;

/// <summary>
/// Представляет модуль для навигации по представлениям из главного экрана.
/// </summary>
internal class MainModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager
             .RegisterViewWithRegion(RegionNames.Main, typeof(RibbonView))
             .RegisterViewWithRegion(RegionNames.Main, typeof(DownloadsView))
             .RegisterViewWithRegion(RegionNames.Main, typeof(SettingsView))
        ;

    }
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<RibbonView>();
        containerRegistry.RegisterForNavigation<DownloadsView>();
        containerRegistry.RegisterForNavigation<SettingsView>();
    }
}
