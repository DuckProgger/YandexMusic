namespace Yandex.Music.Core.Crypto;

internal class NoneCryptoProvider : ICryptoProvider
{
    public string CreateKey() {
        return null;
    }

    public string DecryptString(string encryptedString, string key) {
        return encryptedString;
    }

    public string EncryptString(string decryptedString, string key) {
        return decryptedString;
    }
}
