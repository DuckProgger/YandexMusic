using Microsoft.Extensions.Logging;
using Yandex.Api;
using Yandex.Api.Logging;

namespace Yandex.Music.Core.Audio.Tag;

public class AudioTagProvider : IAudioTagProvider
{
    private readonly ILogger logger = LoggerService.Create<AudioTagProvider>();
    
    public Task SetAudioTagsAsync(string audioFilePath, AudioTagData audioTagData, CancellationToken cancellationToken) {
        using TagLib.File audioFile = TagLib.File.Create(audioFilePath);

        bool completed = false;
        if (audioFile.TagTypes.HasFlag(TagLib.TagTypes.Apple)) {
            TagLib.Mpeg4.AppleTag appleTag = (TagLib.Mpeg4.AppleTag)audioFile.GetTag(TagLib.TagTypes.Apple);
            SetGenericAudioTag(appleTag, audioTagData);
            SetSpecificM4AAudioTag(appleTag, audioTagData);
            completed = true;
        }

        if (audioFile.TagTypes.HasFlag(TagLib.TagTypes.Id3v2)) {
            // Установка тэгов для формата MP3
            TagLib.Id3v2.Tag id3v2Tag = (TagLib.Id3v2.Tag)audioFile.GetTag(TagLib.TagTypes.Id3v2);
            SetGenericAudioTag(id3v2Tag, audioTagData);
            SetSpecificID3v2AudioTag(id3v2Tag, audioTagData);
            completed = true;
        }

        Validate.IsTrue(completed,
            () => new NotSupportedException($"Установка тэгов для кодека текущего аудиофайла не поддерживается."));

        audioFile.Save();
        return Task.CompletedTask;
    }

    public Task<AudioTagData> GetAudioTagDataAsync(string audioFilePath, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }


    private void SetGenericAudioTag(TagLib.Tag tag, AudioTagData audioTagData) {
        tag.Track = audioTagData.TrackNumber.HasValue ? audioTagData.TrackNumber.Value : 0;
        tag.TrackCount = audioTagData.TrackCount.HasValue ? audioTagData.TrackCount.Value : 0;
        tag.Disc = audioTagData.DiskNumber.HasValue ? audioTagData.DiskNumber.Value : 0;
        tag.DiscCount = audioTagData.DiskCount.HasValue ? audioTagData.DiskCount.Value : 0;
        tag.Title = audioTagData.Title;
        tag.Performers = audioTagData.Artists;
        tag.Album = audioTagData.Album;
        tag.Genres = audioTagData.Genres;
        tag.Year = audioTagData.Year.HasValue ? audioTagData.Year.Value : 0;
        tag.Comment = audioTagData.Comment;
        tag.AlbumArtists = audioTagData.AlbumArtists;
        tag.Copyright = audioTagData.Copyright;
        tag.Composers = audioTagData.Composers;
        //public string Conductor { get; set; }
        //public string Encoder { get; set; }
        //public string Mood { get; set; }
        //public string Catalog { get; set; }
        tag.ISRC = audioTagData.ISRC;
        tag.BeatsPerMinute = audioTagData.BeatsPerMinute.HasValue ? audioTagData.BeatsPerMinute.Value : 0;
        //public int? Rate { get; set; }
        tag.ReplayGainTrackGain = audioTagData.TrackGain.HasValue ? audioTagData.TrackGain.Value : double.NaN;
        tag.ReplayGainAlbumGain = audioTagData.AlbumGain.HasValue ? audioTagData.AlbumGain.Value : double.NaN;
        if (audioTagData.Picture != null) {
            TagLib.ByteVector byteVector = new(audioTagData.Picture);
            TagLib.Picture picture = new(byteVector);
            tag.Pictures = new TagLib.Picture[] { picture };
        }
        else {
            tag.Pictures = new TagLib.IPicture[0];
        }
        tag.Lyrics = audioTagData.Lyrics;
        //public string LyricsAuthor { get; set; }
        //public bool? CompilationPart { get; set; }
    }

    private void SetSpecificM4AAudioTag(TagLib.Mpeg4.AppleTag tag, AudioTagData audioTagData) {
        if (!string.IsNullOrEmpty(audioTagData.Url)) {
            tag.SetDashBox("com.apple.iTunes", "WWW", audioTagData.Url);
        }
        if (!string.IsNullOrEmpty(audioTagData.Publisher)) {
            tag.SetDashBox("com.apple.iTunes", "PUBLISHER", audioTagData.Publisher);
        }
    }

    private void SetSpecificID3v2AudioTag(TagLib.Id3v2.Tag tag, AudioTagData audioTagData) {
        if (!string.IsNullOrEmpty(audioTagData.Url)) {
            TagLib.Id3v2.TextInformationFrame tagUrl = TagLib.Id3v2.TextInformationFrame.Get(tag, "WXXX", TagLib.StringType.Latin1, true);
            tagUrl.Text = new string[] { audioTagData.Url };
        }
        if (!string.IsNullOrEmpty(audioTagData.Publisher)) {
            TagLib.Id3v2.TextInformationFrame tagPublisher = TagLib.Id3v2.TextInformationFrame.Get(tag, "TPUB", TagLib.StringType.UTF8, true);
            tagPublisher.Text = new string[] { audioTagData.Publisher };
        }
    }
}
