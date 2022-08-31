using Yandex.Api;
using Yandex.Api.Music.Mobile;
using Yandex.Api.Music.Web;
using Yandex.Api.Passport;

namespace Yandex.Tests.Internal;

internal class TestFactory
{
    public static TestsConfiguration Configuration { get; private set; } = new TestsConfiguration();

    public static IDataProvider DataProvider { get; } = new TestDataProvider();

    public static YandexMusicMobileApi GetMusicMobileApi() {
        YandexMusicMobileApi api = new(new TestDataProvider());
        return api;
    }

    public static YandexMusicWebApi GetMusicWebApi() {
        YandexMusicWebApi api = new(new TestDataProvider());
        return api;
    }

    public static YandexPassportApi GetPassportApi() {
        YandexPassportApi api = new(new TestDataProvider());
        return api;
    }

    public static PassportWebAuthData GetWebAuthData() {
        if (Configuration.AuthData == null) {
            Configuration.AuthData = GetPassportApi()
                .WebAuthAsync(Configuration.Login, Configuration.Password, default).Result;
            string serializeString = Configuration.AuthData.SerializeToJson();
        }
        return Configuration.AuthData;
    }

    public static PassportMobileAuthData GetMobileAuthData() {
        if (Configuration.MobileAuthData == null) {
            Configuration.MobileAuthData = GetPassportApi()
                .MobileAuthAsync(Configuration.Login, Configuration.Password, default).Result;
            string serializeString = Configuration.MobileAuthData.SerializeToJson();
        }
        return Configuration.MobileAuthData;
    }
}
