using ICSharpCode.AvalonEdit.Document;
using robot_editor.Classes;

namespace robot_editor.Interfaces
{
    public interface IBracketSearcher
    {
        BracketSearchResult SearchBracket(TextDocument document, int offset);
    }
}
