using Yandex.Api.Music.Web.Entities;

namespace Yandex.Music.Core;

public interface IEntityHandlerProvider
{
    EntityHandler GetEntityHandler(IWebMusicEntity entity);
}
