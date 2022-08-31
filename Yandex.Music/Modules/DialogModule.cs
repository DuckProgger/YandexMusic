using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Yandex.Music.Infrastructure.Constants;
using Yandex.Music.Views;

namespace Yandex.Music.Modules;

internal class DialogModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        IRegionManager regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager
            .RegisterViewWithRegion(RegionNames.Dialog, typeof(AuthorizationView))
            ;
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<AuthorizationView>();

    }
}
