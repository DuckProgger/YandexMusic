using Microsoft.Extensions.Logging;
using Yandex.Api.Logging;

namespace Yandex.Music.Core.TaskSchedule;

public class TaskScheduleManager : IDisposable
{
    private readonly ILogger logger = LoggerService.Create<TaskScheduleManager>();

    public TaskScheduleManager(int initialMaxParallelTasksCount) {
        MaxParallelTasksCount = initialMaxParallelTasksCount;
        Task.Run(() => {
            StartShedulerService(shedulerServiceCancellationToken.Token);
        });
    }

    public int MaxParallelTasksCount { get; set; }

    public TaskScheduleInfo Add(Func<CancellationToken, Task> taskAction) {
        TaskScheduleInfo sheduledTaskInfo = new();
        CancellationToken sheduledTaskToken = sheduledTaskInfo.CancellationTokenSource.Token;
        sheduledTaskInfo.Task = new Task(() => {
            OnTaskRunning(sheduledTaskInfo, sheduledTaskToken, taskAction);
        });
        sheduledTaskInfo.IsPending = true;

        logger.LogTrace($"Добавление в очередь задачи Id={sheduledTaskInfo.Task.Id}");
        lock (list) {
            list.Add(sheduledTaskInfo);
        }

        shedulerServiceResetEvent.Set();

        return sheduledTaskInfo;
    }

    public void CancelAll() {
        lock (list) {
            logger.LogTrace($"Отмена поставленных в очередь задач (кол-во: {list.Count})");
            list.ForEach(x => x.Cancel());
        }
        shedulerServiceResetEvent?.Set();
    }

    public void Dispose() {
        shedulerServiceCancellationToken.Cancel();
        shedulerServiceCancellationToken.Dispose();
        shedulerServiceResetEvent.Set();

        AutoResetEvent disposedResetEvent = shedulerServiceResetEvent;
        shedulerServiceResetEvent = null;
        disposedResetEvent.Dispose();
    }

    protected virtual void OnTaskRunning(TaskScheduleInfo taskScheduleInfo, CancellationToken sheduledTaskToken, Func<CancellationToken, Task> taskAction) {
        try {
            sheduledTaskToken.ThrowIfCancellationRequested();
            taskAction.Invoke(sheduledTaskToken).Wait(sheduledTaskToken);
            OnTaskCompleted(taskScheduleInfo);
        }
        catch (OperationCanceledException) {
            OnTaskCanceled(taskScheduleInfo);
        }
        catch (Exception ex) {
            OnTaskError(taskScheduleInfo, ex);
            throw;
        }
    }

    protected virtual void OnTaskStarted(TaskScheduleInfo taskScheduleInfo) {
        Interlocked.Increment(ref runningTasksCount);

        logger.LogTrace($"Старт задачи Id={taskScheduleInfo.Task.Id} (текущее кол-во: {runningTasksCount})");
        taskScheduleInfo.Task.Start();
    }

    protected virtual void OnTaskCompleted(TaskScheduleInfo taskScheduleInfo) {
        Interlocked.Decrement(ref runningTasksCount);

        taskScheduleInfo.IsCompleted = true;
        taskScheduleInfo.Dispose();
        logger.LogTrace($"Завершение задачи Id={taskScheduleInfo.Task.Id} (текущее кол-во: {runningTasksCount})");

        shedulerServiceResetEvent?.Set();
    }

    protected virtual void OnTaskCanceled(TaskScheduleInfo taskScheduleInfo) {
        Interlocked.Decrement(ref runningTasksCount);

        taskScheduleInfo.IsCompleted = true;
        taskScheduleInfo.Dispose();
        logger.LogTrace($"Отмена задачи Id={taskScheduleInfo.Task.Id} (текущее кол-во: {runningTasksCount})");

        shedulerServiceResetEvent?.Set();
    }

    protected virtual void OnTaskError(TaskScheduleInfo taskScheduleInfo, Exception exception) {
        Interlocked.Decrement(ref runningTasksCount);

        taskScheduleInfo.IsCompleted = true;
        taskScheduleInfo.Dispose();
        logger.LogError(exception, $"Ошибка задачи Id={taskScheduleInfo.Task.Id} (текущее кол-во: {runningTasksCount})");

        shedulerServiceResetEvent?.Set();
    }


    private AutoResetEvent shedulerServiceResetEvent = new(false);
    private readonly CancellationTokenSource shedulerServiceCancellationToken = new();
    private readonly List<TaskScheduleInfo> list = new();
    private volatile int runningTasksCount;

    private void StartShedulerService(CancellationToken shedulerServiceCancellationToken) {
        logger.LogDebug("Запуск фонового сервиса обслуживания очереди");
        try {
            while (true) {

                shedulerServiceResetEvent?.WaitOne();
                if (shedulerServiceCancellationToken.IsCancellationRequested) {
                    break;
                }

                lock (list) {

                    // Очистка от выполненных задач
                    if (list.Any(x => x.IsCompleted)) {
                        int beforeCount = list.Count;
                        list.RemoveAll(x => x.IsCompleted);
                        logger.LogTrace($"Очистка от выполненных задач ({beforeCount}=>{list.Count})");
                    }

                    foreach (TaskScheduleInfo taskScheduleInfo in list) {
                        // Запуск задачи
                        if (runningTasksCount < MaxParallelTasksCount && taskScheduleInfo.IsPending) {
                            taskScheduleInfo.IsPending = false;

                            if (!taskScheduleInfo.CancellationTokenSource.IsCancellationRequested) {
                                OnTaskStarted(taskScheduleInfo);
                            }
                            else {
                                Interlocked.Increment(ref runningTasksCount);
                                OnTaskCanceled(taskScheduleInfo);
                            }
                        }

                        // Отмена задачи
                        else if (runningTasksCount > MaxParallelTasksCount && !taskScheduleInfo.IsCompleted) {
                            Interlocked.Increment(ref runningTasksCount);
                            OnTaskCanceled(taskScheduleInfo);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException) {
            logger.LogDebug("Завершение фонового сервиса обслуживания очереди");
        }
        catch (Exception ex) {
            logger.LogError(ex, "Аварийное завершение фонового сервиса обслуживания очереди");
        }
    }
}
