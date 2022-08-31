using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Yandex.Api;
using Yandex.Api.Music.Mobile.Entities;
using Yandex.Tests.Internal;

namespace Yandex.Tests.Api;

[TestClass]
public class YandexMobileMusicTests
{
    [TestMethod]
    public async Task DownloadTrack() {
        string trackUrl = "https://music.yandex.ru/album/38065/track/133060";

        MobileTrackDownloadData[] downloadData = await TestFactory.GetMusicMobileApi()
            .GetDownloadTrackDataAsync(trackUrl, TestFactory.GetMobileAuthData(), default);

        Assert.AreEqual("aac", downloadData[1].Codec);
        Assert.AreEqual(128, downloadData[1].Bitrate);

        string downloadUri = await TestFactory.GetMusicMobileApi()
            .GetDownloadTrackUri(downloadData[1], TestFactory.GetMobileAuthData(), default);

        byte[] trackData = await TestFactory.DataProvider.GetBytesAsync(new RequestData() {
            RequestUrl = downloadUri,
            RequestType = RequestType.Mobile,
            AuthData = TestFactory.GetMobileAuthData(),
        }, default);

        Assert.AreEqual(3416460, trackData.Length);
    }

    [TestMethod]
    public async Task GetAlbum() {
        MobileAlbum album = await TestFactory.GetMusicMobileApi()
            .GetAlbumAsync("https://music.yandex.ru/album/13117", TestFactory.GetMobileAuthData(), default);

        Assert.AreEqual("Comatose", album.Title);
        Assert.AreEqual("Skillet", album.Artists[0].Name);
    }

    [TestMethod]
    public async Task GetPlaylistById() {
        MobilePlaylist playlist = await TestFactory.GetMusicMobileApi()
            .GetPlaylistByIdAsync("103372440", "2573", TestFactory.GetMobileAuthData(), default);

        Assert.AreEqual("Русский рок: новички", playlist.Title);
    }
}
