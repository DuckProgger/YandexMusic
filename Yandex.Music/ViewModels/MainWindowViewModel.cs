using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Windows.Input;
using Yandex.Api.Logging;
using Yandex.Music.Core;
using Yandex.Music.Events;
using Yandex.Music.Infrastructure;
using Yandex.Music.Infrastructure.Constants;
using Yandex.Music.Settings;

namespace Yandex.Music.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger logger = LoggerService.Create<MainWindowViewModel>();

    private readonly CoreService coreService;
    private readonly IRegionManager regionManager;

    public bool SignInButtonVisibility => !coreService.Authorized;
    public bool SignOutButtonVisibility => coreService.Authorized;
    public string UserName => ConfigService.GetSettings().Auth?.Login;
    public bool UserNameVisibility => coreService.Authorized;
    public bool ProgressBarVisibility { get; set; }
    public bool IsIndeterminateProgressBar { get; set; }
    public double ProgressBarValue { get; set; }

    public MainWindowViewModel(CoreService coreService, IRegionManager regionManager, IEventAggregator eventAggregator) : base(eventAggregator) {
        this.coreService = coreService;
        this.regionManager = regionManager;
        eventAggregator.GetEvent<AuthorizeEvent>().Subscribe(a => UpdateProperies());
        eventAggregator.GetEvent<ProgressBarEvent>().Subscribe(ManageProgressBar);
        Notifier.SetMessageParams(MessageType.Warning, new MessageParameters(false));
    }

    private void ManageProgressBar(ProgressBarData data) {
        ProgressBarVisibility = data.Visibility;
        ProgressBarValue = data.Value;
        IsIndeterminateProgressBar = data.IsIndeterminate;
    }

    private void UpdateProperies() {
        OnPropertyChanged(nameof(SignInButtonVisibility));
        OnPropertyChanged(nameof(SignOutButtonVisibility));
        OnPropertyChanged(nameof(UserName));
        OnPropertyChanged(nameof(UserNameVisibility));
    }

    #region Command NavigateToOtherView - Команда переключиться на другое представление

    private ICommand _NavigateToOtherViewCommand;
    /// <summary>Команда - переключиться на другое представление</summary>
    public ICommand NavigateToOtherViewCommand => _NavigateToOtherViewCommand
        ??= new DelegateCommand<string>(OnNavigateToOtherViewCommandExecuted, CanNavigateToOtherViewCommandExecute);
    private bool CanNavigateToOtherViewCommandExecute(string viewName) => true;
    private void OnNavigateToOtherViewCommandExecuted(string viewName) =>
        regionManager.RequestNavigate(RegionNames.Main, viewName);

    #endregion

    #region Command SignIn - Команда авторизации

    private ICommand _SignInCommand;

    /// <summary>Команда - авторизации</summary>
    public ICommand SignInCommand => _SignInCommand
        ??= new DelegateCommand(OnSignInCommandExecuted);

    private async void OnSignInCommandExecuted() {
        await WrapInLoadingContext(async () => {
            await Authorization.AuthorizeAsync(coreService, true);
            UpdateProperies();
        }, "Авторизация");
    }

    #endregion

    #region Command SignOut - Команда разавторизация

    private ICommand _SignOutCommand;

    /// <summary>Команда - разавторизация</summary>
    public ICommand SignOutCommand => _SignOutCommand
        ??= new DelegateCommand(OnSignOutCommandExecuted);

    private void OnSignOutCommandExecuted() {
        coreService.SignOut();
        ConfigService.ResetAuthData();
        UpdateProperies();
    }

    #endregion
}

