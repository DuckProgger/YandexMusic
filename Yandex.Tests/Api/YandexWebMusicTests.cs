using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Yandex.Api;
using Yandex.Api.Music.Queries;
using Yandex.Api.Music.Web.Entities;
using Yandex.Tests.Internal;

namespace Yandex.Tests.Api;

[TestClass]
public class YandexWebMusicTests
{
    [TestMethod]
    public async Task GetUserInfo() {
        WebUserData userData = await TestFactory.GetMusicWebApi()
            .GetUserDataAsync(TestFactory.GetWebAuthData(), default);

        Assert.IsNotNull(userData?.AuthData?.User?.Sign, "Отсутствуют данные авторизации.");
    }

    [TestMethod]
    public async Task GetArtistInfo() {
        string artistUrl = "https://music.yandex.ru/artist/169649";
        WebArtistData artistInfo = await TestFactory.GetMusicWebApi()
            .GetArtistAsync(artistUrl, TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("Лолита", artistInfo.Artist.Name);
        Assert.AreEqual("Лолита Марковна Горелик", artistInfo.Artist.FullNames[0]);
    }

    [TestMethod]
    public async Task GetAlbumInfo() {
        string albumUrl = "https://music.yandex.ru/album/5750816";
        WebAlbum album = await TestFactory.GetMusicWebApi()
            .GetAlbumAsync(albumUrl, TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("...And Then There Was X", album.Title);
        Assert.AreEqual("...And Then There Was X", album.Title);
        Assert.AreEqual("DMX", album.Artists[0].Name);
        Assert.AreEqual("The Kennel", album.Tracks[0].Title);
        Assert.AreEqual(18, album.Tracks.Length);
        Assert.AreEqual(18, album.TrackCount);
    }

    [TestMethod]
    public async Task GetAlbumsInfo() {
        MusicAlbumQuery[] albumUrls = {
            "https://music.yandex.ru/album/17161822",
            "https://music.yandex.ru/album/5379306",
            "https://music.yandex.ru/album/4364659",
        };
        WebAlbum[] albums = await TestFactory.GetMusicWebApi()
            .GetAlbumsAsync(albumUrls, TestFactory.GetWebAuthData(), default);

        Assert.AreEqual(3, albums.Length);
        Assert.AreEqual("Swerve", albums[0].Title);
        Assert.AreEqual("Last Resort", albums[1].Title);
        Assert.AreEqual("Crooked Teeth", albums[2].Title);
    }

    [TestMethod]
    public async Task GetPlaylistInfo() {
        string trackUrl = "https://music.yandex.ru/users/music-blog/playlists/2547";
        WebPlaylist playlist = await TestFactory.GetMusicWebApi()
            .GetPlaylistAsync(trackUrl, TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("Для вечеринки", playlist.Title);
        Assert.AreEqual("Устраиваете шумный вечер с друзьями или танцуете для себя? Собрали треки, под которые никто не усидит на месте.", playlist.Description);
    }

    [TestMethod]
    public async Task GetPlaylistsInfo() {
        string[] playlistIds = {
            "103372440:1175",
            "405705740:1029",
            "103372440:2465",
        };
        WebPlaylist[] playlists = await TestFactory.GetMusicWebApi()
            .GetPlaylistsAsync(playlistIds, TestFactory.GetWebAuthData(), default);

        Assert.AreEqual(3, playlists.Length);
        Assert.AreEqual("Громкие новинки месяца", playlists[0].Title);
        Assert.AreEqual("Авторская песня", playlists[1].Title);
        Assert.AreEqual("Громкие новинки: рок", playlists[2].Title);
    }

    [TestMethod]
    public async Task GetTrackInfo() {
        string trackUrl = "https://music.yandex.ru/album/38065/track/133060";
        WebTrackData trackInfo = await TestFactory.GetMusicWebApi()
            .GetTrackAsync(trackUrl, TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("Numb / Encore", trackInfo.Track.Title);
        Assert.AreEqual("Linkin Park", trackInfo.Track.Artists[0].Name);
        Assert.AreEqual("Collision Course", trackInfo.Track.Albums[0].Title);
        Assert.AreEqual(1, trackInfo.Lyric.Length);
    }

    [TestMethod]
    public async Task GetTracksInfo() {
        MusicTrackQuery[] trackUrls = {
            "https://music.yandex.ru/album/6979954/track/46841582",
            "https://music.yandex.ru/album/7020598/track/44182609",
            "https://music.yandex.ru/album/4364659/track/35150762",
        };
        WebTrack[] tracks = await TestFactory.GetMusicWebApi()
            .GetTracksAsync(trackUrls, TestFactory.GetWebAuthData(), default);

        Assert.AreEqual(3, tracks.Length);
        Assert.AreEqual("Cross Off", tracks[0].Title);
        Assert.AreEqual("Better Than Life", tracks[1].Title);
        Assert.AreEqual("My Medication", tracks[2].Title);
    }

    [TestMethod]
    public async Task GetGenres() {
        WebGenres genres = await TestFactory.GetMusicWebApi()
            .GetGenresAsync(TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(genres.Structure.Length > 0);
        Assert.IsTrue(genres.Titles.Count > 0);
        Assert.IsTrue(genres.CustomTitles.Count > 0);
        Assert.IsTrue(genres.CustomUrls.Count > 0);
    }

    [TestMethod]
    public async Task LikeTrack() {
        string trackUrl = "https://music.yandex.ru/album/4364659/track/35150768";

        WebUserData userData = await TestFactory.GetMusicWebApi()
            .GetUserDataAsync(TestFactory.GetWebAuthData(), default);

        WebStatus status = await TestFactory.GetMusicWebApi()
            .LikeTrackAsync(trackUrl, true, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);

        status = await TestFactory.GetMusicWebApi()
           .LikeTrackAsync(trackUrl, false, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);
    }

    [TestMethod]
    public async Task BlockTrack() {
        string trackUrl = "https://music.yandex.ru/album/20038952/track/97333570";

        WebUserData userData = await TestFactory.GetMusicWebApi()
            .GetUserDataAsync(TestFactory.GetWebAuthData(), default);

        WebStatus status = await TestFactory.GetMusicWebApi()
            .DislikeTrackAsync(trackUrl, true, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);

        status = await TestFactory.GetMusicWebApi()
         .DislikeTrackAsync(trackUrl, false, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);
    }

    [TestMethod]
    public async Task DownloadTrack() {
        string trackUrl = "https://music.yandex.ru/album/38065/track/133060";

        WebDownloadTrackData downloadInfo = await TestFactory.GetMusicWebApi()
            .GetDownloadTrackDataAsync(trackUrl, TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("mp3", downloadInfo.Codec);
        Assert.AreEqual(192, downloadInfo.Bitrate);

        byte[] trackData = await TestFactory.DataProvider.GetBytesAsync(new RequestData() {
            RequestUrl = downloadInfo.DownloadUrl,
            AuthData = TestFactory.GetWebAuthData(),
        }, default);

        Assert.AreEqual(4927738, trackData.Length);
    }

    [TestMethod]
    public async Task GetPageByPlaylistUrl() {
        IWebMusicEntity entity = await TestFactory.GetMusicWebApi()
            .GetEntityAsync("https://music.yandex.ru/users/yamusic-origin/playlists/48293360", TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(entity is WebPlaylist);
        Assert.AreEqual("Плейлист с Алисой", (entity as WebPlaylist).Title);
    }

    [TestMethod]
    public async Task GetPageByArtistUrl() {
        IWebMusicEntity entity = await TestFactory.GetMusicWebApi()
            .GetEntityAsync("https://music.yandex.ru/artist/36762", TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(entity is WebArtistData);
        Assert.AreEqual("Green Day", (entity as WebArtistData).Artist.Name);
    }

    [TestMethod]
    public async Task GetPageByTrackUrl() {
        IWebMusicEntity entity = await TestFactory.GetMusicWebApi()
            .GetEntityAsync("https://music.yandex.ru/album/2390201/track/148345", TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(entity is WebTrackData);
        Assert.AreEqual("Californication", (entity as WebTrackData).Track.Title);
    }

    [TestMethod]
    public async Task GetPageByAlbumUrl() {
        IWebMusicEntity entity = await TestFactory.GetMusicWebApi()
            .GetEntityAsync("https://music.yandex.ru/album/3550565", TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("The Getaway", (entity as WebAlbum).Title);
    }

    [TestMethod]
    public async Task GetSearchResultByArtist() {
        WebSearchResult searchResult = await TestFactory.GetMusicWebApi()
            .GetSearchResultAsync("меладзе", TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("Валерий Меладзе", (searchResult.Best as WebArtist).Name);
    }

    [TestMethod]
    public async Task GetSearchResultByTrack() {
        WebSearchResult searchResult = await TestFactory.GetMusicWebApi()
            .GetSearchResultAsync("Nirvana - Smells Like Teen Spirit", TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("Smells Like Teen Spirit", (searchResult.Best as WebTrack).Title);
    }

    [TestMethod]
    public async Task GetSearchResultByAlbum() {
        WebSearchResult searchResult = await TestFactory.GetMusicWebApi()
            .GetSearchResultAsync("Позови На Движ альбом", TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("Позови На Движ", (searchResult.Best as WebAlbum).Title);
    }

    [TestMethod]
    public async Task GetSearchResultByPlaylist() {
        WebSearchResult searchResult = await TestFactory.GetMusicWebApi()
            .GetSearchResultAsync("акустический соул плейлист", TestFactory.GetWebAuthData(), default);

        Assert.AreEqual("Акустический соул", (searchResult.Best as WebPlaylist).Title);
    }

    [TestMethod]
    public async Task GetHomePage() {
        WebBlockPage homePage = await TestFactory.GetMusicWebApi()
            .GetHomePageDataAsync(TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(homePage.Blocks.Length > 0);
    }

    [TestMethod]
    public async Task GetNewReleasesPage() {
        WebNewReleasesPage newReleasesPage = await TestFactory.GetMusicWebApi()
            .GetNewReleasesPageAsync(TestFactory.GetWebAuthData(), default);
        Assert.IsNotNull(newReleasesPage.NewReleases[0].Title);
        Assert.IsNotNull(newReleasesPage.NewReleases[1].Title);
    }

    [TestMethod]
    public async Task GetChartPage() {
        WebChartPage chartPage = await TestFactory.GetMusicWebApi()
            .GetChartPageAsync(TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(chartPage.ChartPositions.Length > 0);
        Assert.IsTrue(chartPage.OtherChartPlaylists.Length > 0);
    }

    [TestMethod]
    public async Task GetGenresPage() {
        WebGenresPage genresPage = await TestFactory.GetMusicWebApi()
            .GetGenresPageAsync(TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(genresPage.MetaTags.Length > 0);
        Assert.IsTrue(genresPage.Entities.Length > 0);
    }

    [TestMethod]
    public async Task GetNonMusicPage() {
        WebBlockPage nonMusicPage = await TestFactory.GetMusicWebApi()
            .GetNonMusicPageDataAsync(TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(nonMusicPage.Blocks.Length > 0);
    }

    [TestMethod]
    public async Task GetKidsPage() {
        WebBlockPage kidsPage = await TestFactory.GetMusicWebApi()
            .GetKidsPageDataAsync(TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(kidsPage.Blocks.Length > 0);
    }

    [TestMethod]
    public async Task GetUserHistory() {
        WebUserHistory userHistory = await TestFactory.GetMusicWebApi()
            .GetUserHistoryAsync(TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(userHistory.Success);
    }

    [TestMethod]
    public async Task GetTag() {
        WebTag tag = await TestFactory.GetMusicWebApi()
            .GetTagAsync(MusicTagQuery.ByName("top", "new"), TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(tag.Playlists.Length > 0);
        Assert.AreEqual("Популярные плейлисты", tag.Title);
    }

    [TestMethod]
    public async Task GetSuggests() {
        WebSuggests suggests = await TestFactory.GetMusicWebApi()
            .GetSuggestsAsync("Валер", default);
        Assert.AreEqual("ok", suggests.Status);
        Assert.AreEqual("Валерий Меладзе", suggests.Entities[0].Results[0].Artist.Name);
    }

    [TestMethod]
    public async Task GetUserPlaylists() {

        WebUserPlaylists userPlaylists = await TestFactory.GetMusicWebApi()
            .GetUserPlaylistsAsync(TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(userPlaylists.Playlists.Length > 0);
        Assert.AreEqual(userPlaylists.Owner.Login, TestFactory.Configuration.Login);
    }

    [TestMethod]
    public async Task LikePlaylist() {
        WebUserData userData = await TestFactory.GetMusicWebApi()
           .GetUserDataAsync(TestFactory.GetWebAuthData(), default);

        WebPlaylist playlist = await TestFactory.GetMusicWebApi()
            .GetPlaylistAsync("https://music.yandex.ru/users/music-blog/playlists/1011", TestFactory.GetWebAuthData(), default);

        WebStatus status = await TestFactory.GetMusicWebApi()
            .LikePlaylistAsync(playlist, true, userData, TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(status.Success);

        status = await TestFactory.GetMusicWebApi()
            .LikePlaylistAsync(playlist, false, userData, TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(status.Success);
    }

    [TestMethod]
    public async Task LikeAlbum() {
        string albumUrl = "https://music.yandex.ru/album/7091121";

        WebUserData userData = await TestFactory.GetMusicWebApi()
              .GetUserDataAsync(TestFactory.GetWebAuthData(), default);

        WebStatus status = await TestFactory.GetMusicWebApi()
            .LikeAlbumAsync(albumUrl, true, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);

        status = await TestFactory.GetMusicWebApi()
            .LikeAlbumAsync(albumUrl, false, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);
    }

    [TestMethod]
    public async Task LikeArtist() {
        string artistUrl = "https://music.yandex.ru/artist/15691816";

        WebUserData userData = await TestFactory.GetMusicWebApi()
              .GetUserDataAsync(TestFactory.GetWebAuthData(), default);

        WebStatus status = await TestFactory.GetMusicWebApi()
            .LikeArtistAsync(artistUrl, true, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);

        status = await TestFactory.GetMusicWebApi()
            .LikeArtistAsync(artistUrl, false, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);
    }

    [TestMethod]
    public async Task BlockArtist() {
        string artistUrl = "https://music.yandex.ru/artist/10894360";

        WebUserData userData = await TestFactory.GetMusicWebApi()
              .GetUserDataAsync(TestFactory.GetWebAuthData(), default);

        WebStatus status = await TestFactory.GetMusicWebApi()
            .DislikeArtistAsync(artistUrl, true, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);

        status = await TestFactory.GetMusicWebApi()
            .DislikeArtistAsync(artistUrl, false, userData, TestFactory.GetWebAuthData(), default);

        Assert.IsTrue(status.Success);
    }

    [TestMethod]
    public async Task ManageUserPlaylist() {
        string playlistTitle = $"Тестовый плейлист - {DateTime.Now:yyyy.MM.dd.HH.mm.ss}";
        string playlistRenamedTitle = $"Переименованный плейлист - {DateTime.Now:yyyy.MM.dd.HH.mm.ss}";

        WebUserData userData = await TestFactory.GetMusicWebApi()
            .GetUserDataAsync(TestFactory.GetWebAuthData(), default);

        // Создание плейлиста
        WebUserPlaylistStatus createdPlaylist = await TestFactory.GetMusicWebApi()
            .CreateUserPlaylistAsync(playlistTitle, userData, TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(createdPlaylist.Success);
        Assert.AreEqual(playlistTitle, createdPlaylist.Playlist.Title);

        // Переименование плейлиста
        WebUserPlaylistStatus renamedPlaylist = await TestFactory.GetMusicWebApi()
            .RenameUserPlaylistAsync(createdPlaylist.Playlist, playlistRenamedTitle, userData, TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(renamedPlaylist.Success);
        Assert.AreEqual(renamedPlaylist.Playlist.Title, playlistRenamedTitle);

        // Добавление треков в плейлист
        MusicTrackQuery[] tracksUrls = {
            "https://music.yandex.ru/album/18809983/track/93390818",
            "https://music.yandex.ru/album/15213275/track/81848875",
            "https://music.yandex.ru/album/18729695/track/93126179",
            "https://music.yandex.ru/album/22408200/track/104263691",
            "https://music.yandex.ru/album/21215112/track/100864526",
        };
        WebUserPlaylistStatus addTrackPlaylist = await TestFactory.GetMusicWebApi()
            .InsertTracksToUserPlaylistAsync(renamedPlaylist.Playlist, tracksUrls, 0, userData, TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(addTrackPlaylist.Success);
        Assert.AreEqual(5, addTrackPlaylist.Playlist.TrackCount);

        // Удаление треков из плейлиста по выбранным диапазонам
        WebUserPlaylistStatus deleteTrackPlaylist = await TestFactory.GetMusicWebApi()
            .DeleteTracksToUserPlaylistAsync(addTrackPlaylist.Playlist, new Range[] { 1..1, 3..4 }, userData, TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(deleteTrackPlaylist.Success);
        Assert.AreEqual(2, deleteTrackPlaylist.Playlist.TrackCount);

        // Удаление плейлиста
        WebStatus deletePlaylistStatus = await TestFactory.GetMusicWebApi()
            .DeleteUserPlaylistAsync(deleteTrackPlaylist.Playlist, userData, TestFactory.GetWebAuthData(), default);
        Assert.IsTrue(deletePlaylistStatus.Success);
    }

    [TestMethod]
    public async Task GetRecomendedTracksFromPlaylist() {
        WebTrack[] recomendedTracks = await TestFactory.GetMusicWebApi()
              .GetRecomendedTracksFromPlaylistAsync("https://music.yandex.ru/users/yamusic-application-account/playlists/1015", 0, 15, TestFactory.GetWebAuthData(), default);
        // TODO
    }

    [TestMethod]
    public async Task GetGenrePageAsync() {
        WebGenrePage genrePage = await TestFactory.GetMusicWebApi()
            .GetGenrePageAsync("https://music.yandex.ru/genre/%D0%BF%D0%BE%D0%BF/artists?page=2", TestFactory.GetWebAuthData(), default);
        Assert.IsNotNull(genrePage.Artists[0].Name);
    }
}
