using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yandex.Music.Core.Crypto;

namespace Yandex.Tests.Core.Secret;

[TestClass]
public class AESSecretProviderTests
{
    [TestMethod]
    public void EncodeAndDecode() {
        AesCryptoProvider aesProvider = new() {
            KeySize = 256,
        };
        string key = aesProvider.CreateKey();
        string encodedString = aesProvider.EncryptString("test", key);
        string decodedString = aesProvider.DecryptString(encodedString, key);
        Assert.AreEqual("test", decodedString);
    }
}
