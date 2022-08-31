using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Yandex.Music.Core.Crypto;

public class AesCryptoProvider : ICryptoProvider
{
    /// <summary>
    /// Длина ключа AES (128/192/256 бит).
    /// </summary>
    public int KeySize { get; set; } = 128;

    public string CreateKey() {
        byte[] key = new byte[KeySize / 8];
        using RandomNumberGenerator random = RandomNumberGenerator.Create();
        random.GetBytes(key);
        return Convert.ToBase64String(key);
    }

    public string DecryptString(string encryptedString, string key) {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedString);
        byte[] keyBytes = Convert.FromBase64String(key);

        using Aes aes = Aes.Create();
        aes.KeySize = keyBytes.Length * 8;
        aes.BlockSize = 128; // константа. Для AES = 128 бит
        aes.Padding = PaddingMode.Zeros;

        using MemoryStream ms = new(encryptedBytes);
        using BinaryReader reader = new(ms);
        int decryptedDataLength = reader.ReadInt32();

        aes.Key = keyBytes;
        aes.IV = reader.ReadBytes(aes.IV.Length);

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] encryptedData = reader.ReadBytes((int)(ms.Length - ms.Position));
        byte[] decryptedData = PerformCryptography(encryptedData, decryptor);

        if (decryptedData.Length != decryptedDataLength) {
            Array.Resize(ref decryptedData, decryptedDataLength);
        }

        return Encoding.UTF8.GetString(decryptedData);
    }

    public string EncryptString(string decryptedString, string key) {
        byte[] decryptedData = Encoding.UTF8.GetBytes(decryptedString);
        byte[] keyBytes = Convert.FromBase64String(key);

        using Aes aes = Aes.Create();
        aes.KeySize = keyBytes.Length * 8;
        aes.BlockSize = 128; // константа. Для AES = 128 бит
        aes.Padding = PaddingMode.Zeros;

        aes.Key = keyBytes;
        aes.GenerateIV();

        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        using ICryptoTransform encryptor = aes.CreateEncryptor();
        writer.Write(decryptedData.Length);
        writer.Write(aes.IV);
        writer.Write(PerformCryptography(decryptedData, encryptor));

        byte[] encryptedBytes = ms.ToArray();
        return Convert.ToBase64String(encryptedBytes);
    }


    private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform) {
        using MemoryStream ms = new();
        using CryptoStream cryptoStream = new(ms, cryptoTransform, CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        return ms.ToArray();
    }
}
