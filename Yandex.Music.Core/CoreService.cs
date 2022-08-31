using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media.Imaging;
using Yandex.Api;
using Yandex.Api.Logging;
using Yandex.Api.Music.Mobile;
using Yandex.Api.Music.Mobile.Entities;
using Yandex.Api.Music.Queries;
using Yandex.Api.Music.Web;
using Yandex.Api.Music.Web.Entities;
using Yandex.Api.Passport;
using Yandex.Music.Core.Audio;
using Yandex.Music.Core.Audio.Tag;
using Yandex.Music.Core.Crypto;
using Yandex.Music.Core.FilePath;
using Yandex.Music.Core.TaskSchedule;

namespace Yandex.Music.Core;

public class CoreService : IDisposable, INotifyPropertyChanged
{
    private readonly ILogger logger = LoggerService.Create<CoreService>();

    public CoreService(IDataProvider dataProvider) {
        Settings = new CoreServiceSettings();
        this.dataProvider = dataProvider;

        MusicWebApi = new(dataProvider);
        MusicMobileApi = new(dataProvider);
        PassportApi = new(dataProvider);

        AudioTagProvider = new AudioTagProvider();
        FilePathProvider = new FilePathProvider();
        EntityHandlerProvider = new EntityHandlerProvider(this);
        CryptoProvider = new AesCryptoProvider();

        downloadCoverScheduler = new(Settings.MaxDownloadCoversThreadsCount);
        downloadScheduler = new(Settings.MaxDownloadsThreadsCount);
        totalDownloadSpeedMetrics = new TimeshiftMetricTopic(Settings.DownloadSpeedStatisticTime / Settings.DownloadSpeedUpdateTime);

        Task.Run(async () => {
            await StartDownloadTotalSpeedMeasurementsService(cts.Token).ConfigureAwait(false);
        });
    }


    public CoreServiceSettings Settings { get; set; }

    public ICryptoProvider CryptoProvider { get; set; }

    public IDataProvider DataProvider {
        get => dataProvider;
        set {
            dataProvider = value;
            PassportApi.DataProvider = value;
            MusicWebApi.DataProvider = value;
            MusicMobileApi.DataProvider = value;
        }
    }

    public IAudioTagProvider AudioTagProvider { get; set; }

    public IFilePathProvider FilePathProvider { get; set; }

    public IEntityHandlerProvider EntityHandlerProvider { get; set; }


    public YandexPassportApi PassportApi { get; }

    public YandexMusicWebApi MusicWebApi { get; }

    public YandexMusicMobileApi MusicMobileApi { get; }


    public PassportWebAuthData WebAuthData { get; private set; }

    public PassportMobileAuthData MobileAuthData { get; private set; }

    public WebUserData UserData { get; private set; }

    public WebUserLibraryWrapper UserLibrary { get; private set; }


    public List<string> SearchQueriesPages { get; } = new();

    public List<DownloadEntityHandler> DownloadsList { get; } = new();

    public bool HasPrevPage => currentPageIndex > 0;

    public bool HasNextPage => currentPageIndex < SearchQueriesPages.Count - 1;

    public bool AutoStartDownload { get; set; } = true;

    public bool Authorized => WebAuthData != null && MobileAuthData != null && UserData != null;

    public double DownloadBytesInSecond { get; set; }


    private int currentPageIndex = -1;
    private readonly TaskScheduleManager downloadCoverScheduler;
    private readonly TaskScheduleManager downloadScheduler;
    private readonly object createDirectoryLocker = new();
    private IDataProvider dataProvider;
    private readonly TimeshiftMetricTopic totalDownloadSpeedMetrics;
    private long totalDownloadedBytes = 0;
    private readonly CancellationTokenSource cts = new();

    #region  // ===== Авторизация ===== //

    public CoreServiceAuthData CreateAuthData(string login, string password) {
        CoreServiceAuthData coreServiceAuthData = new() {
            SecretKey = CryptoProvider.CreateKey(),
            Login = login
        };
        coreServiceAuthData.EncryptedPassword = CryptoProvider.EncryptString(password, coreServiceAuthData.SecretKey);
        return coreServiceAuthData;
    }

    public async Task<CoreServiceAuthData> SignInAsync(CoreServiceAuthData authData, CancellationToken cancellationToken) {
        string login = authData.Login;
        string password = CryptoProvider.DecryptString(authData.EncryptedPassword, authData.SecretKey);

        CoreServiceAuthData actualAuthData = new() {
            Login = authData.Login,
            EncryptedPassword = authData.EncryptedPassword,
            SecretKey = authData.SecretKey,
        };

        PassportWebAuthData webAuthData = null;
        PassportMobileAuthData mobileAuthData = null;
        WebUserData userData = null;

        // Мобильная авторизация
        Task<PassportMobileAuthData> mobileAuthTask = null;
        if (Settings.UseAuthCache && !string.IsNullOrEmpty(authData.EncryptedMobileAuthDataCache)
                                  && (DateTime.Now - authData.MobileAuthDataCacheCreatedTime.Value).TotalDays < Settings.AuthCacheLifeTimeDays) {

            string mobileAuthDataCache = CryptoProvider.DecryptString(authData.EncryptedMobileAuthDataCache, authData.SecretKey);
            logger.LogDebug("Использование кэша для мобильной авторизации");
            mobileAuthData = PassportMobileAuthData.DeserializeFromJson(mobileAuthDataCache);

            actualAuthData.EncryptedMobileAuthDataCache = authData.EncryptedMobileAuthDataCache;
            actualAuthData.MobileAuthDataCacheCreatedTime = authData.MobileAuthDataCacheCreatedTime;
        }
        else {
            mobileAuthTask = PassportApi.MobileAuthAsync(login, password, cancellationToken);
        }

        // Web-авторизация
        Task<PassportWebAuthData> webAuthTask = null;
        if (Settings.UseAuthCache && !string.IsNullOrEmpty(authData.EncryptedWebAuthDataCache)
                                  && (DateTime.Now - authData.WebAuthDataCacheCreatedTime.Value).TotalDays < Settings.AuthCacheLifeTimeDays) {

            string webAuthDataCache = CryptoProvider.DecryptString(authData.EncryptedWebAuthDataCache, authData.SecretKey);
            logger.LogDebug("Использование кэша для Web авторизации");
            webAuthData = PassportWebAuthData.DeserializeFromJson(webAuthDataCache);

            actualAuthData.EncryptedWebAuthDataCache = authData.EncryptedWebAuthDataCache;
            actualAuthData.WebAuthDataCacheCreatedTime = authData.WebAuthDataCacheCreatedTime;
        }
        else {
            webAuthTask = PassportApi.WebAuthAsync(login, password, cancellationToken);
        }

        if (webAuthTask != null) {
            logger.LogDebug("Web авторизация через логин/пароль");
            webAuthData = await webAuthTask.ConfigureAwait(false);

            actualAuthData.EncryptedWebAuthDataCache = CryptoProvider.EncryptString(webAuthData.SerializeToJson(), actualAuthData.SecretKey);
            actualAuthData.WebAuthDataCacheCreatedTime = DateTime.Now;
        }

        userData = await GetUserData(webAuthData, cancellationToken).ConfigureAwait(false);

        if (mobileAuthTask != null) {
            logger.LogDebug("Мобильная авторизация через логин/пароль");
            mobileAuthData = await mobileAuthTask.ConfigureAwait(false);

            actualAuthData.EncryptedMobileAuthDataCache = CryptoProvider.EncryptString(mobileAuthData.SerializeToJson(), actualAuthData.SecretKey);
            actualAuthData.MobileAuthDataCacheCreatedTime = DateTime.Now;
        }

        WebAuthData = webAuthData;
        MobileAuthData = mobileAuthData;
        UserData = userData;
        UserLibrary = new WebUserLibraryWrapper(userData.LibraryData);

        return actualAuthData;
    }

    private async Task<WebUserData> GetUserData(PassportWebAuthData webAuthData, CancellationToken cancellationToken) {
        // Получение пользовательских данных
        logger.LogDebug("Получение пользовательских данных");
        WebUserData userData = await MusicWebApi.GetUserDataAsync(webAuthData, cancellationToken).ConfigureAwait(false);
        Validate.IsTrue(userData?.AuthData?.User != null,
            () => new YandexPassportApiAuthorizationException());
        return userData;
    }

    public void SignOut() {
        WebAuthData = null;
        MobileAuthData = null;
        UserData = null;
        UserLibrary = null;
    }

    #endregion

    #region //===== Страницы ===== //

    public async Task<PageData> OpenPageDataAsync(string searchQuery, CancellationToken cancellationToken) {
        PageData pageData = await GetPageDataAsyncInternal(searchQuery, cancellationToken);

        currentPageIndex++;
        SearchQueriesPages.RemoveRange(currentPageIndex, SearchQueriesPages.Count - currentPageIndex);
        SearchQueriesPages.Add(pageData.Query);

        return pageData;
    }

    public async Task<PageData> GetPrevPageAsync(CancellationToken cancellationToken) {
        Validate.IsTrue(currentPageIndex > 0,
            () => new InvalidOperationException("Предыдущей страницы не существует."));

        currentPageIndex--;
        PageData pageData = await GetPageDataAsyncInternal(SearchQueriesPages[currentPageIndex], cancellationToken);
        return pageData;
    }

    public async Task<PageData> GetNextPageAsync(CancellationToken cancellationToken) {
        Validate.IsTrue(currentPageIndex < SearchQueriesPages.Count - 1,
            () => new InvalidOperationException("Следующей страницы не существует."));

        currentPageIndex++;
        PageData pageData = await GetPageDataAsyncInternal(SearchQueriesPages[currentPageIndex], cancellationToken);
        return pageData;
    }

    private async Task<PageData> GetPageDataAsyncInternal(string searchQuery, CancellationToken cancellationToken) {
        CheckAuthorize();

        PageData pageData = new() {
            Query = searchQuery
        };

        // Преобразование строки в поисковой запрос для API
        if (!searchQuery.Contains("://")) {
            searchQuery = new MusicSearchQuery() {
                Text = searchQuery,
            };
        }

        IWebMusicEntity entity = await MusicWebApi.GetEntityAsync(searchQuery, WebAuthData, default).ConfigureAwait(false);
        pageData.MainEntity = EntityHandlerProvider.GetEntityHandler(entity);
        Validate.NotNull(pageData.MainEntity, () => new UnknownHandlerException("Не найден обработчик для страницы."));

        pageData.MainEntity.Query = searchQuery;

        // Обновление пользовательской библиотеки при каждой загрузке страницы (данные могли поменяться извне)
        UserData = await GetUserData(WebAuthData, cancellationToken).ConfigureAwait(false);
        UserLibrary = new WebUserLibraryWrapper(UserData.LibraryData);

        List<IWebMusicEntity> ribbon = await pageData.MainEntity.GetRibbonAsync(cancellationToken).ConfigureAwait(false);
        foreach (IWebMusicEntity ribbonEntity in ribbon) {
            EntityHandler ribbonEntityHandler = EntityHandlerProvider.GetEntityHandler(ribbonEntity);
            ribbonEntityHandler.Query = ribbonEntityHandler.GetUrl();
            pageData.Ribbon.Add(ribbonEntityHandler);
        }

        // Отмена имеющихся задач на загрузку обложек
        downloadCoverScheduler.CancelAll();

        // Асинхронная загрузка оболочки основной сущности
        if (Settings.LoadEntityCover && pageData.MainEntity.Cover == null && pageData.MainEntity.SupportCover) {
            downloadCoverScheduler.Add(async (token) => {
                pageData.MainEntity.Cover = await GetBitmapImageAsync(pageData.MainEntity, token).ConfigureAwait(false);
            });
        }

        foreach (EntityHandler ribbonEntity in pageData.Ribbon) {
            // Установка информации об имеющихся лайках/блокировках из пользовательской библиотеки
            ribbonEntity.UpdateLikeStateFromLibrary();

            // Асинхронная загрузка обложек
            if (Settings.LoadEntityCover && ribbonEntity.Cover == null && ribbonEntity.SupportCover) {
                downloadCoverScheduler.Add(async (token) => {
                    ribbonEntity.Cover = await GetBitmapImageAsync(ribbonEntity, token).ConfigureAwait(false);
                });
            }
        }

        return pageData;
    }

    #endregion

    public async Task PlayEntitiesAsync(IEnumerable<EntityHandler> entities, CancellationToken cancellationToken, IProgress<KeyValuePair<int, int>> progress) {
        List<StartDownloadInfo> downloadList = new();
        List<Task<WebDownloadTrackData>> downloadTaskList = new();
        foreach (EntityHandler entity in entities) {
            if (entity.SupportDownload) {
                List<StartDownloadInfo> downloadItems = await entity.GetStartDownloadInfoAsync(cancellationToken).ConfigureAwait(false);
                foreach (StartDownloadInfo downloadItem in downloadItems) {
                    Task<WebDownloadTrackData> task = MusicWebApi.GetDownloadTrackDataAsync(
                        MusicTrackQuery.ByEntity(downloadItem.Track),
                        WebAuthData, cancellationToken);
                    downloadTaskList.Add(task);
                    downloadList.Add(downloadItem);
                }
            }
        }

        List<WebDownloadTrackData> trackDataList = new();
        progress?.Report(new KeyValuePair<int, int>(0, downloadTaskList.Count));
        for (int i = 0; i < downloadTaskList.Count; i++) {
            Task<WebDownloadTrackData> task = downloadTaskList[i];
            try {
                trackDataList.Add(await task.ConfigureAwait(false));
            }
            catch (Exception ex) {
                WebTrack track = downloadList[i].Track;
                logger.LogError(ex, $"Не удалось получить ссылку для \"{track.Title}\" Id={track.Id}:{track.Albums.First().Id}");
            }
            progress?.Report(new KeyValuePair<int, int>(i + 1, downloadTaskList.Count));
        }

        List<M3uTrack> m3uList = new();
        for (int i = 0; i < trackDataList.Count; i++) {
            EntityHandler track = EntityHandlerProvider.GetEntityHandler(downloadList[i].Track);
            WebDownloadTrackData data = trackDataList[i];

            int duration = (int)track.Duration.Value.TotalSeconds;
            if (Math.Truncate(track.Duration.Value.TotalSeconds) != track.Duration.Value.TotalSeconds) {
                duration++;
            }

            m3uList.Add(new M3uTrack {
                Title = $"{track.SecondTitle} - {track.Title}",
                Duration = duration,
                Path = data.DownloadUrl,
            });
        }

        string m3uPath;
        try {
            string m3uFileContent = M3uTrack.ToString(m3uList);
            m3uPath = $"{Settings.DownloadTempDirectoryPath}/{Path.GetRandomFileName()}.m3u";
            m3uPath = Path.GetFullPath(m3uPath);
            File.WriteAllText(m3uPath, m3uFileContent, Encoding.UTF8);
        }
        catch (Exception ex) {
            Exception exception = new("Не удалось сформировать M3U-плейлист.", ex);
            logger.LogError(ex, exception.Message);
            throw exception;
        }

        try {
            Process.Start(new ProcessStartInfo() {
                FileName = m3uPath,
                UseShellExecute = true,
            });
        }
        catch (Exception ex) {
            Exception exception = new("Не удалось открыть M3U-плейлист.", ex);
            logger.LogError(ex, exception.Message);
            throw exception;
        }
    }

    #region // ===== Изображения ===== //

    public async Task<byte[]> GetImageBytesAsync(EntityHandler handler, CancellationToken cancellationToken) {
        return await GetImageBytesAsync(handler.CoverUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<byte[]> GetImageBytesAsync(string encodedCoverUri, CancellationToken cancellationToken) {
        if (string.IsNullOrEmpty(encodedCoverUri)) {
            return null;
        }
        string coverUri = new MusicCoverQuery(encodedCoverUri, Settings.CoverSizePx, Settings.CoverSizePx);
        byte[] imageBytes = await DataProvider.GetBytesAsync(new RequestData {
            RequestUrl = coverUri,
            AuthData = WebAuthData,
            SupportCache = true,
        }, cancellationToken).ConfigureAwait(false);
        return imageBytes;
    }

    public async Task<BitmapImage> GetBitmapImageAsync(EntityHandler handler, CancellationToken cancellationToken) {
        byte[] imageBytes = await GetImageBytesAsync(handler, cancellationToken).ConfigureAwait(false);
        if (imageBytes == null) {
            return null;
        }
        BitmapImage image = new();
        using (MemoryStream mem = new(imageBytes)) {
            mem.Position = 0;
            cancellationToken.ThrowIfCancellationRequested();
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = mem;
            cancellationToken.ThrowIfCancellationRequested();
            image.EndInit();
        }
        image.Freeze();
        return image;
    }


    #endregion

    #region // ===== Лайки / Блокировки сущностей ===== //

    public async Task SetLikeStateAsync(EntityHandler entity, LikeState state, CancellationToken cancellationToken) {
        await entity.SetLikeStateAsync(state, cancellationToken);
    }

    #endregion

    #region // ===== Работа с загрузками ===== //

    public async Task AddDownloadsAsync(IEnumerable<EntityHandler> entities, CancellationToken cancellationToken) {
        List<DownloadEntityHandler> downloadEntityHandlers = new();
        foreach (EntityHandler entity in entities) {
            if (entity.SupportDownload) {

                List<StartDownloadInfo> downloadItems = await entity.GetStartDownloadInfoAsync(cancellationToken).ConfigureAwait(false);
                foreach (StartDownloadInfo downloadItem in downloadItems) {

                    DownloadEntityHandler downloadEntityHandler = new() {
                        StartDownloadInfo = downloadItem,
                        TrackEntity = EntityHandlerProvider.GetEntityHandler(downloadItem.Track)
                    };
                    if (downloadItem.ParentEntity != null) {
                        downloadEntityHandler.ParentEntity = EntityHandlerProvider.GetEntityHandler(downloadItem.ParentEntity);
                    }
                    downloadEntityHandlers.Add(downloadEntityHandler);

                    TaskScheduleInfo taskScheduleInfo = downloadScheduler.Add(async (token) => {
                        await StartDownloadAsync(downloadEntityHandler, token).ConfigureAwait(false);
                    });
                    downloadEntityHandler.DownloadTaskInfo = taskScheduleInfo;
                }
            }
        }

        lock (DownloadsList) {
            DownloadsList.AddRange(downloadEntityHandlers);
        }
    }

    public async Task StopDownloadsAsync(IEnumerable<DownloadEntityHandler> entities) {
        List<Task> stoppedTasks = new(entities.Count());
        foreach (DownloadEntityHandler entity in entities) {
            stoppedTasks.Add(StopDownloadAsync(entity));
        }
        foreach (Task task in stoppedTasks) {
            await task.ConfigureAwait(false);
        }
    }

    public async Task StopAllDownloadAsync() {
        await StopDownloadsAsync(DownloadsList).ConfigureAwait(false);
    }

    public Task ResumeDownloadsAsync(IEnumerable<DownloadEntityHandler> entities) {
        foreach (DownloadEntityHandler entity in entities) {
            if (entity.CanResume) {
                entity.Status = DownloadEntityHandlerStatus.Pending;
                TaskScheduleInfo taskScheduleInfo = downloadScheduler.Add(async (token) =>
                    await StartDownloadAsync(entity, token));
                entity.DownloadTaskInfo = taskScheduleInfo;
            }
        }
        return Task.CompletedTask;
    }

    public async Task ResumeAllDownloadAsync() {
        await ResumeDownloadsAsync(DownloadsList).ConfigureAwait(false);
    }

    public async Task RemoveDownloadsAsync(IEnumerable<DownloadEntityHandler> entities) {
        await StopDownloadsAsync(entities).ConfigureAwait(false);
        lock (DownloadsList) {
            DownloadsList.RemoveAll(x => entities.Contains(x));
        }
    }

    public async Task RemoveAllDownloadsAsync() {
        await StopDownloadsAsync(DownloadsList).ConfigureAwait(false);
        lock (DownloadsList) {
            DownloadsList.Clear();
        }
    }

    public async Task RemoveDownloadsWithFilesAsync(IEnumerable<DownloadEntityHandler> entities) {
        await StopDownloadsAsync(entities).ConfigureAwait(false);
        List<DownloadEntityHandler> deletedEntities = new();
        foreach (DownloadEntityHandler entity in entities) {
            try {
                if (entity.DownloadTempFilePath != null) {
                    File.Delete(entity.DownloadTempFilePath);
                }
                if (entity.DownloadedFilePath != null) {
                    File.Delete(entity.DownloadedFilePath);
                }
                if (entity.TaggedFilePath != null) {
                    File.Delete(entity.TaggedFilePath);
                }
                if (entity.ResultFilePath != null) {
                    File.Delete(entity.ResultFilePath);
                }
                deletedEntities.Add(entity);
            }
            catch (Exception ex) {
                logger.LogError(ex, $"Невозможно удалить все файлы загрузок для \"{entity.TrackEntity.Title}\"");
            }
        }
        lock (DownloadsList) {
            DownloadsList.RemoveAll(x => deletedEntities.Contains(x));
        }
        Validate.IsTrue(entities.Count() == deletedEntities.Count, "Не все выбранные загруженные файлы были удалены.");
    }

    public async Task OpenDownloadFilesAsync(IEnumerable<DownloadEntityHandler> entities) {
        IEnumerable<M3uTrack> m3uList = entities
            .Where(x => x.Status is DownloadEntityHandlerStatus.Finished or DownloadEntityHandlerStatus.ResultFileExists)
            .Select(x => new M3uTrack {
                Path = x.ResultFilePath,
            });

        string m3uFileContent = M3uTrack.ToString(m3uList);
        string m3uPath = $"{Settings.DownloadTempDirectoryPath}/{Path.GetRandomFileName()}.m3u";
        m3uPath = Path.GetFullPath(m3uPath);
        File.WriteAllText(m3uPath, m3uFileContent, Encoding.UTF8);

        Process.Start(new ProcessStartInfo() {
            FileName = m3uPath,
            UseShellExecute = true,
        });
    }

    public Task OpenDownloadFilesInFoldersAsync(IEnumerable<DownloadEntityHandler> entities) {
        if (entities.Count() > 1) {
            // Открытие директорий для выбранных загрузок
            List<string> folders = entities
                .Where(x => x.Status is DownloadEntityHandlerStatus.Finished or DownloadEntityHandlerStatus.ResultFileExists)
                .Select(x => Path.GetDirectoryName(x.ResultFilePath))
                .Distinct()
                .ToList();
            Validate.NotEmpty(folders, "Отсутствуют завершённые загрузки.");
            foreach (string folder in folders) {
                try {
                    Process.Start("explorer", folder);
                }
                catch (Exception ex) {
                    Exception exception = new($"Не удалось открыть директорию \"{folder}\" в Проводнике.", ex);
                    logger.LogError(exception, exception.Message);
                    Task.FromException(exception);
                }
            }
        }
        else if (entities.Count() == 1) {
            DownloadEntityHandler entity = entities.First();
            Validate.IsTrue(entity.Status is DownloadEntityHandlerStatus.Finished or DownloadEntityHandlerStatus.ResultFileExists,
                "Загрузка не завершена.");
            try {
                Process.Start("explorer", $"/select, \"{entity.ResultFilePath}\"");
            }
            catch (Exception ex) {
                Exception exception = new($"Не удалось открыть директорию с файлом \"{entity.ResultFilePath}\" в Проводнике.", ex);
                logger.LogError(exception, exception.Message);
                Task.FromException(exception);
            }
        }
        else {
            Task.FromException(new ArgumentException("Не выбран список загрузок."));
        }
        return Task.CompletedTask;
    }

    private async Task StartDownloadAsync(DownloadEntityHandler handler, CancellationToken cancellationToken) {
        try {
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation($"Загрузка трека '{handler.TrackEntity.Title}'");
            handler.Status = DownloadEntityHandlerStatus.Starting;
            handler.DownloadLength = 0;
            handler.DownloadPosition = 0;
            handler.ErrorMessage = null;

            logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': получение ссылки для скачивания");
            handler.Status = DownloadEntityHandlerStatus.GetTrackInfo;
            MobileTrackDownloadData trackDownloadData = await GetMobileTrackDownloadDataAsync(handler, cancellationToken).ConfigureAwait(false);

            logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': получение шаблона пути к загружаемому треку");
            string pathTemplate = handler.StartDownloadInfo.ParentEntity == null
                ? Settings.DownloadTrackFilePath
                : handler.StartDownloadInfo.ParentEntity is WebAlbum
                    ? Settings.DownloadAlbumTrackFilePath
                    : handler.StartDownloadInfo.ParentEntity is WebPlaylist
                        ? Settings.DownloadPlaylistTrackFilePath
                        : throw new Exception("Не найдено подходящего обработчика для формирования пути к загружаемому файлу");

            logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': Получение преобразованного пути для сохранения файла");
            FilePathCreationData filePathCreationData = new() {
                StartDownloadInfo = handler.StartDownloadInfo,
                MobileTrackDownloadData = trackDownloadData,
            };
            FilePathList resultFilePath = FilePathProvider.GetFilePathList(pathTemplate, filePathCreationData);

            logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': формирование полных путей для всех этапов загрузки");
            string tempFileName = FilePathProvider.GetTempFileName(filePathCreationData);
            string fileExtension = trackDownloadData.Codec switch {
                "mp3" => ".mp3",
                "aac" => ".m4a",
                _ => $".{trackDownloadData.Codec}"
            };

            handler.DownloadTempFilePath = Path.GetFullPath(Path.Combine(
                Settings.DownloadTempDirectoryPath,
                tempFileName.ToString()
                + fileExtension
                + ".tmp"));

            handler.DownloadedFilePath = Path.GetFullPath(Path.Combine(
                Settings.DownloadTempDirectoryPath,
                tempFileName.ToString()
                + fileExtension));

            handler.TaggedFilePath = Path.GetFullPath(Path.Combine(
                Settings.DownloadTempDirectoryPath,
                tempFileName.ToString()
                + ".tagged"
                + fileExtension));

            handler.ResultFilePath = Path.GetFullPath(Path.Combine(
                                                          Settings.DownloadResultDirectoryPath,
                                                          resultFilePath.ToString())
                                                      + fileExtension);

            bool resultFileExists = File.Exists(handler.ResultFilePath) && new FileInfo(handler.ResultFilePath).Length > 0;
            bool taggedFileExists = File.Exists(handler.TaggedFilePath) && new FileInfo(handler.TaggedFilePath).Length > 0;
            bool downloadedFileExists = File.Exists(handler.DownloadedFilePath) && new FileInfo(handler.DownloadedFilePath).Length > 0;

            // 1) Скачивание необработанного файла с Яндекса
            cancellationToken.ThrowIfCancellationRequested();
            if (!resultFileExists && !taggedFileExists && !downloadedFileExists) {
                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': процесс загрузки");

                handler.Status = DownloadEntityHandlerStatus.Downloading;
                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': получение прямой ссылки для скачивания");
                string downloadUri = await MusicMobileApi.GetDownloadTrackUri(trackDownloadData, MobileAuthData, cancellationToken).ConfigureAwait(false);

                using HttpClient httpClient = DataProvider.GetHttpClientFactory(RequestType.Mobile)
                    .CreateHttpClient(MobileAuthData, null);
                using HttpRequestMessage request = new() {
                    RequestUri = new Uri(downloadUri),
                };

                long startDownloadPosition = 0;
                if (File.Exists(handler.DownloadTempFilePath)) {
                    startDownloadPosition = new FileInfo(handler.DownloadTempFilePath).Length;
                    request.Headers.Range = new RangeHeaderValue(startDownloadPosition, null);
                    handler.DownloadPosition = startDownloadPosition;
                    logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': дозагрузка с момента остановки предыдущей загрузки [{startDownloadPosition}]");
                }

                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': отправка запроса на скачивание файла");
                using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) {
                    if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable) {
                        logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': файл загружен в полном объёме");
                        handler.DownloadLength = startDownloadPosition;
                    }
                    else {
                        throw new Exception(response.StatusCode.ToString());
                    }
                }
                else {
                    logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': осталось загрузить '{response.Content.Headers.ContentLength}' байт");

                    // Получение размера скачиваемого файла
                    handler.DownloadLength = response.Content.Headers.ContentLength + startDownloadPosition;

                    // Создание временной папки для хранения загружаемых необработанных файлов
                    string tempDirectory = Path.GetDirectoryName(handler.DownloadTempFilePath);
                    SafeCreateDirectory(tempDirectory, null);

                    logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': получение Stream");
                    await using Stream netStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': получение Stream - ok");

                    await using Stream fileStream = new FileStream(handler.DownloadTempFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                    fileStream.Seek(0, SeekOrigin.End);

                    byte[] buffer = new byte[256 * 1024];
                    int readedBytes;
                    while ((readedBytes = await netStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0) {
                        cancellationToken.ThrowIfCancellationRequested();
                        //logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': получено '{readedBytes}' байт");

                        await fileStream.WriteAsync(buffer, 0, readedBytes, cancellationToken).ConfigureAwait(false);

                        handler.DownloadPosition += readedBytes;

                        Interlocked.Add(ref totalDownloadedBytes, readedBytes);
                    }
                    await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }

                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': перемещение из '{handler.DownloadTempFilePath}' в '{handler.DownloadedFilePath}'");
                File.Move(handler.DownloadTempFilePath, handler.DownloadedFilePath);

                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': процесс загрузки - ok");
            }

            // 2) Тэгирование необработанного аудиофайла
            cancellationToken.ThrowIfCancellationRequested();
            if (!resultFileExists && !taggedFileExists) {
                handler.Status = DownloadEntityHandlerStatus.Tagging;
                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': процесс тэгирования");

                AudioTagData audioTagData = await GetAudioTagDataAsync(handler.StartDownloadInfo, cancellationToken).ConfigureAwait(false);
                try {
                    File.Delete(handler.TaggedFilePath);
                    File.Copy(handler.DownloadedFilePath, handler.TaggedFilePath);
                    await AudioTagProvider.SetAudioTagsAsync(handler.TaggedFilePath, audioTagData, cancellationToken).ConfigureAwait(false);
                    File.Delete(handler.DownloadedFilePath);
                }
                catch {
                    File.Delete(handler.TaggedFilePath);
                    throw;
                }
                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': процесс тэгирования - ok");
            }

            // 3) Сохранение тэгированного аудиофайла в конечную директорию
            cancellationToken.ThrowIfCancellationRequested();
            if (!File.Exists(handler.ResultFilePath)) {
                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': процесс сохранения конечного файла");

                // Создание структуры директорий для сохранения готового файла
                string resultPath = Path.GetFullPath(Settings.DownloadResultDirectoryPath);
                SafeCreateDirectory(resultPath, null);
                for (int i = 0; i < resultFilePath.Count - 1; i++) {
                    FilePathSegment resultFilePathSegment = resultFilePath[i];
                    resultPath = Path.Combine(resultPath, resultFilePathSegment.Path);
                    byte[] directoryImage = null;
                    if (resultFilePathSegment.CoverUri != null) {
                        directoryImage = await GetImageBytesAsync(resultFilePathSegment.CoverUri, cancellationToken).ConfigureAwait(false);
                    }
                    SafeCreateDirectory(resultPath, directoryImage);
                }

                File.Move(handler.TaggedFilePath, handler.ResultFilePath);
                logger.LogTrace($"Загрузка трека '{handler.TrackEntity.Title}': процесс сохранения конечного файла - ok");

                handler.Status = DownloadEntityHandlerStatus.Finished;
            }
            else {
                handler.DownloadPosition = handler.DownloadLength =
                    new FileInfo(handler.ResultFilePath).Length;
                handler.Status = DownloadEntityHandlerStatus.ResultFileExists;
            }

            logger.LogInformation($"Загрузка трека '{handler.TrackEntity.Title}' - ok");
        }
        catch (OperationCanceledException) {
            logger.LogInformation($"Загрузка трека '{handler.TrackEntity.Title}' - отмена");

            handler.Status = DownloadEntityHandlerStatus.Stopped;
        }
        catch (Exception ex) {
            logger.LogInformation($"Загрузка трека '{handler.TrackEntity.Title}' - ошибка");
            handler.Status = DownloadEntityHandlerStatus.Error;
            handler.ErrorMessage = ex.Message;
        }
    }

    private async Task StopDownloadAsync(DownloadEntityHandler handler) {
        if (handler.CanStop) {
            DownloadEntityHandlerStatus prevStatus = handler.Status;
            handler.Status = DownloadEntityHandlerStatus.Stopping;
            handler.DownloadTaskInfo?.Cancel();
            if (handler.DownloadTaskInfo != null && handler.DownloadTaskInfo.Task.Status == TaskStatus.Running) {
                await handler.DownloadTaskInfo.Task.ConfigureAwait(false);
            }
            if (prevStatus == DownloadEntityHandlerStatus.Pending) {
                // В случае, если закачка была поставлена в очередь и ещё не запускалась, статус применяется принудительно
                handler.Status = DownloadEntityHandlerStatus.Stopped;
            }
        }
    }

    private async Task<MobileTrackDownloadData> GetMobileTrackDownloadDataAsync(DownloadEntityHandler handler, CancellationToken cancellationToken) {
        MobileTrackDownloadData[] downloadDataArray = await MusicMobileApi
            .GetDownloadTrackDataAsync(MusicTrackQuery.ByEntity(handler.TrackEntity.Entity as WebTrack), MobileAuthData, cancellationToken)
            .ConfigureAwait(false);
        MobileTrackDownloadData trackDownloadData = SelectDownloadFormat(downloadDataArray);
        return trackDownloadData;
    }

    private MobileTrackDownloadData SelectDownloadFormat(MobileTrackDownloadData[] downloadDataArray) {
        string format = Settings.PreferredAudioCodec switch {
            AudioCodec.MP3 => "mp3",
            AudioCodec.AAC => "aac",
            _ => throw new Exception("Выбран неизвестный тип кодека."),
        };

        int bitrate = Settings.PreferredAudioBitrate switch {
            AudioBitrate.B320 => 320,
            AudioBitrate.B192 => 192,
            AudioBitrate.B128 => 128,
            _ => throw new Exception("Выбран неизвестный битрейт."),
        };

        IEnumerable<MobileTrackDownloadData> selected = downloadDataArray;

        // Выбор кодека
        IEnumerable<MobileTrackDownloadData> selectedByCodec = selected
            .Where(x => x.Codec == format);
        if (selectedByCodec.Count() > 0) {
            selected = selectedByCodec;
        }

        // Выбор качества
        selected = selected
            .OrderByDescending(x => x.Bitrate);
        IEnumerable<MobileTrackDownloadData> selectedByQuality = selected
            .Where(x => x.Bitrate <= bitrate);
        return selectedByQuality.Count() > 0
            ? selectedByQuality.FirstOrDefault()
            : selected.FirstOrDefault();
    }

    private async Task<AudioTagData> GetAudioTagDataAsync(StartDownloadInfo startDownloadInfo, CancellationToken cancellationToken) {
        AudioTagData audioTagData = new();

        MusicTrackQuery trackQuery = MusicTrackQuery.ByEntity(startDownloadInfo.Track);
        WebTrackData trackData = await MusicWebApi.GetTrackAsync(
            trackQuery, WebAuthData, CancellationToken.None).ConfigureAwait(false);

        WebGenres genres = await MusicWebApi.GetGenresAsync(WebAuthData, CancellationToken.None).ConfigureAwait(false);

        //!!! ВЕРОЯТНО НУЖНО ТОЛЬКО ПЕРВЫЙ АЛЬБОМ (Imagine dragons - demons - 121 альбом)
        string[] trackGenres = trackData.Track.Albums
            .Select(x => x.Genre)
            .Where(x => x != null)
            .Distinct()
            .Select(x => genres.Titles.GetValueOrDefault(x))
            .ToArray();

        audioTagData.Picture = await GetImageBytesAsync(trackData.Track.CoverUri ?? trackData.Track.OgImage, cancellationToken).ConfigureAwait(false);
        audioTagData.TrackNumber = (uint)startDownloadInfo.Number;
        audioTagData.TrackCount = (uint)startDownloadInfo.TotalCount;
        audioTagData.Title = trackData.Track.Title;
        audioTagData.Album = trackData.Track.Albums.FirstOrDefault()?.Title;
        audioTagData.Lyrics = trackData.Lyric?.FirstOrDefault()?.FullLyrics;
        audioTagData.Url = MusicWebApi.Settings.MainUrl + trackQuery.GetString();
        audioTagData.DiskNumber = (uint?)startDownloadInfo.AlbumVolumeNumber;
        audioTagData.DiskCount = (uint?)startDownloadInfo.AlbumVolumeTrackTotalCount;
        audioTagData.Artists = trackData.Artists.Select(x => x.Name).ToArray();
        audioTagData.Genres = trackGenres;
        audioTagData.Year = (uint?)trackData.Track.Albums?.FirstOrDefault()?.Year;
        return audioTagData;
    }


    #endregion

    #region  // ===== Прочее ===== //

    public void ApplySettings() {
        downloadCoverScheduler.MaxParallelTasksCount = Settings.MaxDownloadCoversThreadsCount;
        downloadScheduler.MaxParallelTasksCount = Settings.MaxDownloadsThreadsCount;
        totalDownloadSpeedMetrics.Capacity = Settings.DownloadSpeedStatisticTime / Settings.DownloadSpeedUpdateTime;
    }

    public void Dispose() {
        cts.Cancel();
        cts.Dispose();
    }

    #endregion



    private async Task StartDownloadTotalSpeedMeasurementsService(CancellationToken cancellationToken) {
        logger.LogDebug("Запущен фоновый процесс измерения скорости скачивания из Интернета");
        try {
            while (true) {
                await Task.Delay(Settings.DownloadSpeedUpdateTime, cancellationToken).ConfigureAwait(false);
                DateTime currentDate = DateTime.Now;
                long currentDownloadedBytes = totalDownloadedBytes;

                //logger.LogTrace($"Скачано байт: {currentDownloadedBytes}");
                totalDownloadSpeedMetrics.AddAccumulativeMetric(currentDate, currentDownloadedBytes);
                DownloadBytesInSecond = totalDownloadSpeedMetrics.GetAverage(1000);
                //if (DownloadBytesInSecond > 0) {
                //    logger.LogTrace($"Общая скорость загрузки (Кб/сек): {DownloadBytesInSecond / 1024}");
                //}
            }
        }
        catch (Exception ex) {
            logger.LogDebug(ex, "Остановлен фоновый процесс измерения скорости скачивания из Интернета");
        }
    }

    /// <summary>
    /// Потокобезопасное создание директории.
    /// </summary>
    /// <param name="path">Путь к создаваемой директории.</param>
    /// <param name="directoryImageBytes">Изображение в качестве фоновой обложки к директории.</param>
    private void SafeCreateDirectory(string path, byte[] directoryImageBytes) {
        lock (createDirectoryLocker) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            if (directoryImageBytes != null) {
                string imagePath = Path.Combine(path, "folder.jpg");
                if (!File.Exists(imagePath)) {
                    File.WriteAllBytes(imagePath, directoryImageBytes);
                    if (Settings.HideFolderCover) {
                        File.SetAttributes(imagePath, FileAttributes.Hidden);
                    }
                }
            }
        }
    }

    private void CheckAuthorize() {
        Validate.IsTrue(Authorized,
            () => new YandexPassportApiAuthorizationException("Авторизация не выполнена."));
    }


    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string prop = "") {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
