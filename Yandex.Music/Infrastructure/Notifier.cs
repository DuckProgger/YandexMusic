using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Yandex.Music.Infrastructure;

/// <summary>
/// Класс для отображения информационной строки в представлении.
/// </summary>
internal class Notifier : INotifyPropertyChanged
{
    private readonly Dictionary<MessageType, MessageParameters> messagesParameters;

    public Notifier() {
        messagesParameters = new();
        MessageParameters defaultParameters = new(true);
        messagesParameters.Add(MessageType.Info, defaultParameters);
        messagesParameters.Add(MessageType.Error, defaultParameters);
        messagesParameters.Add(MessageType.Warning, defaultParameters);
    }
    public ObservableCollection<Message> Messages { get; } = new();

    public void AddInfo(string text) {
        Message message = new(text, MessageType.Info);
        AddMessage(message);
    }
    public void AddError(string text) {
        Message message = new(text, MessageType.Error);

        AddMessage(message);
    }
    public void AddWarning(string text) {
        Message message = new(text, MessageType.Warning);
        AddMessage(message);
    }

    private void AddMessage(Message message) {
        Messages.Add(message);
        MessageParameters parameters = messagesParameters[message.Type];
        if (parameters.Disappearing) {
            AutoRemove(message, parameters.DisappearingDelay);
        }
    }

    public void SetMessageParams(MessageType type, MessageParameters parameters) {
        messagesParameters[type] = parameters;
    }

    /// <summary>
    /// Флаг наличия сообщения.
    /// </summary>
    public bool HasMessage => Messages.Count > 0;

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string prop = "") {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    /// <summary>
    /// Автоматически очистить сообщение через заданное время.
    /// </summary>
    private async void AutoRemove(Message message, TimeSpan delay) {
        await Task.Delay(delay);
        _ = Messages.Remove(message);
    }

    public void Remove(string text) {
        Message message = Messages.FirstOrDefault(m => m.Text.Equals(text));
        if (message is null) {
            return;
        }

        _ = Messages.Remove(message);
    }
}

internal class MessageParameters
{
    private const double defaultDisappearingDelay = 3.0;
    public MessageParameters(bool disappearing, TimeSpan disappearingDelay) {
        Disappearing = disappearing;
        DisappearingDelay = disappearingDelay;
    }

    public MessageParameters(bool disappearing) {
        Disappearing = disappearing;
        DisappearingDelay = TimeSpan.FromSeconds(defaultDisappearingDelay);
    }

    public bool Disappearing { get; }
    public TimeSpan DisappearingDelay { get; }
}
