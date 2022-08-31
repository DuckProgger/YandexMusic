namespace Yandex.Music.Core.Audio.Tag;

public interface IAudioTagProvider
{
    Task SetAudioTagsAsync(string audioFilePath, AudioTagData audioTagData, CancellationToken cancellationToken);

    Task<AudioTagData> GetAudioTagDataAsync(string audioFilePath, CancellationToken cancellationToken);
}
