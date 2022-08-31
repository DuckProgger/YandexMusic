using Microsoft.Extensions.Logging;
using Prism.Commands;
using System;
using System.Windows.Input;
using Yandex.Api.Logging;
using Yandex.Music.Infrastructure;
using Yandex.Music.Settings;

namespace Yandex.Music.ViewModels;
internal class AuthorizationViewModel : ViewModelBase
{
    private readonly ILogger logger = LoggerService.Create<AuthorizationViewModel>();

    public AuthorizationViewModel() {
        Title = "Авторизация";
    }

    // TODO Значения по умолчанию только для дебага
    public string Login { get; set; } = "yamusic-application-account";
    public string Password { get; set; } = "riehgoijroi!HIOTJHIO23523";

    #region Command Confirm - Команда логин

    private ICommand _ConfirmCommand;

    /// <summary>Команда - логин</summary>
    public ICommand ConfirmCommand => _ConfirmCommand
        ??= new DelegateCommand(OnConfirmCommandExecuted);

    private void OnConfirmCommandExecuted() {
        try {
            ConfigService.SetAuthData(Login, Password);
        }
        catch (ArgumentException ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnConfirmCommandExecuted");
            return;
        }
        DialogService dialogService = ServiceLocator.GetService<DialogService>();
        dialogService.SetResult(DialogService.DialogResults.Ok);
        dialogService.GoBack();
    }

    #endregion 
}
