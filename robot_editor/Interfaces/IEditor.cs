﻿using System;
using System.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using robot_editor.Classes;

namespace robot_editor.Interfaces
{
    public interface IEditor : ITextBuffer, IServiceProvider
    {
        new string Text { get; set; }
        int TotalNumberOfLines { get; }
        event EventHandler<TextChangeEventArgs> Changing;
        event EventHandler<TextChangeEventArgs> Changed;
        IEditorDocumentLine GetLine(int lineNumber);
        IEditorDocumentLine GetLineForOffset(int offset);
        int PositionToOffset(int line, int column);
        Location OffsetToPosition(int offset);
        string Filename { get; set; }
        bool IsModified { get; set; }

        [Localizable(false)]
        void Insert(int offset, string text);

        void Insert(int offset, string text, AnchorMovementType defaultAnchorMovementType);
        void Remove(int offset, int length);
        void Replace(int offset, int length, string newText);
        void StartUndoableAction();
        void EndUndoableAction();
        IDisposable OpenUndoGroup();
        ITextAnchor CreateAnchor(int offset);
        void ReplaceAll();
    }

}
