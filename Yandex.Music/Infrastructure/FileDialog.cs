using Microsoft.WindowsAPICodePack.Dialogs;

namespace Yandex.Music.Infrastructure;

internal class PathDialog
{
    public string Open() {
        CommonOpenFileDialog dialog = new();
        dialog.IsFolderPicker = true;
        return dialog.ShowDialog() == CommonFileDialogResult.Ok 
            ? dialog.FileName 
            : string.Empty;
    }
}

