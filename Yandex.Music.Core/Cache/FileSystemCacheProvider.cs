using System.IO;
using System.Text;
using Yandex.Api;

namespace Yandex.Music.Core.Cache;

public class FileSystemCacheProvider : ICacheProvider
{
    public FileSystemCacheProvider() {
        CacheTokenProvider = new Md5CacheTokenProvider();
        Settings = new();
    }

    public FileSystemCacheProvider(FileSystemCacheProviderSettings settings) : this() {
        Settings = settings;
    }

    public FileSystemCacheProviderSettings Settings { get; set; }

    public ICacheTokenProvider CacheTokenProvider { get; set; }


    public async Task<byte[]> GetBytesAsync(RequestData requestData, CancellationToken cancellationToken) {
        string filePath = GetFilePath(requestData);
        return File.Exists(filePath) ? await File.ReadAllBytesAsync(filePath).ConfigureAwait(false) : null;
    }

    public async Task<string> GetStringAsync(RequestData requestData, CancellationToken cancellationToken) {
        string filePath = GetFilePath(requestData);
        return File.Exists(filePath) ? await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false) : null;
    }

    public async Task SetBytesAsync(RequestData requestData, byte[] content, CancellationToken cancellationToken) {
        string filePath = GetFilePath(requestData);
        if (!Directory.Exists(Settings.DirectoryPath)) {
            Directory.CreateDirectory(Settings.DirectoryPath);
        }
        await File.WriteAllBytesAsync(filePath, content).ConfigureAwait(false);
    }

    public async Task SetStringAsync(RequestData requestData, string content, CancellationToken cancellationToken) {
        string filePath = GetFilePath(requestData);
        if (!Directory.Exists(Settings.DirectoryPath)) {
            Directory.CreateDirectory(Settings.DirectoryPath);
        }
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8).ConfigureAwait(false);
    }


    private string GetFilePath(RequestData requestData) {
        string token = CacheTokenProvider.GetToken(requestData);
        return Path.Combine(Settings.DirectoryPath, token + Settings.FileExtension);
    }
}
