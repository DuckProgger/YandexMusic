namespace Yandex.Music.Infrastructure;

internal class Message
{
    public Message(string text, MessageType type) {
        Text = text;
        Type = type;
    }

    public string Text { get; } = null!;
    public MessageType Type { get; }
}
internal enum MessageType { Info, Error, Warning }
