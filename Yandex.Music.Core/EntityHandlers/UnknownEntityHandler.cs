using Yandex.Api.Music.Web.Entities;

namespace Yandex.Music.Core.EntityHandlers;

public class UnknownEntityHandler : EntityHandler
{
    public UnknownEntityHandler(IWebMusicEntity entity, CoreService service) : base(entity, service) {
    
    }
}
