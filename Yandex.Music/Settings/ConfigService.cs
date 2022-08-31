using System;
using System.IO;
using System.Text;
using Yandex.Api;
using Yandex.Music.Core;
using Yandex.Music.Core.StringSerializers;
using Yandex.Music.Infrastructure;

namespace Yandex.Music.Settings;

internal class ConfigService
{
    // Путь к конфигу
    private const string settingsFilePath = "appsettings.json";
    private static readonly IStringSerializer<RootSettings> stringSerializer;
    private static RootSettings settings;
    private static readonly Lazy<CoreService> coreServiceLazy;

    static ConfigService() {
        // Инициализация базовых провайдеров (думаю на протяжении всего жизненного цикла программы они останутся теми же)
        stringSerializer = new JsonStringSerializer<RootSettings>();
        coreServiceLazy = new Lazy<CoreService>(ServiceLocator.GetService<CoreService>);
    }

    public static RootSettings GetSettings() {

        // Получение конфига из файла в случае совместимости его версии, иначе создание нового (с прописыванием секрета)
        if (settings != null) {
            return settings;
        }

        if (File.Exists(settingsFilePath)) {
            RootSettings readedRootSettings =
                stringSerializer.Deserialize(File.ReadAllText(settingsFilePath, Encoding.UTF8));
            if (readedRootSettings.Version == RootSettings.ActualVersion) {
                settings = readedRootSettings;
            }
        }

        return settings ??= new RootSettings();
    }

    public static void SetSettings(RootSettings newSettings) {
        File.WriteAllText(settingsFilePath, stringSerializer.Serialize(newSettings));
        settings.Refresh(newSettings);
        coreServiceLazy.Value.ApplySettings();
    }

    public static void SetAuthData(string login, string password) {
        Validate.NotEmpty(login, "Логин не может быть пустым.");
        Validate.NotEmpty(password, "Пароль не может быть пустым.");

        CoreServiceAuthData authData = coreServiceLazy.Value.CreateAuthData(login, password);
        settings.Auth = authData;
    }

    public static void ResetAuthData() {
        settings.Auth = null;
    }
}
