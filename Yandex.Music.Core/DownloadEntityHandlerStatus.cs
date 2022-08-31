namespace Yandex.Music.Core;

public enum DownloadEntityHandlerStatus
{
    Pending,

    Starting,
    GetTrackInfo,
    GetDirectUrl,
    Downloading,
    Tagging,
    Stopping,

    Stopped,
    Error,

    ResultFileExists,
    Finished,
}
