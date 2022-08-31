using System.IO;
using System.Text;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.FilePath.Snippet;

namespace Yandex.Music.Core.FilePath;

public class FilePathProvider : IFilePathProvider
{
    public FilePathProvider() {
        AddDefaultSnippetReplacers();
    }

    public string ReplaceInvalidPathCharText { get; set; } = "_";

    private readonly Dictionary<string, SnippetReplacer> snippetReplacers = new();
    private readonly HashSet<char> invalidFileNameChars = new(Path.GetInvalidFileNameChars());
    private readonly HashSet<char> invalidPathChars = new(Path.GetInvalidPathChars());
  
    public FilePathList GetFilePathList(string pathTemplate, FilePathCreationData data) {
        FilePathList filePathList = new();

        // Разделение шаблона на отдельные сегменты пути (каталоги и имя файла)
        string[] pathSegments = pathTemplate.Split(new char[] { '/', '\\' }, StringSplitOptions.None);

        // Преобразование каждого сегмента пути по отдельности
        for (int i = 0; i < pathSegments.Length; i++) {
            string pathSegment = pathSegments[i];

            StringBuilder pathSegmentText = new();
            string coverUri = null;

            SnippetString parsedPathSegment = SnippetString.Parse(pathSegment);
            foreach (SnippetStringFragment fragment in parsedPathSegment) {
                if (fragment.IsSnippet) {
                    string snippetName = fragment.Text;
                    if (snippetReplacers.TryGetValue(snippetName, out SnippetReplacer snipperReplacer)) {
                        FilePathSegment buildedPathSegment = snipperReplacer.Invoke(data);
                        if (buildedPathSegment != null) {
                            pathSegmentText.Append(buildedPathSegment.Path);
                            if (coverUri == null) {
                                coverUri = buildedPathSegment.CoverUri;
                            }
                        }
                    }
                    else {
                        throw new Exception($"Не удалось распознать сниппет '{snippetName}'");
                    }
                }
                else {
                    pathSegmentText.Append(fragment.Text);
                }
            }

            // Замена некорректных символов в сегменте пути
            string fixPathSegment = (i < pathSegments.Length - 1)
                ? ReplaceInvalidPathChars(pathSegmentText.ToString(), invalidPathChars)
                : ReplaceInvalidPathChars(pathSegmentText.ToString(), invalidFileNameChars);

            filePathList.Add(new FilePathSegment {
                Path = fixPathSegment,
                CoverUri = coverUri,
            });
        }

        return filePathList;
    }

    public string GetTempFileName(FilePathCreationData data) {
        StringBuilder str = new($"track.{data.StartDownloadInfo.Track.Id}");
        if (data.StartDownloadInfo.ParentEntity is WebAlbum album) {
            str.Append($".album.{album.Id}");
        }
        if (data.StartDownloadInfo.ParentEntity is WebPlaylist playlist) {
            str.Append($".playlist.{playlist.Owner.Uid}.{playlist.Kind}");
        }
        if (data.StartDownloadInfo.ParentEntity is WebArtist artist) {
            str.Append($".artist.{artist.Id}");
        }
        str.Append($".{data.MobileTrackDownloadData.Codec}");
        str.Append($".{data.MobileTrackDownloadData.Bitrate}");
        return str.ToString();
    }

    public void AddSnippetReplacer(string snippetName, SnippetReplacer replacer) {
        snippetReplacers.Add(snippetName, replacer);
    }


    private string ReplaceInvalidPathChars(string value, HashSet<char> invalidChars) {
        StringBuilder str = new(value.Length);
        foreach (char c in value) {
            if (invalidChars.Contains(c)) {
                str.Append(ReplaceInvalidPathCharText);
            }
            else {
                str.Append(c);
            }
        }
        return str.ToString();
    }

    private void AddDefaultSnippetReplacers() {
        AddSnippetReplacer("artist_name", (data) => {
            return new(data.StartDownloadInfo.Track.Artists?[0]?.Name);
        });

        AddSnippetReplacer("track_title", (data) => {
            return new(data.StartDownloadInfo.Track.Title);
        });

        AddSnippetReplacer("album_year", (data) => {
            return data.StartDownloadInfo.ParentEntity is WebAlbum album
            ? new(album.Year?.ToString())
            : null;
        });

        AddSnippetReplacer("album_volume_number", (data) => {
            return new(data.StartDownloadInfo.AlbumVolumeNumber?
                .ToString(new string('0', data.StartDownloadInfo.AlbumVolumeTotalCount.ToString().Length)));
        });

        AddSnippetReplacer("album_title", (data) => {
            return data.StartDownloadInfo.ParentEntity is WebAlbum album
              ? new(album.Title, album.CoverUri ?? album.OgImage)
              : null;
        });

        AddSnippetReplacer("track_number", (data) => {
            return new(data.StartDownloadInfo.Number
                .ToString(new string('0', data.StartDownloadInfo.TotalCount.ToString().Length)));
        });

        AddSnippetReplacer("track_number_in_album_volume", (data) => {
            return new(data.StartDownloadInfo.AlbumVolumeTrackNumber?
                .ToString(new string('0', data.StartDownloadInfo.AlbumVolumeTrackTotalCount.ToString().Length)));
        });

        AddSnippetReplacer("track_count", (data) => {
            return new(data.StartDownloadInfo.TotalCount.ToString());
        });

        AddSnippetReplacer("track_count_in_album_volume", (data) => {
            return new(data.StartDownloadInfo.AlbumVolumeTrackTotalCount.ToString());
        });

        AddSnippetReplacer("playlist_title", (data) => {
            return data.StartDownloadInfo.ParentEntity is WebPlaylist playlist
                ? new(playlist.Title, playlist.Cover?.Uri ?? playlist.OgImage)
                : null;
        });

        AddSnippetReplacer("audio_codec", (data) => {
            return new(data.MobileTrackDownloadData.Codec);
        });

        AddSnippetReplacer("audio_bitrate", (data) => {
            return new(data.MobileTrackDownloadData.Bitrate.ToString());
        });
    }
}
