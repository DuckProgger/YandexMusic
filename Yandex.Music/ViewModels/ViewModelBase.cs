using Microsoft.Extensions.Logging;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using Yandex.Api.Logging;
using Yandex.Music.Events;
using Yandex.Music.Infrastructure;

namespace Yandex.Music.ViewModels;

internal abstract class ViewModelBase : INotifyPropertyChanged, INavigationAware
{
    private readonly ILogger logger = LoggerService.Create<ViewModelBase>();

    protected readonly IEventAggregator eventAggregator;
    private readonly ProgressBarEvent progressBarEvent;

    protected ViewModelBase() { }

    protected ViewModelBase(IEventAggregator eventAggregator) {
        this.eventAggregator = eventAggregator;
        progressBarEvent = eventAggregator.GetEvent<ProgressBarEvent>();
    }

    public Notifier Notifier { get; set; } = new();

    public string Title { get; set; } = string.Empty;


    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string prop = "") {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public virtual void OnNavigatedTo(NavigationContext navigationContext) { }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext) {
        return true;
    }

    public virtual void OnNavigatedFrom(NavigationContext navigationContext) { }

    internal async Task WrapInLoadingContext(Func<Task> action, ProgressBarProgress progress, string message = default) {
        var data = new ProgressBarData { Visibility = true, IsIndeterminate = false };
        progress.Updated += (sender, args) => {
            data.Value = (double)args.CurrentProgress / args.Total * 100.0;
            progressBarEvent.Publish(data);
        };
        await WrapInLoadingContext(action, data, message);
    }

    internal async Task WrapInLoadingContext(Func<Task> action, string message = default) {
        var data = new ProgressBarData { Visibility = true, IsIndeterminate = true };
        await WrapInLoadingContext(action, data, message);
    }

    private async Task WrapInLoadingContext(Func<Task> action, ProgressBarData data, string message = null) {
        progressBarEvent.Publish(data);
        bool isMessageExists = !string.IsNullOrEmpty(message);
        if (isMessageExists)
            Notifier.AddWarning(message);
        try {
            await action();
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "WrapInLoadingContext");
        }
        finally {
            data.Visibility = false;
            progressBarEvent.Publish(data);
            if (isMessageExists)
                Notifier.Remove(message);
        }
    }

    protected void ShowProgressBar() => progressBarEvent.Publish(new ProgressBarData { Visibility = true, IsIndeterminate = true });
    protected void HideProgressBar() => progressBarEvent.Publish(new ProgressBarData { Visibility = false, IsIndeterminate = true });
}

internal class ViewModelBase<T> : ViewModelBase
{
    protected ViewModelBase(IEventAggregator eventAggregator) : base(eventAggregator) {
        collectionView = (CollectionView)CollectionViewSource.GetDefaultView(Collection);
    }
    public ObservableCollection<T> Collection { get; set; } = new();

    private readonly ICollectionView collectionView;

    protected void SetFilter(Predicate<T> predicate) {
        collectionView.Filter = obj => predicate((T)obj);
    }

    protected void RefreshFilter() {
        collectionView.Refresh();
    }

    protected ViewModelBase() : this(null) {
    }
}