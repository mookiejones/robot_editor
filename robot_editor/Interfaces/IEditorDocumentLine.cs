using ICSharpCode.AvalonEdit.Document;

namespace robot_editor.Interfaces
{
    public interface IEditorDocumentLine : IDocumentLine
    {
        string Text { get; }
    }
}
