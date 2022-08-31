using System.ComponentModel;
using System.Runtime.CompilerServices;
using Yandex.Api.Music.Web.Entities;
using Yandex.Music.Core.Annotations;
using Yandex.Music.Core.TaskSchedule;

namespace Yandex.Music.Core;

public class DownloadEntityHandler : INotifyPropertyChanged
{

    public EntityHandler TrackEntity { get; set; }

    public EntityHandler ParentEntity { get; set; }


    public DownloadEntityHandlerStatus Status { get; set; } = DownloadEntityHandlerStatus.Pending;

    public string ErrorMessage { get; set; }


    public long? DownloadLength {
        get => downloadLength;
        set {
            downloadLength = value;
            ChangeDownloadProgress();
        }
    }

    public long? DownloadPosition {
        get => downloadPosition;
        set {
            downloadPosition = value;
            ChangeDownloadProgress();
        }
    }

    public double DownloadProgress { get; set; }


    public StartDownloadInfo StartDownloadInfo { get; set; }

    public WebTrackData TrackData { get; set; }

    public bool CanResume => Status is DownloadEntityHandlerStatus.Stopped
                                    or DownloadEntityHandlerStatus.Error;

    public bool CanStop => Status is not (DownloadEntityHandlerStatus.Finished
                                       or DownloadEntityHandlerStatus.ResultFileExists
                                       or DownloadEntityHandlerStatus.Stopped
                                       or DownloadEntityHandlerStatus.Error);

    public bool CanResumeOrStop => CanResume || CanStop;

    public TaskScheduleInfo DownloadTaskInfo { get; set; }


    private long? downloadLength;
    private long? downloadPosition;
    private void ChangeDownloadProgress() {
        if (DownloadLength.HasValue && DownloadPosition.HasValue) {
            if (DownloadLength.Value > 0) {
                DownloadProgress = (double)DownloadPosition.Value / DownloadLength.Value * 100.0;
            }
        }
    }




    public string DownloadTempFilePath { get; set; }

    public string DownloadedFilePath { get; set; }

    public string TaggedFilePath { get; set; }

    public string ResultFilePath { get; set; }





    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
