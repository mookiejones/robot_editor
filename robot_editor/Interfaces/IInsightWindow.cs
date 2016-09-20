using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;

namespace robot_editor.Interfaces
{
    public interface IInsightWindow : ICompletionWindow
    {
        IList<IInsightItem> Items { get; }
        IInsightItem SelectedItem { get; set; }
        event EventHandler<TextChangeEventArgs> DocumentChanged;
        event EventHandler SelectedItemChanged;
        event EventHandler CaretPositionChanged;
    }
}
