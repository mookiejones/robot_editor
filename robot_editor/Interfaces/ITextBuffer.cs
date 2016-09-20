using System;
using System.IO;

namespace robot_editor.Interfaces
{
    public interface ITextBuffer
    {
        ITextBufferVersion Version { get; }
        int TextLength { get; }
        string Text { get; }
        event EventHandler TextChanged;
        ITextBuffer CreateSnapshot();
        ITextBuffer CreateSnapshot(int offset, int length);
        TextReader CreateReader();
        TextReader CreateReader(int offset, int length);
        char GetCharAt(int offset);
        string GetText(int offset, int length);
    }
}
