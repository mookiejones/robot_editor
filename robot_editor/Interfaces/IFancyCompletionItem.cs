namespace robot_editor.Interfaces
{
    public interface IFancyCompletionItem : ICompletionItem
    {
        object Content { get; }
        new object Description { get; }
    }
}
