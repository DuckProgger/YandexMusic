using System.Text;

namespace Yandex.Music.Core.FilePath.Snippet;

internal class SnippetString : List<SnippetStringFragment>
{
    public static SnippetString Parse(string template) {
        SnippetString snippetString = new();

        SnippetStringFragment currentFragment = new();
        StringBuilder text = new();
        StringBuilder args = new();

        StringBuilder current = text;
        bool isSnippet = false;

        for (int i = 0; i < template.Length; i++) {
            char c = template[i];
            char? next = i < template.Length - 1 ? template[i + 1] : null;

            if (c == '$' && next.HasValue && next == '$') {
                current.Append(c);
                i++;
            }

            else if (!isSnippet && c == '$') {
                snippetString.AppendFragmentAndClear(isSnippet, text, args);
                isSnippet = true;
                current = text;
            }

            else if (isSnippet && c == '(') {
                current = args;
            }

            else if (isSnippet && c == ')') {
                snippetString.AppendFragmentAndClear(isSnippet, text, args);
                isSnippet = false;
                current = text;
            }

            else {
                current.Append(c);
            }
        }
        snippetString.AppendFragmentAndClear(isSnippet, text, args);

        return snippetString;
    }

    public override string ToString() {
        StringBuilder str = new();
        foreach (SnippetStringFragment fragment in this) {
            str.Append(fragment.ToString());
        }
        return str.ToString();
    }


    private SnippetString() {

    }

    private void AppendFragmentAndClear(bool isSnippet, StringBuilder text, StringBuilder args) {
        if (text.Length > 0) {
            Add(new SnippetStringFragment {
                IsSnippet = isSnippet,
                Text = text.ToString(),
                Args = args.ToString(),
            });
        }
        text.Clear();
        args.Clear();
    }
}
