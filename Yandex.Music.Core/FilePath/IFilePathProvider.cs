namespace Yandex.Music.Core.FilePath;

public interface IFilePathProvider
{
    FilePathList GetFilePathList(string pathTemplate, FilePathCreationData data);

    string GetTempFileName(FilePathCreationData data);
}
