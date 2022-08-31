namespace Yandex.Music.Core.StringSerializers;

public interface IStringSerializer<T>
{
    string Serialize(T value);

    T Deserialize(string value);
}
