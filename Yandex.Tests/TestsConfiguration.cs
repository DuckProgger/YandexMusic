using Yandex.Api.Passport;

namespace Yandex.Tests;

public class TestsConfiguration
{
    public string Login { get; set; } = "yamusic-application-account"; // Контрольный вопрос: yamusic-application-account-to

    public string Password { get; set; } = "riehgoijroi!HIOTJHIO23523";

    public PassportWebAuthData AuthData = PassportWebAuthData.DeserializeFromJson("{\"Cookies\":{\"yandexuid\":\"460901101658021017\",\"Session_id\":\"3:1658021019.5.0.1658021019562:ChfjWw:7e.1.2:1|1645860220.0.2|3:10255381.813804.TkUw0hdIIwhEKgUtAEIGSQzG4Kg\",\"sessionid2\":\"3:1658021019.5.0.1658021019562:ChfjWw:7e.1.2:1.499:1|1645860220.0.2|3:10255381.212219.V847-usHBnGUwaeFDg3Ou6VGXVs\",\"yp\":\"1973381019.udn.cDp5YW11c2ljLWFwcGxpY2F0aW9uLWFjY291bnQ%3D\",\"ys\":\"udn.cDp5YW11c2ljLWFwcGxpY2F0aW9uLWFjY291bnQ%3D\",\"L\":\"WSx+W0VmUUJQfEJ7BERIBWRyYgBJR11gKwNfMBckVm4kKUEbUS44AhoeIlgOJiZdPCJD.1658021019.15041.331961.5c3db7570e9052f3f0ef0b0a183d43b7\",\"yandex_login\":\"yamusic-application-account\",\"uniqueuid\":\"234821401658021017\",\"sessguard\":\"1.1658021019.1658021019562:ChfjWw:7e..3.500:27397.758j9ZdB.8oOaKFNjyQG8SB2wkcddKCRDnvQ\",\"lah\":\"2:1721093019.10015519.EfSEpaAcDsaVceqS.Z7emGeZh3hrJZ5rPjIaHoWPWqUvGf7-i-GPYzVJLsvBk8-f6TIc9EyFI-RPV14IwuM5MQxgr9UsIlo3V_JB-.tozVJPrDrF0lXySgPrT8cQ\",\"mda2_beacon\":\"1658021019585\"}}");

    public PassportMobileAuthData MobileAuthData { get; set; } = null;//new() { AccessToken = "AQAAAABiGdV8AAG8XhNr_Vca2EosmErmCUPQTDU" };

    /// <summary>
    /// Автономный режим работы (использование только кэша).
    /// </summary>
    public bool OfflineMode { get; set; } = false;

    /// <summary>
    /// Загружать страницы с использованием кэша.
    /// </summary>
    public bool LoadFromCache { get; set; } = true;

    /// <summary>
    /// Принудительное обновление кэшированных данных.
    /// </summary>
    public bool UpdateCache { get; set; } = true;
}
