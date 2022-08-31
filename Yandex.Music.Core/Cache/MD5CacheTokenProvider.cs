using System.Security.Cryptography;
using System.Text;
using Yandex.Api;

namespace Yandex.Music.Core.Cache;

public class Md5CacheTokenProvider : ICacheTokenProvider
{
    public string GetToken(RequestData requestData) {
        StringBuilder line = new(requestData.RequestUrl);
        if (requestData.RequestContent != null) {
            line.AppendLine(requestData.RequestContent.ReadAsStringAsync().Result);
        }

        if (requestData.AuthData != null) {
            line.AppendLine(requestData.AuthData.GetUid());
        }

        byte[] buffer = Encoding.UTF8.GetBytes(line.ToString());
        using (HashAlgorithm md5 = MD5.Create()) {
            buffer = md5.ComputeHash(buffer);
        }
        StringBuilder token = new(buffer.Length * 2);
        foreach (byte value in buffer) {
            token.Append(value.ToString("x2"));
        }

        return token.ToString();
    }
}
