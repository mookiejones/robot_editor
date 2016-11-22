namespace robot_editor.Interfaces
{
    public interface ISnippetCompletionItem : ICompletionItem
    {
        string Keyword { get; }
    }
}
