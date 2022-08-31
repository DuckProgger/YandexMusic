using System;

namespace Yandex.Music.Events;
internal class ProgressBarProgressEventArgs : EventArgs
{
    public int CurrentProgress { get; set; }
    public int Total { get; set; }
}
