using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Snippets;
using robot_editor.Interfaces;

namespace robot_editor.Classes
{
    public class LanguageBase:ILanguageClass
    {
        public DirectoryInfo RootPath { get; set; }
        public string FileName { get; set; }
        public MenuItem RobotMenuItems { get; set; }
        public string Name { get; }
        public string SnippetPath { get; }
        public string Intellisense { get; }
        public string SnippetFilePath { get; }
        public string Filename { get; }
        public string RawText { get; set; }
        public Regex MethodRegex { get; }
        public Regex StructRegex { get; }
        public Regex FieldRegex { get; }
        public Regex SignalRegex { get; }
        public Regex EnumRegex { get; }
        public Regex XYZRegex { get; }
        public ReadOnlyObservableCollection<Snippet> Snippets { get; }
        public AbstractFoldingStrategy FoldingStrategy { get; }
        public string CommentChar { get; }
        public IEnumerable<IVariable> Fields { get; set; }
        public void GetRootDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string FoldTitle(FoldingSection section, TextDocument document)
        {
            throw new NotImplementedException();
        }
    }
}
