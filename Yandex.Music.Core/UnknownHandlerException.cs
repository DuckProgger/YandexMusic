using System.Runtime.Serialization;

namespace Yandex.Music.Core;

public class UnknownHandlerException : Exception
{
    public UnknownHandlerException() {
    }

    public UnknownHandlerException(string message) : base(message) {
    }

    public UnknownHandlerException(string message, Exception innerException) : base(message, innerException) {
    }

    protected UnknownHandlerException(SerializationInfo info, StreamingContext context) : base(info, context) {
    }
}
