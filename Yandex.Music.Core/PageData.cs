namespace Yandex.Music.Core;

public class PageData
{
    public string Query { get; set; }

    public EntityHandler MainEntity { get; set; }

    public List<EntityHandler> Ribbon { get; set; } = new();
}
