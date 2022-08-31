namespace Yandex.Music.Core.Crypto;

public interface ICryptoProvider
{
    string CreateKey();

    string DecryptString(string encryptedString, string key);

    string EncryptString(string decryptedString, string key);
}
