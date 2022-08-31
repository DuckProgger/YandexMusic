using System.IO;

namespace Yandex.Music.Core.FilePath;

public class FilePathList : List<FilePathSegment>
{
    public override string ToString() {
        return string.Join(Path.DirectorySeparatorChar, this.Select(x => x.Path).ToArray());
    }
}
