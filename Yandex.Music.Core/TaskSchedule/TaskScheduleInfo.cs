namespace Yandex.Music.Core.TaskSchedule;

public class TaskScheduleInfo : IDisposable
{
    public bool IsPending { get; set; }

    public bool IsCompleted { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; private set; } = new();

    public Task Task { get; set; }


    public void Cancel() {
        CancellationTokenSource.Cancel();
    }

    public void Dispose() {
        CancellationTokenSource.Dispose();
    }
}
