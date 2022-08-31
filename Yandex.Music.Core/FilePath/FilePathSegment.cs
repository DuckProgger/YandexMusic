namespace Yandex.Music.Core.FilePath;

public class FilePathSegment
{
    public FilePathSegment() {

    }

    public FilePathSegment(string path) : this() {
        Path = path;
    }

    public FilePathSegment(string path, string coverUri) : this(path) {
        CoverUri = coverUri;
    }

    public string Path { get; set; }

    public string CoverUri { get; set; }
}
