using System;
using System.Collections.Generic;

namespace robot_editor.Interfaces
{
    public interface IBookmarkMargin
    {
        IList<IBookmark> Bookmarks { get; }
        event EventHandler RedrawRequested;
        void Redraw();
    }
}
