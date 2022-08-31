namespace Yandex.Music.Core;

public class CoreServiceAuthData
{
    internal CoreServiceAuthData() {

    }

    public string Login { get; set; }

    public string EncryptedPassword { get; set; }

    public string EncryptedWebAuthDataCache { get; set; }

    public DateTime? WebAuthDataCacheCreatedTime { get; set; }

    public string EncryptedMobileAuthDataCache { get; set; }

    public DateTime? MobileAuthDataCacheCreatedTime { get; set; }

    public string SecretKey { get; set; }
}
