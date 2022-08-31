namespace Yandex.Music.Core.FilePath.Snippet;

internal class SnippetStringFragment
{
    public bool IsSnippet { get; set; }

    public string Text { get; set; }

    public string Args { get; set; }

    public override string ToString() {
        return IsSnippet ? $"${Text}({Args})" : Text;
    }
}
