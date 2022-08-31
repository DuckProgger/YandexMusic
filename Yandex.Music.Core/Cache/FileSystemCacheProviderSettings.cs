namespace Yandex.Music.Core.Cache;

public class FileSystemCacheProviderSettings
{
    public string DirectoryPath { get; set; } = "cache";

    public string FileExtension { get; set; } = ".cache";
}
