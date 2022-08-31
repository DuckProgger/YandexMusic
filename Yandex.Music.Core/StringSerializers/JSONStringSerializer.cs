using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Yandex.Music.Core.StringSerializers;

public class JsonStringSerializer<T> : IStringSerializer<T>
{
    public JsonSerializerSettings SerializerSettings { get; set; }

    public JsonStringSerializer() {
        SerializerSettings = CreateSnakeCaseSerializerSettings();
    }

    public T Deserialize(string value) {
        return JsonConvert.DeserializeObject<T>(value, SerializerSettings);
    }

    public string Serialize(T value) {
        return JsonConvert.SerializeObject(value, SerializerSettings);
    }

    private static JsonSerializerSettings CreateSnakeCaseSerializerSettings() {
        DefaultContractResolver contractResolver = new() {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };
        JsonSerializerSettings serializerSettings = new() {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented,
        };
        serializerSettings.Converters.Add(new StringEnumConverter() {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        });
        return serializerSettings;
    }
}
