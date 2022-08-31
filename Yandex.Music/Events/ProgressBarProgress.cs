using System;
using System.Collections.Generic;

namespace Yandex.Music.Events;
internal class ProgressBarProgress : IProgress<KeyValuePair<int, int>>
{
    public event ProgressBarEventHandler Updated;

    public delegate void ProgressBarEventHandler(object sender, ProgressBarProgressEventArgs args);

    public void Report(KeyValuePair<int, int> value) {
        ProgressBarProgressEventArgs args = new() {
            CurrentProgress = value.Key,
            Total = value.Value,
        };
        Updated?.Invoke(this, args);
    }
}
