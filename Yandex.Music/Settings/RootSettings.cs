using Force.DeepCloner;
using Newtonsoft.Json;
using Yandex.Api.Music.Mobile;
using Yandex.Api.Music.Web;
using Yandex.Api.Passport;
using Yandex.Music.Core;

namespace Yandex.Music.Settings;
internal class RootSettings
{
    [JsonIgnore]
    public const int ActualVersion = 1;

    public int Version { get; set; } = ActualVersion;

    public CoreServiceAuthData Auth { get; set; }

    public YandexMusicWebApiSettings MusicWebApi { get; set; } = new();

    public YandexMusicMobileApiSettings MusicMobileApi { get; set; } = new();

    public YandexPassportApiSettings PassportApi { get; set; } = new();

    public CoreServiceSettings CoreService { get; set; } = new();

    public CacheSettings Cache { get; set; } = new();

    public GuiSettings Gui { get; set; } = new();

    public void Refresh(RootSettings newSettings) {
        newSettings.Auth.DeepCloneTo(Auth);
        newSettings.MusicWebApi.DeepCloneTo(MusicWebApi);
        newSettings.MusicMobileApi.DeepCloneTo(MusicMobileApi);
        newSettings.PassportApi.DeepCloneTo(PassportApi);
        newSettings.CoreService.DeepCloneTo(CoreService);
        newSettings.Cache.DeepCloneTo(Cache);
        newSettings.Gui.DeepCloneTo(Gui);
    }
}
