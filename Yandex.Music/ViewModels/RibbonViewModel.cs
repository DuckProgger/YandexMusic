using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Yandex.Api.Logging;
using Yandex.Api.Music.Web;
using Yandex.Music.Core;
using Yandex.Music.Events;
using Yandex.Music.Infrastructure;
using Yandex.Music.Settings;

namespace Yandex.Music.ViewModels;
internal class RibbonViewModel : ViewModelBase<EntityHandler>
{
    private readonly ILogger logger = LoggerService.Create<RibbonViewModel>();

    private readonly CoreService coreService;
    private EntityHandler selectedEntity;
    private readonly DownloadEvent downloadEvent;
    private readonly AuthorizeEvent authorizeEvent;
    private bool initialized;
    private CancellationTokenSource playCancellation;

    public RibbonViewModel(CoreService coreService, IEventAggregator eventAggregator) : base(eventAggregator) {
        this.coreService = coreService;
        downloadEvent = eventAggregator.GetEvent<DownloadEvent>();
        authorizeEvent = eventAggregator.GetEvent<AuthorizeEvent>();
        Notifier.SetMessageParams(MessageType.Warning, new MessageParameters(false));
        SetFilter(RibbonFilter);
        StartPageLink = ConfigService.GetSettings()?.Gui?.StartPageLink;
    }

    public string Query { get; set; }
    public PageData CurrentPageData { get; set; }

    public EntityHandler SelectedEntity {
        get => selectedEntity;
        set {
            selectedEntity = value;
            OnPropertyChanged(nameof(IsEntitySelected));
        }
    }
    public List<EntityHandler> SelectedEntities { get; set; } = new();

    public bool IsPrevPageExists => coreService.HasPrevPage;
    public bool IsNextPageExists => coreService.HasNextPage;
    public bool IsEntitySelected => !SelectedEntity?.IsCaption ?? false;
    public bool IsSelectedEntitiesDownloadSupports => SelectedEntities?.Count > 0 &&
                                                      SelectedEntities.All(e => e.SupportDownload);
    public bool IsSelectedEntitiesPlaySupports => SelectedEntities?.Count > 0 &&
                                                  SelectedEntities.All(e => e.SupportPlay) &&
                                                  !PlayCancellationButtonVisibility;
    public bool IsSelectedEntitiesOpenSupports => SelectedEntities?.Count == 1 && (SelectedEntities?.First().SupportOpen ?? false);
    public bool IsMainEntityDownloadSupports => CurrentPageData?.MainEntity?.SupportDownload ?? false;
    public bool IsMainEntityPlaySupports => CurrentPageData?.MainEntity?.SupportPlay ?? false;
    public bool IsMainEntityOpenSupports => CurrentPageData?.MainEntity?.SupportOpen ?? false;
    public bool DownloadMainEntityButtonVisibility => CurrentPageData?.MainEntity?.SupportDownload ?? false;
    public bool ArtistButtonsVisibility => SelectedEntity?.EntityType == EntityType.Artist;
    public bool SearchFieldVisibility { get; set; }
    public bool PlayCancellationButtonVisibility { get; set; }
    public string StartPageLink { get; }

    public string EntityFilter {
        get => entityFilter;
        set {
            entityFilter = value;
            RefreshFilter();
        }
    }

    public event EventHandler PageChanged;

    private void OnPageChanged() {
        PageChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool RibbonFilter(EntityHandler entity) =>
        EntityFilter is null ||
        (entity.Title?.Contains(EntityFilter, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (entity.SecondTitles?.Any(st => st.Title.Contains(EntityFilter, StringComparison.OrdinalIgnoreCase)) ?? false);

    private void RefreshPage() {
        Collection.Clear();
        Collection.AddRange(CurrentPageData.Ribbon);
        Query = CurrentPageData.Query;
        OnPropertyChanged(nameof(Collection));
        OnPropertyChanged(nameof(IsNextPageExists));
        OnPropertyChanged(nameof(IsPrevPageExists));
        OnPropertyChanged(nameof(DownloadMainEntityButtonVisibility));
    }

    private async Task FollowLinkAsync(string link) {
        if (string.IsNullOrWhiteSpace(link) || CurrentPageData?.Query == link) {
            return;
        }

        await WrapInLoadingContext(async () => {
            CurrentPageData = await coreService.OpenPageDataAsync(link, default);
            RefreshPage();
            OnPageChanged();
        });
    }

    private async Task<bool> CheckAndExecuteAuthorizationAsync(bool forcedDialog) {
        if (coreService.Authorized) {
            return true;
        }

        ShowProgressBar();
        Notifier.AddWarning("Авторизация...");
        try {
            bool authorizationResult = await Authorization.AuthorizeAsync(coreService, forcedDialog);
            authorizeEvent.Publish(authorizationResult);
            return authorizationResult;
        }
        catch (Exception ex) {
            Notifier.AddError($"Ошибка авторизации: {ex.Message}");
            logger.LogError(ex, "Ошибка авторизации");
            return false;
        }
        finally {
            Notifier.Remove("Авторизация...");
            HideProgressBar();
        }
    }

    #region Command Initializing - Команда инициализация

    private ICommand _InitializingCommand;

    /// <summary>Команда - инициализация</summary>
    public ICommand InitializingCommand => _InitializingCommand
        ??= new DelegateCommand(OnInitializingCommandExecuted);

    private async void OnInitializingCommandExecuted() {
        if (!initialized && await CheckAndExecuteAuthorizationAsync(false)) {
            Query = StartPageLink;
            await FollowLinkAsync(StartPageLink);
            initialized = true;
        }
    }

    #endregion

    #region Command FollowLink - Команда перейти по выбранной ссылке

    private ICommand _FollowLinkCommand;

    /// <summary>Команда - перейти по выбранной ссылке</summary>
    public ICommand FollowLinkCommand => _FollowLinkCommand
        ??= new DelegateCommand<string>(OnFollowLinkCommandExecuted);

    private async void OnFollowLinkCommandExecuted(string searchQuery) {
        if (await CheckAndExecuteAuthorizationAsync(true)) {
            await FollowLinkAsync(searchQuery);
        }
    }

    #endregion

    #region Command FollowLinkByUrlType - Команда перейти по ссылке в зависимости от требуемого типа

    private ICommand _FollowLinkByUrlTypeCommand;

    /// <summary>Команда - перейти по ссылке в зависимости от требуемого типа</summary>
    public ICommand FollowLinkByUrlTypeCommand => _FollowLinkByUrlTypeCommand
        ??= new DelegateCommand<EntityHandlerUrlType?>(OnFollowLinkByUrlTypeCommandExecuted);

    private async void OnFollowLinkByUrlTypeCommandExecuted(EntityHandlerUrlType? type) {
        if (type.HasValue && await CheckAndExecuteAuthorizationAsync(true)) {
            string link = SelectedEntity.GetUrl(type.Value);
            await FollowLinkAsync(link);
        }
    }

    #endregion

    #region Command NextPage - Команда перейти на следующую страницу

    private ICommand _NextPageCommand;

    /// <summary>Команда - перейти на следующую страницу</summary>
    public ICommand NextPageCommand => _NextPageCommand
        ??= new DelegateCommand(OnNextPageCommandExecuted)
            .ObservesCanExecute(() => IsNextPageExists);

    private async void OnNextPageCommandExecuted() {
        await WrapInLoadingContext(async () => {
            CurrentPageData = await coreService.GetNextPageAsync(default);
            RefreshPage();
            OnPageChanged();
        });
    }

    #endregion

    #region Command PrevPage - Команда перейти на предыдущую страницу

    private ICommand _PrevPageCommand;

    /// <summary>Команда - перейти на предыдущую страницу</summary>
    public ICommand PrevPageCommand => _PrevPageCommand
        ??= new DelegateCommand(OnPrevPageCommandExecuted)
            .ObservesCanExecute(() => IsPrevPageExists);

    private async void OnPrevPageCommandExecuted() {
        await WrapInLoadingContext(async () => {
            CurrentPageData = await coreService.GetPrevPageAsync(default);
            RefreshPage();
            OnPageChanged();
        });
    }

    #endregion

    #region Command Like - Команда Лайк

    private ICommand _LikeCommand;

    /// <summary>Команда - "Понравилось"</summary>
    public ICommand LikeCommand => _LikeCommand
        ??= new DelegateCommand(OnLikeCommandExecuted);

    private async void OnLikeCommandExecuted() {
        await WrapInLoadingContext(async () => {
            LikeState likeState = !SelectedEntity.Liked ? LikeState.Like : LikeState.None;
            await coreService.SetLikeStateAsync(SelectedEntity, likeState, default);
            SelectedEntity = Collection.RefreshItem(SelectedEntity);
        });
    }

    #endregion

    #region Command Dislike - Команда Дизлайк

    private ICommand _DislikeCommand;

    /// <summary>Команда - "Заблокировать"</summary>
    public ICommand DislikeCommand => _DislikeCommand
        ??= new DelegateCommand(OnBlockCommandExecuted);
    private async void OnBlockCommandExecuted() {
        await WrapInLoadingContext(async () => {
            LikeState likeState = !SelectedEntity.Disliked ? LikeState.Dislike : LikeState.None;
            await coreService.SetLikeStateAsync(SelectedEntity, likeState, default);
            SelectedEntity = Collection.RefreshItem(SelectedEntity);
        });
    }

    #endregion

    #region Command DownloadEntity - Команда скачать сущность

    private ICommand _DownloadEntityCommand;

    /// <summary>Команда - скачать сущность</summary>
    public ICommand DownloadEntityCommand => _DownloadEntityCommand
        ??= new DelegateCommand<EntityHandler>(OnDownloadEntityCommandExecuted);

    private void OnDownloadEntityCommandExecuted(EntityHandler entity) {
        downloadEvent.Publish(new List<EntityHandler> { entity });
        Notifier.AddInfo("Файл добавлен к загрузке.");
    }

    #endregion

    #region Command DownloadSelectedList - Команда скачать выбранный список сущностей

    private ICommand _DownloadSelectedListCommand;

    /// <summary>Команда - скачать выбранный список сущностей</summary>
    public ICommand DownloadSelectedListCommand => _DownloadSelectedListCommand
        ??= new DelegateCommand<List<EntityHandler>>(OnDownloadSelectedListCommandExecuted);

    private void OnDownloadSelectedListCommandExecuted(List<EntityHandler> entities) {
        downloadEvent.Publish(new List<EntityHandler>(entities));
        Notifier.AddInfo("Файлы добавлены к загрузке.");
    }

    #endregion

    #region Command PlaySelectedList - Команда воспроизвести выбранный список сущностей

    private ICommand _PlaySelectedListCommand;

    /// <summary>Команда - воспроизвести выбранный список сущностей</summary>
    public ICommand PlaySelectedListCommand => _PlaySelectedListCommand
        ??= new DelegateCommand<IEnumerable<EntityHandler>>(OnPlaySelectedListCommandExecuted);

    private async void OnPlaySelectedListCommandExecuted(IEnumerable<EntityHandler> entities) {
        ProgressBarProgress progress = new();
        await WrapInLoadingContext(async () => {
            PlayCancellationButtonVisibility = true;
            OnPropertyChanged(nameof(IsSelectedEntitiesPlaySupports));
            playCancellation = new CancellationTokenSource();
            await coreService.PlayEntitiesAsync(entities, playCancellation.Token, progress);
            PlayCancellationButtonVisibility = false;
            OnPropertyChanged(nameof(IsSelectedEntitiesPlaySupports));
        }, progress, "Скачивание...");
    }

    #endregion

    #region Command CancelPlay - Команда отмена воспроизведения

    private ICommand _CancelPlayCommand;

    /// <summary>Команда - отмена воспроизведения</summary>
    public ICommand CancelPlayCommand => _CancelPlayCommand
        ??= new DelegateCommand(OnCancelPlayCommandExecuted);

    private void OnCancelPlayCommandExecuted() {
        playCancellation?.Cancel();
        playCancellation?.Dispose();
        PlayCancellationButtonVisibility = false;
        OnPropertyChanged(nameof(IsSelectedEntitiesPlaySupports));
    }

    #endregion

    #region Command ChangeSearchFieldVisibility - Команда изменить видимость поля поиска

    private ICommand? _ChangeSearchFieldVisibilityCommand;

    /// <summary>Команда - изменить видимость поля поиска</summary>
    public ICommand ChangeSearchFieldVisibilityCommand => _ChangeSearchFieldVisibilityCommand
        ??= new DelegateCommand(OnChangeSearchFieldVisibilityCommandExecuted);

    private void OnChangeSearchFieldVisibilityCommandExecuted() {
        SearchFieldVisibility = !SearchFieldVisibility;
        if (!SearchFieldVisibility && EntityFilter is not null) {
            EntityFilter = null;
            RefreshFilter();
        }
    }

    #endregion

    #region Command GetSelectedItems - Команда получить выбранные позиции

    private ICommand _GetSelectedItemsCommand;
    private string entityFilter;

    /// <summary>Команда - получить выбранные позиции</summary>
    public ICommand GetSelectedItemsCommand => _GetSelectedItemsCommand
        ??= new DelegateCommand<object>(OnGetSelectedItemsCommandExecuted);

    private void OnGetSelectedItemsCommandExecuted(object entities) {
        if (entities is not IList items || items.Count == 0) {
            return;
        }

        IEnumerable<EntityHandler> collection = items
            .Cast<EntityHandler>();

        SelectedEntities.Clear();
        SelectedEntities.AddRange(collection);
        SelectedEntity = SelectedEntities.Count == 1 ? SelectedEntities.First() : null;
        OnPropertyChanged(nameof(IsSelectedEntitiesDownloadSupports));
        OnPropertyChanged(nameof(IsSelectedEntitiesPlaySupports));
        OnPropertyChanged(nameof(IsSelectedEntitiesOpenSupports));
    }

    #endregion
}
