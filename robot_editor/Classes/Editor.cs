using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using robot_editor.Interfaces;
 
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit.Snippets;
using Microsoft.Win32;
 
namespace robot_editor.Classes
{
    public class Editor: TextEditor,IEditor
    {
        #region Constructor

        public Editor()
        {


 
            _iconBarMargin = new IconBarMargin(_iconBarManager = new IconBarManager());
            InitializeMyControl();
            MouseHoverStopped += delegate { _toolTip.IsOpen = false; };
        }

        #endregion

        #region ViewModel Properties

        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
            typeof(Editor), new PropertyMetadata((obj, args) =>
            {
                var target = (Editor)obj;
                target.Text = (string)args.NewValue;
            }));

        private readonly ReadOnlyObservableCollection<IVariable> _readonlyVariables = null;
        private readonly ObservableCollection<IVariable> _variables = new ObservableCollection<IVariable>();
        private EDITORTYPE _editortype;
        private string _fileSave = string.Empty;
        private ILanguageClass _filelanguage = new LanguageBase();
        private String _filename = string.Empty;
        private IVariable _selectedVariable;

        public int Line
        {
            get { return TextArea.Caret.Column; }
        }

        /// <summary>
        ///     Used for displaying position in status bar
        /// </summary>
        public int Column
        {
            get { return TextArea.Caret.Column; }
        }

        /// <summary>
        ///     Used for displaying position in status bar
        /// </summary>
        public int Offset
        {
            get { return TextArea.Caret.Offset; }
        }

        public new string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public EDITORTYPE EditorType
        {
            get { return _editortype; }
            set
            {
                _editortype = value;
                OnPropertyChanged("EditorType");
            }
        }


        public IVariable SelectedVariable
        {
            get { return _selectedVariable; }
            set
            {
                _selectedVariable = value;
                SelectText(_selectedVariable);
                OnPropertyChanged("SelectedVariable");
            }
        }

        public string Filename
        {
            get { return _filename; }
            set
            {
                _filename = value;
                OnPropertyChanged("Filename");
                OnPropertyChanged("Title");
            }
        }

        public ILanguageClass FileLanguage
        {
            get { return _filelanguage; }
            set
            {
                _filelanguage = value;
                OnPropertyChanged("FileLanguage");
            }
        }


        public ReadOnlyObservableCollection<IVariable> Variables
        {
            get { return _readonlyVariables ?? new ReadOnlyObservableCollection<IVariable>(_variables); }
        }


        public string FileSave
        {
            get { return _fileSave; }
            set
            {
                _fileSave = value;
                OnPropertyChanged("FileSave");
            }
        }

        #endregion

        #region Commands



        #region UndoCommand
        private RelayCommand _undoCommand;

        /// <summary>
        /// Gets the UndoCommand.
        /// </summary>
        public RelayCommand UndoCommand
        {
            get
            {
                return _undoCommand
                    ?? (_undoCommand = new RelayCommand(ExecuteUndoCommand));
            }
        }

        private void ExecuteUndoCommand()
        {
            Undo();
        }
        #endregion


        #region RedoCommand
        private RelayCommand _redoCommand;

        /// <summary>
        /// Gets the RedoCommand.
        /// </summary>
        public RelayCommand RedoCommand
        {
            get
            {
                return _redoCommand
                    ?? (_redoCommand = new RelayCommand(ExecuteRedoCommand));
            }
        }

        private void ExecuteRedoCommand()
        {
            Redo();
        }
        #endregion


        #region
        private RelayCommand _saveCommand;

        /// <summary>
        /// Gets the SaveCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(
                    ExecuteSaveCommand,
                    CanExecuteSaveCommand));
            }
        }

        private void ExecuteSaveCommand()
        {
            Save();
        }

        private bool CanExecuteSaveCommand()
        {
            return CanSave();
        }
        #endregion


        #region SaveAsCommand

        #region
        private RelayCommand _saveAsCommand;

        /// <summary>
        /// Gets the SaveAsCommand.
        /// </summary>
        public RelayCommand SaveAsCommand
        {
            get
            {
                return _saveAsCommand ?? (_saveAsCommand = new RelayCommand(
                    ExecuteSaveAsCommand,
                    CanExecuteSaveAsCommand));
            }
        }

        private void ExecuteSaveAsCommand()
        {
            SaveAs();
        }

        private bool CanExecuteSaveAsCommand()
        {
            return CanSave();
        }
        #endregion
        #endregion

        #region ReplaceCommand
        private RelayCommand _replaceCommand;

        /// <summary>
        /// Gets the ReplaceCommand.
        /// </summary>
        public RelayCommand ReplaceCommand
        {
            get
            {
                return _replaceCommand
                    ?? (_replaceCommand = new RelayCommand(ExecuteReplaceCommand));
            }
        }

        private void ExecuteReplaceCommand()
        {
            Replace();
        }
        #endregion



        #region
        private RelayCommand<object> _variableDoubleClickCommand;

        /// <summary>
        /// Gets the VariableDoubleClickCommand.
        /// </summary>
        public RelayCommand<object> VariableDoubleClickCommand
        {
            get
            {
                return _variableDoubleClickCommand ?? (_variableDoubleClickCommand = new RelayCommand<object>(
                    ExecuteVariableDoubleClickCommand,
                    CanExecuteVariableDoubleClickCommand));
            }
        }

        private void ExecuteVariableDoubleClickCommand(object parameter)
        {
            SelectText((IVariable)((ListViewItem)parameter).Content);
        }

        private bool CanExecuteVariableDoubleClickCommand(object parameter)
        {
            return parameter != null;
        }
        #endregion

        #region GotoCommand

        #region
        private RelayCommand _gotoCommand;

        /// <summary>
        /// Gets the GotoCommand.
        /// </summary>
        public RelayCommand GotoCommand
        {
            get
            {
                return _gotoCommand ?? (_gotoCommand = new RelayCommand(
                    ExecuteGotoCommand,
                    CanExecuteGotoCommand));
            }
        }

        private void ExecuteGotoCommand()
        {
            Goto();
        }

        private bool CanExecuteGotoCommand()
        {
            return !String.IsNullOrEmpty(Text);
        }
        #endregion
        #endregion

        #region OpenAllFoldsCommand

        private RelayCommand _openAllFoldsCommand;

        /// <summary>
        ///     Gets the OpenAllFoldsCommand.
        /// </summary>
        public RelayCommand OpenAllFoldsCommand
        {
            get
            {
                return _openAllFoldsCommand ??
                       (_openAllFoldsCommand =
                           new RelayCommand(ExecuteOpenAllFoldsCommand,
                               CanOpenAllFoldsCommand));
            }
        }

        private void ExecuteOpenAllFoldsCommand()
        {
            ChangeFoldStatus(false);
        }

        private bool CanOpenAllFoldsCommand()
        {
            return ((_foldingManager != null) && (_foldingManager.AllFoldings.Any()));
        }
        #endregion

        #region ToggleCommentCommand

        private RelayCommand _toggleCommentCommand;

        public RelayCommand ToggleCommentCommand
        {
            get
            {
                return _toggleCommentCommand ??
                       (_toggleCommentCommand =
                           new RelayCommand(ToggleComment, CanToggleCommentCommand));
            }

        }

        private bool CanToggleCommentCommand()
        {
            return !String.IsNullOrEmpty(FileLanguage.CommentChar);
        }

        #endregion

        #region ToggleFoldsCommand

        private RelayCommand _toggleFoldsCommand;

        public ICommand ToggleFoldsCommand
        {
            get
            {
                return _toggleFoldsCommand ??
                       (_toggleFoldsCommand =
                           new RelayCommand(ToggleFolds, CanToggleFoldsCommand));
            }
        }


        private bool CanToggleFoldsCommand()
        {
            return ((_foldingManager != null) && (_foldingManager.AllFoldings.Any()));
        }
        #endregion

        #region ToggleAllFoldsCommand

        private RelayCommand _toggleAllFoldsCommand;

        public ICommand ToggleAllFoldsCommand
        {
            get
            {
                return _toggleAllFoldsCommand ??
                       (_toggleAllFoldsCommand =
                           new RelayCommand(ToggleAllFolds, CanToggleFoldsCommand));

            }
        }

        #endregion

        #region CloseAllFoldsCommand

        private RelayCommand _closeAllFoldsCommand;

        public ICommand CloseAllFoldsCommand
        {
            get
            {
                return _closeAllFoldsCommand ??
                       (_closeAllFoldsCommand =
                           new RelayCommand(ExecuteCloseAllFoldsCommand, CanToggleFoldsCommand));
            }
        }

        private void ExecuteCloseAllFoldsCommand()
        {
            ChangeFoldStatus(true);
        }
        #endregion

        #region AddTimeStampCommand



        #region
        private RelayCommand _addTimeStampCommand;

        /// <summary>
        /// Gets the AddTimeStampCommand.
        /// </summary>
        public RelayCommand AddTimeStampCommand
        {
            get
            {
                return _addTimeStampCommand
                    ?? (_addTimeStampCommand = new RelayCommand(
                                          () => AddTimeStamp(true)));
            }
        }
        #endregion


        private void AddTimeStamp(bool b)
        {
            var _return =
                new SnippetTextElement { Text = "\r\n; * " };

            var by = new SnippetTextElement { Text = "By : " };

            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null)
            {
                var name = new SnippetReplaceableTextElement { Text = windowsIdentity.Name }
                    ;

                var date = new SnippetTextElement
                {
                    Text = DateTime.Now.ToString(((EditorOptions)Options).TimestampFormat)
                };
                var snippet = new Snippet
                {
                    Elements =
                    {
                        _return,
                        @by,
                        name,
                        _return,
                        date,
                        _return
                    }
                };
                snippet.Insert(TextArea);
            }
        }

        #endregion

        #region FindCommand


        #region
        private RelayCommand _findCommand;

        /// <summary>
        /// Gets the FindCommand.
        /// </summary>
        public RelayCommand FindCommand
        {
            get
            {
                return _findCommand ?? (_findCommand = new RelayCommand(
                    ExecuteFindCommand,
                    CanExecuteFindCommand));
            }
        }

        private void ExecuteFindCommand()
        {
            ChangeFoldStatus(true);
        }

        private bool CanExecuteFindCommand()
        {
            return ((_foldingManager != null) && (_foldingManager.AllFoldings.Any()));
        }
        #endregion

        #endregion

        #region ReloadCommand

        private RelayCommand _reloadCommand;

        public ICommand ReloadCommand
        {
            get { return _reloadCommand ?? (_reloadCommand = new RelayCommand(Reload)); }
        }

        #endregion

        #region ShowDefinitionsCommand

        private RelayCommand _showDefinitionsCommand;

        public ICommand ShowDefinitionsCommand
        {
            get
            {
                return _showDefinitionsCommand ??
                       (_showDefinitionsCommand =
                           new RelayCommand(ShowDefinitions, () => _foldingManager != null));
            }
        }

        #endregion

        #region CutCommand

        private RelayCommand _cutCommand;

        public ICommand CutCommand
        {
            get { return _cutCommand ?? (_cutCommand = new RelayCommand(Cut, () => (Text.Length > 0))); }
        }

        #endregion

        #region CopyCommand

        private RelayCommand _copyCommand;

        public ICommand CopyCommand
        {
            get { return _copyCommand ?? (_copyCommand = new RelayCommand(Cut, () => (Text.Length > 0))); }
        }

        #endregion

        #region PasteCommand

        private RelayCommand _pasteCommand;

        public ICommand PasteCommand
        {
            get
            {
                return _pasteCommand ?? (_pasteCommand = new RelayCommand(Paste, () => (Clipboard.ContainsText())));
            }
        }

        #endregion

        #region FunctionWindowClickCommand


        private RelayCommand<object> _functionWindowClickCommand;

        /// <summary>
        /// Gets the FunctionWindowClickCommand.
        /// </summary>
        public RelayCommand<object> FunctionWindowClickCommand
        {
            get
            {
                return _functionWindowClickCommand
                    ?? (_functionWindowClickCommand = new RelayCommand<object>(
                                          p =>
                                          {
                                              OpenFunctionItem(p);
                                          }));
            }
        }
        #endregion

        #endregion

        #region ChangeIndentCommand

        private RelayCommand<object> _changeIndentCommand;





        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand<object> ChangeIndentCommand
        {
            get
            {
                return _changeIndentCommand
                    ?? (_changeIndentCommand = new RelayCommand<object>(
                                          ChangeIndent));
            }
        }
        #endregion




        #region Private Members

        //  For Zooming When Scrolling Text
        /// <summary>
        /// </summary>
        private const int LogicListFontSizeMax = 50;

        /// <summary>
        ///     For Zooming When Scrolling Text
        /// </summary>
        private const int LogicListFontSizeMin = 10;

        private readonly IconBarManager _iconBarManager;
        private readonly IconBarMargin _iconBarMargin;


        /// <summary>
        ///     Records last key For Multiple Key presses
        /// </summary>
        private KeyEventArgs _lastKeyUpArgs;

        #endregion

        private ToolTip _toolTip = new ToolTip();

        private void OpenFunctionItem(object parameter)
        {
            var i = (IVariable)((ListViewItem)parameter).Content;
            SelectText(i);
        }


        private void InitializeMyControl()
        {
            TextArea.LeftMargins.Insert(0, _iconBarMargin);
            //            var searchInputHandler = new SearchInputHandler(TextArea);
            SearchPanel searchPanel = SearchPanel.Install(TextArea);

            //           TextArea.DefaultInputHandler.NestedInputHandlers.Add(searchPanel);

            AddBindings();
            TextArea.TextEntered += TextEntered;
            //   TextArea.TextEntering += TextEntering;
            TextArea.Caret.PositionChanged += CaretPositionChanged;
            DataContext = this;
        }

        private void AddBookMark(int lineNumber, string imgpath)
        {
            BitmapImage bitmap = Utilities.LoadBitmap(imgpath);
            var bmi = new BookmarkImage(bitmap);
            _iconBarManager.Bookmarks.Add(new ClassMemberBookmark(lineNumber, bmi));
        }


        //TODO Signal Path for KUKARegex currently displays linear motion
        private void FindMatches(Regex matchstring, string imgPath)
        {
            // Dont Include Empty Values
            if (String.IsNullOrEmpty(matchstring.ToString())) return;

            Match m = matchstring.Match(Text.ToLowerInvariant());


            while (m.Success)
            {
                _variables.Add(new Variable
                {
                    Declaration = m.Groups[0].ToString(),
                    Offset = m.Index,
                    Type = m.Groups[1].ToString(),
                    Name = m.Groups[2].ToString(),
                    Value = m.Groups[3].ToString(),
                    Path = Filename,
                    Icon = Utilities.LoadBitmap(imgPath)
                });
                DocumentLine d = Document.GetLineByOffset(m.Index);
                AddBookMark(d.LineNumber, imgPath);
                m = m.NextMatch();
            }
            if (FileLanguage is Languages.KUKA)
            {
                m =
                    matchstring.Match(String.CompareOrdinal(Text, FileLanguage.SourceText) == 0
                        ? FileLanguage.DataText
                        : FileLanguage.SourceText);
                while (m.Success)
                {
                    _variables.Add(new Variable
                    {
                        Declaration = m.Groups[0].ToString(),
                        Offset = m.Index,
                        Type = m.Groups[1].ToString(),
                        Name = m.Groups[2].ToString(),
                        Value = m.Groups[3].ToString(),
                        Path = Filename,
                        Icon = Utilities.LoadBitmap(imgPath)
                    });

                    m = m.NextMatch();
                }
            }
        }

        /// <summary>
        ///     Find info for bookmark
        ///     <remarks>Need to make sure Correct Priority is set. Whatever is set first will overwrite anything after</remarks>
        /// </summary>
        private void FindBookmarkMembers()
        {
            // Return if FileLanguage doesnt exist yet
            if (FileLanguage == null) return;
            _iconBarManager.Bookmarks.Clear();
            _variables.Clear();
            FindMatches(FileLanguage.MethodRegex, Global.ImgMethod);
            FindMatches(FileLanguage.StructRegex, Global.ImgStruct);
            FindMatches(FileLanguage.FieldRegex, Global.ImgField);
            FindMatches(FileLanguage.SignalRegex, Global.ImgSignal);
            FindMatches(FileLanguage.EnumRegex, Global.ImgEnum);
            FindMatches(FileLanguage.XYZRegex, Global.ImgXyz);
        }
        /*
        protected override void OnOptionChanged(PropertyChangedEventArgs e)
        {
            base.OnOptionChanged(e);
#if TRACE
            Console.WriteLine(e.PropertyName);
#endif
            switch (e.PropertyName)
            {
                case "EnableFolding":
                    UpdateFolds();
                    break;
            }
        }

        */
        public void SetHighlighting()
        {
            try
            {
                if (Filename != null)
                    SyntaxHighlighting =
                        HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(Filename));
            }
            catch (Exception ex)
            {
                MessageViewModel.AddError(String.Format("Could not load Syntax Highlighting for {0}", Filename), ex);
            }
        }

        private void EditorPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var textEditorOptions = Options as EditorOptions;

            if (textEditorOptions != null && !textEditorOptions.MouseWheelZoom) return;
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            if (e.Delta <= 0 || !(FontSize < LogicListFontSizeMax))
            {
                FontSize -= 1;
            }
            else if ((e.Delta > 0) && (FontSize > LogicListFontSizeMin))
            {
                FontSize += 1;
            }
            e.Handled = true;
        }

        [Localizable(false), UsedImplicitly]
        private void InsertSnippet()
        {
#pragma warning disable 168
            var loopCounter = new SnippetReplaceableTextElement { Text = "i" };
#pragma warning restore 168

            var snippet = new Snippet
            {
                Elements =
                {
                    new SnippetTextElement {Text = "for "},
                    new SnippetReplaceableTextElement {Text = "item"},
                    new SnippetTextElement {Text = " in range("},
                    new SnippetReplaceableTextElement {Text = "from"},
                    new SnippetTextElement {Text = ", "},
                    new SnippetReplaceableTextElement {Text = "to"},
                    new SnippetTextElement {Text = ", "},
                    new SnippetReplaceableTextElement {Text = "step"},
                    new SnippetTextElement {Text = "):backN\t"},
                    new SnippetSelectionElement()
                }
            };
            snippet.Insert(TextArea);
        }


        private void TextEditorGotFocus(object sender, RoutedEventArgs e)
        {
            DocumentViewModel.Instance.TextBox = this;


            OnPropertyChanged("Line");
            OnPropertyChanged("Column");
            OnPropertyChanged("Offset");
            OnPropertyChanged("RobotType");
            FileSave = !String.IsNullOrEmpty(Filename)
                ? File.GetLastWriteTime(Filename).ToString(CultureInfo.InvariantCulture)
                : String.Empty;
        }

        #region Folding Section

        private FoldingManager _foldingManager;
        private object _foldingStrategy;


        [Localizable(false)]
        private void UpdateFolds()
        {
            var textEditorOptions = Options as EditorOptions;
            bool foldingEnabled = textEditorOptions != null && textEditorOptions.EnableFolding;

            //if (File == null) return;
            if (SyntaxHighlighting == null)
                _foldingStrategy = null;


            // If Filename is null then return. no need for folding
            if (!File.Exists(Filename)) return;


            // Get XML Folding
            if ((Path.GetExtension(Filename) == ".xml") || (Path.GetExtension(Filename) == ".cfg"))
                _foldingStrategy = new XmlFoldingStrategy();
            else if (FileLanguage != null)
                _foldingStrategy = FileLanguage.FoldingStrategy;

            if (_foldingStrategy != null && foldingEnabled)
            {
                if (_foldingManager == null)
                    _foldingManager = FoldingManager.Install(TextArea);
                ((AbstractFoldingStrategy)_foldingStrategy).UpdateFoldings(_foldingManager, Document);
                RegisterFoldTitles();
            }
            else
            {
                if (_foldingManager != null)
                {
                    FoldingManager.Uninstall(_foldingManager);
                    _foldingManager = null;
                }
            }
        }


        /// <summary>
        ///     Writes Titles for When the fold is closed
        /// </summary>
        private void RegisterFoldTitles()
        {
            if ((DocumentViewModel.Instance.FileLanguage is LanguageBase) && (Path.GetExtension(Filename) == ".xml"))
                return;

            foreach (FoldingSection section in _foldingManager.AllFoldings)
                section.Title = DocumentViewModel.Instance.FileLanguage.FoldTitle(section, Document);
        }

        private string GetLine(int idx)
        {
            DocumentLine line = Document.GetLineByNumber(idx);
            return Document.GetText(line.Offset, line.Length);
        }

        public string FindWord()
        {
            string line = GetLine(TextArea.Caret.Line);
            string search = line;
            char[] terminators = { ' ', '=', '(', ')', '[', ']', '<', '>', '\r', '\n' };


            // Are there any terminators in the line?
            int end = line.IndexOfAny(terminators, TextArea.Caret.Column - 1);
            if (end > -1)
                search = (line.Substring(0, end));

            int start = search.LastIndexOfAny(terminators) + 1;

            if (start > -1)
                search = search.Substring(start).Trim();

            return search;
        }

        private bool GetCurrentFold(TextViewPosition loc)
        {
            int off = Document.GetOffset(loc.Location);

            ReadOnlyCollection<FoldingSection> f = _foldingManager.GetFoldingsAt(off);
            if (f.Count == 0)
                return false;
            _toolTip = new ToolTip
            {
                Style = (Style)FindResource("FoldToolTipStyle"),
                DataContext = f,
                PlacementTarget = this,
                IsOpen = true
            };


            //         foreach (var fld in _foldingManager.AllFoldings)
            //         {
            //
            //             if (fld.StartOffset <= off && off <= fld.EndOffset && fld.IsFolded)
            //             {
            //             	toolTip = new System.Windows.Controls.ToolTip
            //             	{
            //             		Style = (Style)FindResource("FoldToolTipStyle"),
            //             		DataContext = fld,
            //             		PlacementTarget = this,
            //             		IsOpen=true
            //             	};
            //
            //                 
            //                 return true;
            //                
            //                // e.Handled = true;
            //             }
            //     }
            return true;
        }

        private void Mouse_OnHover(object sender, MouseEventArgs e)
        {
            if (_foldingManager == null) return;

            //UpdateFolds();
            TextViewPosition? tvp = GetPositionFromPoint(e.GetPosition(this));


            if (tvp.HasValue)
                e.Handled = GetCurrentFold((TextViewPosition)tvp);


            //TODO _variables
            //    toolTip.PlacementTarget = this;
            //    // required for property inheritance 
            //    toolTip.Content = wordhover; 
            //    pos.ToString();
            //    toolTip.IsOpen = true;
            //    e.Handled = true;


            // Is Current Line a Variable?
            // ToolTip t = FileLanguage.variables.VariableToolTip(GetLine(pos.Value.Line));
            //   if (t != null)
            //   {
            //       t.PlacementTarget = this;
            //       t.IsOpen = true;
            //       e.Handled = true;
            //       disposeToolTip = false;
            //       return;
            //   }
            //
        }


        private void ToggleFolds()
        {
            if (_foldingManager == null) return;
            // Look for folding on this line: 
            FoldingSection folding =
                _foldingManager.GetNextFolding(TextArea.Document.GetOffset(TextArea.Caret.Line,
                    TextArea.Caret.Column));
            if (folding == null || Document.GetLineByOffset(folding.StartOffset).LineNumber != TextArea.Caret.Line)
            {
                // no folding found on current line: find innermost folding containing the caret
                folding = _foldingManager.GetFoldingsContaining(TextArea.Caret.Offset).LastOrDefault();
            }
            if (folding != null)
            {
                folding.IsFolded = !folding.IsFolded;
            }
        }

        private void ToggleAllFolds()
        {
            if (_foldingManager == null) return;
            foreach (FoldingSection fm in _foldingManager.AllFoldings)
                fm.IsFolded = !fm.IsFolded;
        }

        private void ChangeFoldStatus(bool isFolded)
        {
            foreach (FoldingSection fm in _foldingManager.AllFoldings)
                fm.IsFolded = isFolded;
        }


        private void ShowDefinitions()
        {
            if (_foldingManager == null) return;
            foreach (FoldingSection fm in _foldingManager.AllFoldings)
                fm.IsFolded = fm.Tag is NewFolding;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region FileWatcher

        // ReSharper disable UnusedMember.Local
        private void SetWatcher()
        // ReSharper restore UnusedMember.Local
        {
            string dir = Path.GetDirectoryName(Filename);
            bool dirExists = dir != null && Directory.Exists(dir);
            //TODO Reimplement this
            // ReSharper disable RedundantJumpStatement
            if (!dirExists) return;
            // ReSharper restore RedundantJumpStatement

            // Only Watch For Module and Not individual files. This prevents from Reloading twice
        }


        public void Reload()
        {
            MessageBoxResult answer = MessageBox.Show("Are you sure you want to reload file?", "Reload file",
                MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (!(answer == MessageBoxResult.OK | (!IsModified))) return;
            Load(Filename);
            UpdateFolds();
        }

        #endregion

        #region Code Completion

        public static readonly DependencyProperty CompletionWindowProperty =
            DependencyProperty.Register("CompletionWindow", typeof(CompletionWindow), typeof(Editor));

        private CompletionWindow CompletionWindow
        {
            get { return (CompletionWindow)GetValue(CompletionWindowProperty); }
            set { SetValue(CompletionWindowProperty, value); }
        }

        private void TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (FileLanguage == null || FileLanguage is LanguageBase) return;

            string currentword = FindWord();


            // CompletionWindow.Activate();
            if (IsModified || IsModified)
                UpdateFolds();


            // Dont Show Completion window until there are 3 Characters
            if (currentword != null && (String.IsNullOrEmpty(currentword)) | currentword.Length < 3) return;

            ShowCompletionWindow(currentword);
        }


        private void ShowCompletionWindow(string currentword)
        {
            CompletionWindow = new CompletionWindow(TextArea);

            // FileLanguage.CompletionList(this, currentword, CompletionWindow.CompletionList.CompletionData);
            IEnumerable<ICompletionData> items = GetCompletionItems();


            foreach (ICompletionData item in items)
                CompletionWindow.CompletionList.CompletionData.Add(item);

            CompletionWindow.Closed += delegate { CompletionWindow = null; };
            CompletionWindow.CloseWhenCaretAtBeginning = true;

            CompletionWindow.CompletionList.SelectItem(currentword);
            if (CompletionWindow.CompletionList.SelectedItem != null)
                CompletionWindow.Show();
        }

        //  private void TextEntering(object sender, TextCompositionEventArgs e)
        //  {
        //    //  if (e.Text.Length <= 0 || CompletionWindow == null) return;
        //    //  if (!char.IsLetterOrDigit(e.Text[0]))
        //    //  {
        //    //      // Whenever a non-letter is typed while the completion window is open,
        //    //      // insert the currently selected element.
        //    //      CompletionWindow.CompletionList.RequestInsertion(e);
        //    //  }
        //    //  // Do not set e.Handled=true.
        //      // We still want to insert the character that was typed.
        //  }

        //Trying to fix
        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (CompletionWindow == null) return;
            if (e.Key == Key.Tab)
                CompletionWindow.CompletionList.RequestInsertion(e);
            if (e.Key == Key.Return)
                CompletionWindow = null;
        }

        #region Code Completion

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public IList<ICompletionData> CompletionData { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        private IEnumerable<ICompletionData> GetCompletionItems()
        {
            var items = new List<ICompletionData>();

            items.AddRange(HighlightList());
            //            items.AddRange(SnippetList());
            //  items.AddRange(LocalCompletionList());
            items.AddRange(ObjectBrowserCompletionList());
            return items.ToArray();
        }

        private IEnumerable<Snippet> SnippetList()
        {
            return FileLanguage.Snippets.ToList();
        }

        private IEnumerable<ICompletionData> HighlightList()
        {
            var items = new List<CodeCompletion>();

            foreach (CodeCompletion item in from rule in SyntaxHighlighting.MainRuleSet.Rules
                                            select rule.Regex.ToString()
                into parseString
                                            let start = parseString.IndexOf(">", StringComparison.Ordinal) + 1
                                            let end = parseString.LastIndexOf(")", StringComparison.Ordinal)
                                            select parseString.Substring(start, end - start)
                into parseString1
                                            select parseString1.Split('|')
                into spl
                                            from item in
                                                spl.Where(t => !String.IsNullOrEmpty(t))
                                                    .Select(t => new CodeCompletion(t.Replace("\\b", "")))
                                                    .Where(item => !items.Contains(item) && char.IsLetter(item.Text, 0))
                                            select item)
            {
                items.Add(item);
            }

            return items.ToArray();
        }


        private IEnumerable<ICompletionData> ObjectBrowserCompletionList()
        {
            return (from v in FileLanguage.Fields
                    where (v.Type != "def") && (v.Type != "deffct")
                    select new CodeCompletion(v.Name) { Image = v.Icon }).Cast<ICompletionData>().ToArray();
        }

        #endregion

        #endregion

        #region Search Replace Section

        public void ReplaceAll()
        {
            Regex r = FindReplaceViewModel.Instance.RegexPattern;
            Match m = r.Match(Text);
            while (m.Success)
            {
                Document.GetLineByOffset(m.Index);
                r.Replace(FindReplaceViewModel.Instance.LookFor, FindReplaceViewModel.Instance.ReplaceWith, m.Index);
                m = m.NextMatch();
            }
        }

        public void ReplaceText()
        {
            FindText();
            SelectedText = SelectedText.Replace(SelectedText, FindReplaceViewModel.Instance.ReplaceWith);
        }

        public void FindText()
        {
            int nIndex = Text.IndexOf(FindReplaceViewModel.Instance.LookFor, CaretOffset, StringComparison.Ordinal);
            if (nIndex > -1)
            {
                Document.GetLineByOffset(nIndex);
                JumpTo(new Variable { Offset = nIndex });
                SelectionStart = nIndex;
                SelectionLength = FindReplaceViewModel.Instance.LookFor.Length;
            }
            else
            {
                FindReplaceViewModel.Instance.SearchResult = "No Results Found, Starting Search from Beginning";
                CaretOffset = 0;
            }
        }

        private void JumpTo(IVariable i)
        {
            TextLocation c = Document.GetLocation(Convert.ToInt32(i.Offset));

            ScrollTo(c.Line, c.Column);
            SelectionStart = Convert.ToInt32(i.Offset);
            SelectionLength = i.Value.Length;
            Focus();
            if (EditorOptions.Instance.EnableAnimations)
                Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)DisplayCaretHighlightAnimation);
        }


        private void DisplayCaretHighlightAnimation()
        {
            if (TextArea == null)
                return;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(TextArea.TextView);

            if (layer == null)
                return;

            var adorner = new CaretHighlightAdorner(TextArea);
            layer.Add(adorner);
        }


        public void SelectText(IVariable var)
        {
            if (var == null) return;
            if (var.Name == null) throw new ArgumentNullException("var");

            DocumentLine d = Document.GetLineByOffset(var.Offset);
            TextArea.Caret.BringCaretToView();
            CaretOffset = d.Offset;
            ScrollToLine(d.LineNumber);


            ReadOnlyCollection<FoldingSection> f = _foldingManager.GetFoldingsAt(d.Offset);
            if (f.Count > 0)
            {
                FoldingSection fs = f[0];
                fs.IsFolded = false;
            }

            FindText(var.Offset, var.Name);
            JumpTo(var);
        }

        private void FindText(int startOffset, string text)
        {
            int start = Text.IndexOf(text, startOffset, StringComparison.OrdinalIgnoreCase);
            SelectionStart = start;
            SelectionLength = text.Length;
        }

        public void FindText(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            SelectionStart = Text.IndexOf(text, CaretOffset, StringComparison.Ordinal);
        }

        public void ShowFindDialog()
        {
            //TODO Remove this if new Find and replace form works.
            //TODO Test this
            // FindAndReplaceForm.Instance.ShowDialog();
            FindandReplaceControl.Instance.ShowDialog();
        }

        #endregion

        #region Editor.Bindings

        private void AddBindings()
        {
            InputBindingCollection inputBindings = TextArea.InputBindings;
            inputBindings.Add(new KeyBinding(ApplicationCommands.Find, Key.F, ModifierKeys.Control));
            inputBindings.Add(new KeyBinding(ApplicationCommands.Replace, Key.R, ModifierKeys.Control));
        }

        protected override bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            switch (managerType.Name)
            {
                case "TextChanged":
                    FindBookmarkMembers();
                    //IsModified = true;
                    UpdateFolds();
                    break;
            }
            return base.ReceiveWeakEvent(managerType, sender, e);
        }


        public void ChangeIndent(object param)
        {
            try
            {
                bool increase = Convert.ToBoolean(param);

                DocumentLine start = Document.GetLineByOffset(SelectionStart);
                DocumentLine end = Document.GetLineByOffset(SelectionStart + SelectionLength);
                int positions = 0;
                using (Document.RunUpdate())
                {
                    for (DocumentLine line = start; line.LineNumber < end.LineNumber + 1; line = line.NextLine)
                    {
                        string currentline = GetLine(line.LineNumber);
                        var rgx = new Regex(@"(^[\s]+)");

                        Match m = rgx.Match(currentline);
                        if (m.Success)
                            positions = m.Groups[1].Length;


                        if (increase)
                            Document.Insert(line.Offset + positions, " ");
                        else
                        {
                            positions = positions > 1 ? positions - 1 : positions;
                            if (positions >= 1)
                                Document.Replace(line.Offset, currentline.Length, currentline.Substring(1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageViewModel.AddError("Editor.ChangeIndent", ex);
            }
        }


        /// <summary>
        ///     Evaluates each line in selection and Comments/Uncomments "Each Line"
        /// </summary>
        private void ToggleComment()
        {
            //No point in commenting if I dont know the Language
            if (FileLanguage == null) return;

            // Get Comment to insert
            DocumentLine start = Document.GetLineByOffset(SelectionStart);
            DocumentLine end = Document.GetLineByOffset(SelectionStart + SelectionLength);

            using (Document.RunUpdate())
            {
                for (DocumentLine line = start; line.LineNumber < end.LineNumber + 1; line = line.NextLine)
                {
                    string currentline = GetLine(line.LineNumber);

                    // Had to put in comment offset for Fanuc 
                    if (FileLanguage.IsLineCommented(currentline))
                        Document.Insert(FileLanguage.CommentOffset(currentline) + line.Offset,
                            FileLanguage.CommentChar);
                    else
                    {
                        string replacestring = FileLanguage.CommentReplaceString(currentline);
                        Document.Replace(line.Offset, currentline.Length, replacestring);
                    }
                }
            }
        }


        private bool CanSave()
        {
            return File.Exists(Filename) ? IsModified : IsModified;
        }

        private void Replace()
        {
            //TODO Replaced the Find and replace form with the find and replace control that will parse through files as well.
            // Make sure we update the Editor _instance           
            //            FindAndReplaceForm.Instance = new FindAndReplaceForm{Left = Mouse.GetPosition(this).X, Top = Mouse.GetPosition(this).Y};
            //           FindAndReplaceForm.Instance.Show();
            FindandReplaceControl.Instance.Left = Mouse.GetPosition(this).X;
            FindandReplaceControl.Instance.Top = Mouse.GetPosition(this).Y;
        }


        private void Goto()
        {
            var vm = new GotoViewModel(this);
            var gtd = new GotoDialog { DataContext = vm };
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            gtd.ShowDialog().GetValueOrDefault();
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }


        private bool IsFileLocked(System.IO.FileInfo file)
        {
            FileStream stream = null;

            if (!File.Exists(file.FullName)) return false;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException ex)
            {
                MessageViewModel.AddError("File is locked!", ex);

                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }


        private string GetFilename()
        {
            var ofd = new SaveFileDialog { Title = "Save As", Filter = "All _files(*.*)|*.*" };

            if (!String.IsNullOrEmpty(Filename))
            {
                ofd.FileName = Filename;
                ofd.Filter += String.Format("|Current Type (*{0})|*{0}", Path.GetExtension(Filename));
                ofd.FilterIndex = 2;
                ofd.DefaultExt = Path.GetExtension(Filename);
            }

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            bool result = ofd.ShowDialog().GetValueOrDefault();
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
            return result ? ofd.FileName : string.Empty;
        }

        public void SaveAs()
        {
            string result = GetFilename();
            if (string.IsNullOrEmpty(result))
                return;
            Filename = result;
            bool islocked = IsFileLocked(new System.IO.FileInfo(Filename));

            if (islocked)
                return;

            File.WriteAllText(Filename, Text);

            OnPropertyChanged("Title");

            MessageViewModel.Add(new OutputWindowMessage { Title = "File Saved", Description = Filename, Icon = null });
        }


        private void Save()
        {
            //_watcher.EnableRaisingEvents = false;
            // _watcher.Text = Text;


            if (String.IsNullOrEmpty(Filename))
            {
                string result = GetFilename();
                if (string.IsNullOrEmpty(result))
                    return;
                Filename = result;
            }

            if (IsFileLocked(new System.IO.FileInfo(Filename))) return;

            File.WriteAllText(Filename, Text);

            FileSave = File.GetLastWriteTime(Filename).ToString(CultureInfo.InvariantCulture);
            IsModified = false;
        }

        #endregion

        #region CaretPositionChanged - Bracket Highlighting

        private readonly MyBracketSearcher _bracketSearcher = new MyBracketSearcher();
        private BracketHighlightRenderer _bracketRenderer;

        /// <summary>
        ///     Highlights matching brackets.
        /// </summary>
// ReSharper disable UnusedParameter.Local
        private void HighlightBrackets(object sender, EventArgs e)
        // ReSharper restore UnusedParameter.Local
        {
            /*
             * Special case: ITextEditor.Language guarantees that it never returns null.
             * In this case however it can be null, since this code may be called while the document is loaded.
             * ITextEditor.Language gets set in CodeEditorAdapter.FileNameChanged, which is called after
             * loading of the document has finished.
             * */


            BracketSearchResult bracketSearchResult = _bracketSearcher.SearchBracket(Document, TextArea.Caret.Offset);
            _bracketRenderer.SetHighlight(bracketSearchResult);
        }


        private void CaretPositionChanged(object sender, EventArgs e)
        {
            var s = sender as Caret;

            UpdateLineTransformers();
            if (s != null)
            {
                OnPropertyChanged("Line");
                OnPropertyChanged("Column");
                OnPropertyChanged("Offset");
                FileSave = !String.IsNullOrEmpty(Filename)
                    ? File.GetLastWriteTime(Filename).ToString(CultureInfo.InvariantCulture)
                    : String.Empty;
            }

            HighlightBrackets(sender, e);
        }

        /// <summary>
        /// </summary>
        private void UpdateLineTransformers()
        {
            // Clear the Current Renderers
            TextArea.TextView.BackgroundRenderers.Clear();
            var textEditorOptions = Options as EditorOptions;

            if (textEditorOptions != null && textEditorOptions.HighlightCurrentLine)
                TextArea.TextView.BackgroundRenderers.Add(new BackgroundRenderer(Document.GetLineByOffset(CaretOffset)));

            if (_bracketRenderer == null)
                _bracketRenderer = new BracketHighlightRenderer(TextArea.TextView);
            else
                TextArea.TextView.BackgroundRenderers.Add(_bracketRenderer);
        }

        #endregion

        #region Overrides

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (_lastKeyUpArgs == null)
            {
                _lastKeyUpArgs = e;
                return;
            }

            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.Control:
                    switch (e.Key)
                    {
                        case Key.O:
                            if (!(String.IsNullOrEmpty(FileLanguage.CommentChar)))
                                ToggleFolds();
                            break;
                    }
                    break;
            }


            // save argument for next event
            _lastKeyUpArgs = e;

            // call base handler
            base.OnKeyUp(e);
        }

        #endregion
    }

    /// <summary>
    ///     Animated rectangle around the caret.
    /// </summary>
    internal sealed class CaretHighlightAdorner : Adorner
    {
        private readonly RectangleGeometry _geometry;
        private readonly Pen _pen;

        public CaretHighlightAdorner(TextArea textArea)
            : base(textArea.TextView)
        {
            Rect min = textArea.Caret.CalculateCaretRectangle();
            min.Offset(-textArea.TextView.ScrollOffset);

            Rect max = min;
            double size = Math.Max(min.Width, min.Height) * 0.25;
            max.Inflate(size, size);

            _pen = new Pen(TextBlock.GetForeground(textArea.TextView).Clone(), 1);

            _geometry = new RectangleGeometry(min, 2, 2);
            _geometry.BeginAnimation(RectangleGeometry.RectProperty,
                new RectAnimation(min, max, new Duration(TimeSpan.FromMilliseconds(300))) { AutoReverse = true });
            _pen.Brush.BeginAnimation(Brush.OpacityProperty,
                new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(200)))
                {
                    BeginTime = TimeSpan.FromMilliseconds(450)
                });
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawGeometry(null, _pen, _geometry);
        }
    }

    public enum EDITORTYPE
    {
        SOURCE,
        DATA
    };
}
}
