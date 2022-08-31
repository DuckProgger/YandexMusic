using Prism.Events;
using System.Collections.Generic;
using Yandex.Music.Core;

namespace Yandex.Music.Events;
internal class DownloadEvent : PubSubEvent<List<EntityHandler>>
{
}
