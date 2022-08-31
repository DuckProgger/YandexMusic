using Prism.Ioc;

namespace Yandex.Music.Infrastructure;

/// <summary>
/// Класс для получения требуемых сервисов.
/// </summary>
internal static class ServiceLocator
{
    /// <summary>
    /// Получить сервис типа <typeparamref name = "T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetService<T>() => ContainerLocator.Container.Resolve<T>();
}
