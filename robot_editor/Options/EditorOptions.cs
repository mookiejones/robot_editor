using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using robot_editor.Interfaces;

namespace robot_editor.Options
{
 
    [Localizable(false)]
    [Serializable]
    public sealed class EditorOptions : TextEditorOptions, IOptions
    {

        #region · Fields ·

     
        private static EditorOptions _instance;
        [NonSerialized]
        private bool _allowScrollingBelowDocument;
        [NonSerialized]
        private Color _backgroundColor = Colors.White;
        private Color _borderColor = Colors.Transparent;

        private double _borderThickness;
        private bool _enableAnimations = true;
        private bool _enableFolding = true;
        private Color _foldToolTipBackgroundBorderColor = Colors.WhiteSmoke;
        [NonSerialized]
        private Color _foldToolTipBackgroundColor = Colors.Red;
        private double _foldToolTipBorderThickness = 1.0;
        [NonSerialized]
        private Color _fontColor = Colors.Black;
        private bool _highlightcurrentline = true;
        [NonSerialized]
        private Color _lineNumbersFontColor = Colors.Gray;
        private Color _lineNumbersForeground = Colors.Gray;
        private bool _mouseWheelZoom = true;
        [NonSerialized]
        private Color _selectedBorderColor = Colors.Orange;
        private double _selectedBorderThickness = 1.0;
        [NonSerialized]
        private Color _selectedFontColor = Colors.White;
        [NonSerialized]
        private Color _selectedTextBackground = Colors.SteelBlue;
        [NonSerialized]
        private Color _selectedTextBorderColor = Colors.Orange;
        [NonSerialized]
        private Color _selectedlinecolor = Colors.Yellow;
        private bool _showlinenumbers = true;
        private string _timestampFormat = "ddd MMM d hh:mm:ss yyyy";
        private bool _wrapWords = true;
        #endregion

        private EditorOptions()
        {
            RegisterSyntaxHighlighting();


        }


        
     
//        private static string OptionsPath
//        {
//
//            get { return Path.Combine(App.StartupPath, "Options.xml"); }
//        }

        public static EditorOptions Instance
        {
            get { return _instance ?? (_instance = ReadXml()); }
            set { _instance = value; }
        }

        public override bool ShowSpaces
        {
            get { return base.ShowSpaces; }
            set
            {
                base.ShowSpaces = value;
                OnPropertyChanged("ShowSpaces");
            }
        }

        public Color SelectedTextBackground
        {
            get { return _selectedTextBackground; }
            set
            {
                _selectedTextBackground = value;
                OnPropertyChanged("SelectedTextBackground");
            }
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                OnPropertyChanged("BackgroundColor");
            }
        }

        public Color FontColor
        {
            get { return _fontColor; }
            set
            {
                _fontColor = value;
                OnPropertyChanged("FontColor");
            }
        }

        public Color SelectedFontColor
        {
            get { return _selectedFontColor; }
            set
            {
                _selectedFontColor = value;
                OnPropertyChanged("SelectedFontColor");
            }
        }

        public Color SelectedBorderColor
        {
            get { return _selectedBorderColor; }
            set
            {
                _selectedBorderColor = value;
                OnPropertyChanged("SelectedBorderColor");
            }
        }

        public bool AllowScrollingBelowDocument
        {
            get { return _allowScrollingBelowDocument; }
            set
            {
                _allowScrollingBelowDocument = value;
                OnPropertyChanged("AllowScrollingBelowDocument");
            }
        }

        public Color LineNumbersFontColor
        {
            get { return _lineNumbersFontColor; }
            set
            {
                _lineNumbersFontColor = value;
                OnPropertyChanged("LineNumbersFontColor");
            }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                OnPropertyChanged("BorderColor");
            }
        }

        public Color LineNumbersForeground
        {
            get { return _lineNumbersForeground; }
            set
            {
                _lineNumbersForeground = value;
                OnPropertyChanged("LineNumbersForeground");
            }
        }

        public Color SelectedTextBorderColor
        {
            get { return _selectedTextBorderColor; }
            set
            {
                _selectedTextBorderColor = value;
                OnPropertyChanged("SelectedTextBorderColor");
            }
        }

        public double SelectedBorderThickness
        {
            get { return _selectedBorderThickness; }
            set
            {
                _selectedBorderThickness = value;
                OnPropertyChanged("SelectedBorderThickness");
            }
        }

        public double BorderThickness
        {
            get { return _borderThickness; }
            set
            {
                _borderThickness = value;
                OnPropertyChanged("BorderThickness");
            }
        }

        public Color HighlightedLineColor
        {
            get { return _selectedlinecolor; }
            set
            {
                _selectedlinecolor = value;
                OnPropertyChanged("HighlightedLineColor");
            }
        }

        public Color FoldToolTipBackgroundColor
        {
            get { return _foldToolTipBackgroundColor; }
            set
            {
                _foldToolTipBackgroundColor = value;
                OnPropertyChanged("FoldToolTipBackgroundColor");
            }
        }

        public Color FoldToolTipBackgroundBorderColor
        {
            get { return _foldToolTipBackgroundBorderColor; }
            set
            {
                _foldToolTipBackgroundBorderColor = value;
                OnPropertyChanged("FoldToolTipBackgroundBorderColor");
            }
        }

        public double FoldToolTipBorderThickness
        {
            get { return _foldToolTipBorderThickness; }
            set
            {
                _foldToolTipBorderThickness = value;
                OnPropertyChanged("FoldToolTipBorderThickness");
            }
        }

        public bool WrapWords
        {
            get { return _wrapWords; }
            set
            {
                _wrapWords = value;
                OnPropertyChanged("WrapWords");
            }
        }

        public string TimestampFormat
        {
            get { return _timestampFormat; }
            set
            {
                _timestampFormat = value;
                OnPropertyChanged("TimestampFormat");
                OnPropertyChanged("TimestampSample");
            }
        }

        public string TimestampSample => DateTime.Now.ToString(_timestampFormat);

        public new bool HighlightCurrentLine
        {
            get { return _highlightcurrentline; }
            set { _highlightcurrentline = value; }
        }

        [DefaultValue(true)]
        public bool EnableFolding
        {
            get { return _enableFolding; }
            set
            {
                _enableFolding = value;
                OnPropertyChanged("EnableFolding");
            }
        }

        [DefaultValue(true)]
        public bool MouseWheelZoom
        {
            get { return _mouseWheelZoom; }
            set
            {
                if (_mouseWheelZoom != value)
                {
                    _mouseWheelZoom = value;
                    OnPropertyChanged("MouseWheelZoom");
                }
            }
        }

        public bool EnableAnimations
        {
            get { return _enableAnimations; }
            set
            {
                _enableAnimations = value;
                OnPropertyChanged("EnableAnimations");
            }
        }

        public bool ShowLineNumbers
        {
            get { return _showlinenumbers; }
            set
            {
                _showlinenumbers = value;
                OnPropertyChanged("ShowLineNumbers");
            }
        }

        public string Title => "Text Editor Options";

        ~EditorOptions()
        {
#if !DEBUG
            WriteXml();
#endif
        }

        private void WriteXml(string path)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(EditorOptions));
                using (var textWriter = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(textWriter, this);

                }

            }
            catch (Exception)
            {
                // ignored
            }
        }
        private void WriteXml()
        {
            var path = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Options.xml");
            WriteXml(path);
        }

        private static EditorOptions ReadXml(string path)
        {

            var editorOptions = new EditorOptions();
            EditorOptions result;
            if (!File.Exists(path))
            {
                result = editorOptions;
            }
            else
            {
                var xmlSerializer = new XmlSerializer(typeof(EditorOptions));
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    try
                    {
                        editorOptions = (EditorOptions)xmlSerializer.Deserialize(fileStream);
                    }
                    catch
                    {
                    }

                }

                result = editorOptions;
            }
            return result;
        }
        private static EditorOptions ReadXml()
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(EditorOptions).Assembly.Location), "Options.xml");
            return ReadXml(path);

        }

        [Localizable(false)]
        private static void Register(string name, string[] ext)
        {

            var filename = string.Format("SimulationWorkflow.Resources.SyntaxHighlighting.{0}Highlight.xshd", name.ToUpperInvariant());

            using (var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename))
            {
                if (manifestResourceStream == null)
                {
                    throw new InvalidOperationException("Could not find embedded resource");
                }

                IHighlightingDefinition highlighting;

                using (var xmlTextReader = new XmlTextReader(manifestResourceStream))
                {
                    highlighting = HighlightingLoader.Load(xmlTextReader, HighlightingManager.Instance);
                }
                HighlightingManager.Instance.RegisterHighlighting(name, ext, highlighting);
            }
        }

        private static void RegisterSyntaxHighlighting()
        {
            Register("KUKA", new[]  {
                    ".dat",
                    ".src",
                    ".ini",
                    ".sub",
                    ".zip",
                    ".kfd"
                });
            Register("KAWASAKI", new[]     {
                    ".as",
                    ".prg"
                });
            Register("Fanuc", new[] { ".ls" });
            Register("ABB", new[]{ ".mod",
                    ".prg"});
        }
    }
}
