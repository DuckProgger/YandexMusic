using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using Yandex.Api.Logging;
using Yandex.Music.Core;
using Yandex.Music.Events;

namespace Yandex.Music.ViewModels;
[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
internal class DownloadsViewModel : ViewModelBase<DownloadEntityHandler>
{
    private readonly ILogger logger = LoggerService.Create<DownloadsViewModel>();

    private readonly CoreService coreService;

    public DownloadsViewModel(CoreService coreService, IEventAggregator eventAggregator) : base(eventAggregator) {
        this.coreService = coreService;
        eventAggregator.GetEvent<DownloadEvent>().Subscribe(AddDownloadsAsync);
        SubscribeOnCoreServicePropertyChanged(nameof(coreService.DownloadBytesInSecond),
            () => TotalDownloadSpeed = coreService.DownloadBytesInSecond);
    }

    /// <summary>
    /// Подписка на обновление свойства главного сервиса.
    /// </summary>
    private void SubscribeOnCoreServicePropertyChanged(string propertyName, Action callback) {
        coreService.PropertyChanged += (s, e) => {
            if (e.PropertyName == propertyName)
                callback.Invoke();
        };
    }

    private async void AddDownloadsAsync(List<EntityHandler> entities) {
        try {
            await coreService.AddDownloadsAsync(entities, default);
            Collection.Clear();
            Collection.AddRange(coreService.DownloadsList);
            OnCollectionChangedInternal();
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "AddDownloadsAsync");
        }
    }

    private void OnCollectionChangedInternal() {
        OnPropertyChanged(nameof(Collection));
        OnPropertyChanged(nameof(IsCollectionNotEmpty));
    }

    public List<DownloadEntityHandler> SelectedEntities { get; set; } = new();

    public bool IsEntitySelected => SelectedEntities.Count > 0;

    public bool IsCollectionNotEmpty => Collection.Count > 0;

    public double TotalDownloadSpeed { get; private set; }

    #region Command StartDownloads - Команда запустить загрузку выбранных сущностей

    private ICommand _StartDownloadsCommand;

    /// <summary>Команда - запустить загрузку выбранных сущностей</summary>
    public ICommand StartDownloadsCommand => _StartDownloadsCommand
        ??= new DelegateCommand<IEnumerable<DownloadEntityHandler>>(OnStartDownloadsCommandExecuted);

    private async void OnStartDownloadsCommandExecuted(IEnumerable<DownloadEntityHandler> entities) {
        await coreService.ResumeDownloadsAsync(entities);
    }

    #endregion

    #region Command StopOrResumeDownload - Команда остановить или запустить загрузку выбранной сущности

    private ICommand _StopOrResumeDownloadCommand;

    /// <summary>Команда - остановить или запустить загрузку выбранной сущности</summary>
    public ICommand StopOrResumeDownloadCommand => _StopOrResumeDownloadCommand
        ??= new DelegateCommand<IEnumerable<DownloadEntityHandler>>(OnStopOrResumeDownloadCommandExecuted);

    private async void OnStopOrResumeDownloadCommandExecuted(IEnumerable<DownloadEntityHandler> entities) {
        var entitiesArray =
            entities as DownloadEntityHandler[] ?? entities.ToArray();
        var entity = entitiesArray.SingleOrDefault();
        if (entity == null || entitiesArray.Length != 1)
            return;
        try {
            if (entity.CanResume)
                await coreService.ResumeDownloadsAsync(entitiesArray);
            else
                await coreService.StopDownloadsAsync(entitiesArray);
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnStopOrResumeDownloadCommandExecuted");
        }
    }

    #endregion

    #region Command StartAllDownloads - Команда запустить все загрузки

    private ICommand _StartAllDownloadsCommand;

    /// <summary>Команда - запустить все загрузки</summary>
    public ICommand StartAllDownloadsCommand => _StartAllDownloadsCommand
        ??= new DelegateCommand(OnStartAllDownloadsCommandExecuted);

    private async void OnStartAllDownloadsCommandExecuted() {
        try {
            await coreService.ResumeAllDownloadAsync();
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnStartAllDownloadsCommandExecuted");
        }
    }

    #endregion

    #region Command StopAllDownloads - Команда остановить все загрузки

    private ICommand _StopAllDownloadsCommand;

    /// <summary>Команда - остановить все загрузки</summary>
    public ICommand StopAllDownloadsCommand => _StopAllDownloadsCommand
        ??= new DelegateCommand(OnStopAllDownloadsCommandExecuted);

    private async void OnStopAllDownloadsCommandExecuted() {
        try {
            await coreService.StopAllDownloadAsync();
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnStopAllDownloadsCommandExecuted");
        }
    }

    #endregion

    #region Command StopDownloads - Команда остановить загрузку выбранных сущностей

    private ICommand _StopDownloadsCommand;

    /// <summary>Команда - приостановить загрузку выбранных сущностей</summary>
    public ICommand StopDownloadsCommand => _StopDownloadsCommand
        ??= new DelegateCommand<IEnumerable<DownloadEntityHandler>>(OnStopDownloadsCommandExecuted);

    private async void OnStopDownloadsCommandExecuted(IEnumerable<DownloadEntityHandler> entities) {
        try {
            await coreService.StopDownloadsAsync(entities);
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnStopDownloadsCommandExecuted");
        }
    }

    #endregion

    #region Command CancelDownloads - Команда отменить выбранные загрузки

    private ICommand _CancelDownloadsCommand;

    /// <summary>Команда - отменить выбранные закачки</summary>
    public ICommand CancelDownloadsCommand => _CancelDownloadsCommand
        ??= new DelegateCommand<IEnumerable<DownloadEntityHandler>>(OnCancelDownloadsCommandExecuted);

    private async void OnCancelDownloadsCommandExecuted(IEnumerable<DownloadEntityHandler> entities) {
        IEnumerable<DownloadEntityHandler> entitiesArray = entities as DownloadEntityHandler[] ?? entities.ToArray();
        try {
            await coreService.RemoveDownloadsAsync(entitiesArray);
            foreach (var entity in entitiesArray)
                Collection.Remove(entity);
            OnCollectionChangedInternal();
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnCancelDownloadsCommandExecuted");
        }
    }

    #endregion

    #region Command RemoveAllDownloads - Команда удалить все загрузки

    private ICommand _RemoveAllDownloadsCommand;

    /// <summary>Команда - удалить все загрузки</summary>
    public ICommand RemoveAllDownloadsCommand => _RemoveAllDownloadsCommand
        ??= new DelegateCommand(OnRemoveAllDownloadsCommandExecuted);

    private async void OnRemoveAllDownloadsCommandExecuted() {
        try {
            await coreService.RemoveAllDownloadsAsync();
            Collection.Clear();
            OnCollectionChangedInternal();
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnRemoveAllDownloadsCommandExecuted");
        }
    }

    #endregion

    #region Command RemoveDownloadsWithFile - Команда удалить загрузки вместе с файлом

    private ICommand _RemoveDownloadsWithFileCommand;

    /// <summary>Команда - удалить загрузки вместе с файлом</summary>
    public ICommand RemoveDownloadsWithFileCommand => _RemoveDownloadsWithFileCommand
        ??= new DelegateCommand<IEnumerable<DownloadEntityHandler>>(OnRemoveDownloadsWithFileCommandExecuted);

    private async void OnRemoveDownloadsWithFileCommandExecuted(IEnumerable<DownloadEntityHandler> entities) {
        try {
            await coreService.RemoveDownloadsWithFilesAsync(entities);
            Collection.RemoveRange(entities);
            OnCollectionChangedInternal();
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnRemoveDownloadsWithFileCommandExecuted");
        }
    }

    #endregion

    #region Command OpenDownloads - Команда открыть (воспроизвести) выбранные сущности

    private ICommand _OpenDownloadsCommand;

    /// <summary>Команда - открыть (воспроизвести) выбранные сущности</summary>
    public ICommand OpenDownloadsCommand => _OpenDownloadsCommand
        ??= new DelegateCommand<IEnumerable<DownloadEntityHandler>>(OnOpenDownloadsCommandExecuted);

    private async void OnOpenDownloadsCommandExecuted(IEnumerable<DownloadEntityHandler> entities) {
        try {
            await coreService.OpenDownloadFilesAsync(entities);
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnOpenDownloadsCommandExecuted");
        }
    }

    #endregion

    #region Command OpenDownloadsInFolder - Команда открыть выбранные сущности в папке

    private ICommand _OpenDownloadsInFolderCommand;

    /// <summary>Команда - открыть выбранные сущности в папке</summary>
    public ICommand OpenDownloadsInFolderCommand => _OpenDownloadsInFolderCommand
        ??= new DelegateCommand<IEnumerable<DownloadEntityHandler>>(OnOpenDownloadsInFolderCommandExecuted);

    private async void OnOpenDownloadsInFolderCommandExecuted(IEnumerable<DownloadEntityHandler> entities) {
        try {
            await coreService.OpenDownloadFilesInFoldersAsync(entities);
        }
        catch (Exception ex) {
            Notifier.AddError(ex.Message);
            logger.LogError(ex, "OnOpenDownloadsInFolderCommandExecuted");
        }
    }

    #endregion

    #region Command GetSelectedItems - Команда получить выбранные позиции

    private ICommand _GetSelectedItemsCommand;

    /// <summary>Команда - получить выбранные позиции</summary>
    public ICommand GetSelectedItemsCommand => _GetSelectedItemsCommand
        ??= new DelegateCommand<object>(OnGetSelectedItemsCommandExecuted);

    private void OnGetSelectedItemsCommandExecuted(object entities) {
        var items = entities as IList;
        IEnumerable<DownloadEntityHandler> collection = items?
            .Cast<DownloadEntityHandler>();
        if (collection is null)
            return;
        SelectedEntities.Clear();
        SelectedEntities.AddRange(collection);
        OnPropertyChanged(nameof(IsEntitySelected));
    }

    #endregion
}
