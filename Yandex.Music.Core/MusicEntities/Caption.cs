using Yandex.Api.Music.Web.Entities;

namespace Yandex.Music.Core.MusicEntities;

public class Caption : IWebMusicEntity
{
    public string Title { get; set; }

    public string Query { get; set; }

    public IWebMusicEntity Entity { get; set; }

    public override string ToString() {
        return Title;
    }
}
