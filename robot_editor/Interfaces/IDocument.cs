using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace robot_editor.Interfaces
{
    public interface IDocument
    {
        Visibility Visibility { get; set; }
        ILanguageClass FileLanguage { get; set; }
        IEditor TextBox { get; set; }
        string FilePath { get; set; }
        ImageSource IconSource { get; set; }
        string FileName { get; }
        string Title { get; set; }
        bool IsDirty { get; set; }
        string ContentId { get; set; }
        bool IsSelected { get; set; }
        bool IsActive { get; set; }

        ICommand CloseCommand { get; }
        void Load(string filepath);
        void SelectText(IVariable variable);
    }
}
