using System.Threading.Tasks;
using Yandex.Music.Core;
using Yandex.Music.Settings;
using Yandex.Music.Views;

namespace Yandex.Music.Infrastructure;

internal class Authorization
{
    public static async Task<bool> AuthorizeAsync(CoreService coreService, bool forcedDialog) {
        // Получение логина и пароля из конфига
        RootSettings settings = ConfigService.GetSettings();

        // Если в конфиге нет логина и пароля, то получить данные из окна авторизации
        if (forcedDialog || settings.Auth == null) {
            DialogService.DialogResult result = await GetAuthorizeDataFromDialogAsync();
            if (result.Result != DialogService.DialogResults.Ok) {
                return false;
            }
        }

        // Авторизация
        try {
            CoreServiceAuthData actualAuth = await coreService.SignInAsync(settings.Auth, default);
            settings.Auth = actualAuth;
            ConfigService.SetSettings(settings);
            return true;
        }
        catch {
            ConfigService.ResetAuthData();
            throw;
        }
    }

    private static async Task<DialogService.DialogResult> GetAuthorizeDataFromDialogAsync() {
        DialogService dialogService = ServiceLocator.GetService<DialogService>();
        return await dialogService.ShowDialog(nameof(AuthorizationView));
    }
}
