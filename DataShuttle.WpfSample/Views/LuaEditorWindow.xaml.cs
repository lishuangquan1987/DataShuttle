using DataShuttle.WpfSample.ViewModels;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace DataShuttle.WpfSample.Views
{
    public partial class LuaEditorWindow : Window
    {
        private CompletionWindow _completionWindow;
        private LuaEditorViewModel VM => DataContext as LuaEditorViewModel;

        public string ResultScript { get; private set; }
        public bool Saved { get; private set; }

        public LuaEditorWindow(string title, string script)
        {
            InitializeComponent();

            var vm = new LuaEditorViewModel { Script = script };
            vm.SaveRequested += s => { ResultScript = s; Saved = true; Close(); };
            vm.CancelRequested += Close;
            // Script 被 Reset 命令改变时，同步回 AvalonEdit
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(LuaEditorViewModel.Script) && Editor.Text != vm.Script)
                    Editor.Text = vm.Script ?? "";
            };
            DataContext = vm;

            TitleText.Text = title;
            LoadHighlighting();
            Editor.Text = string.IsNullOrEmpty(script) ? vm.Script : script;

            // AvalonEdit 事件 → ViewModel（AvalonEdit 不支持 XAML Behavior 绑定）
            Editor.TextChanged += (_, _) => VM?.SyncScript(Editor.Text);
            Editor.TextArea.Caret.PositionChanged += (_, _) =>
            {
                var p = Editor.TextArea.Caret.Position;
                VM?.UpdateCaretStatus(p.Line, p.Column);
            };

            // 补全
            Editor.TextArea.TextEntered += TextArea_TextEntered;
            Editor.TextArea.TextEntering += TextArea_TextEntering;
        }

        private void LoadHighlighting()
        {
            var asm = typeof(LuaEditorWindow).Assembly;
            var res = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("Lua.xshd"));
            if (res == null) return;
            using var stream = asm.GetManifestResourceStream(res);
            using var reader = new XmlTextReader(stream);
            Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        // ── 智能补全（纯 View 职责）────────────────────────────────
        private static readonly string[] Completions =
        {
            "and","break","do","else","elseif","end","false","for","function","goto",
            "if","in","local","nil","not","or","repeat","return","then","true","until","while",
            "print(","tostring(","tonumber(","type(","pairs(","ipairs(","error(","assert(",
            "pcall(","string.format(","string.byte(","string.char(","string.sub(","string.len(",
            "table.insert(","table.remove(","table.concat(",
            "math.floor(","math.ceil(","math.abs(","math.max(","math.min(",
            "bit.band(","bit.bor(","bit.bxor(","bit.lshift(","bit.rshift(",
            "data","intercept(","log("
        };

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
                if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_' && e.Text[0] != '.')
                    _completionWindow.CompletionList.RequestInsertion(e);
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsLetter(e.Text[0]) && e.Text[0] != '_') return;
            var word = GetCurrentWord();
            if (word.Length < 1) return;

            var candidates = Completions
                .Where(k => k.StartsWith(word, StringComparison.OrdinalIgnoreCase) && k != word)
                .ToList();
            if (candidates.Count == 0) return;

            _completionWindow = new CompletionWindow(Editor.TextArea) { Width = 260 };
            foreach (var c in candidates)
                _completionWindow.CompletionList.CompletionData.Add(new LuaCompletionData(c));
            _completionWindow.Show();
            _completionWindow.Closed += (_, _) => _completionWindow = null;
        }

        private string GetCurrentWord()
        {
            var doc = Editor.Document;
            int offset = Editor.CaretOffset, start = offset;
            while (start > 0)
            {
                char c = doc.GetCharAt(start - 1);
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '.') break;
                start--;
            }
            return doc.GetText(start, offset - start);
        }
    }

    internal class LuaCompletionData : ICompletionData
    {
        public LuaCompletionData(string text) { Text = text; }
        public System.Windows.Media.ImageSource Image => null;
        public string Text { get; }
        public object Content => Text;
        public object Description => null;
        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs e)
        {
            var doc = textArea.Document;
            int start = completionSegment.Offset;
            while (start > 0 && (char.IsLetterOrDigit(doc.GetCharAt(start - 1))
                || doc.GetCharAt(start - 1) == '_' || doc.GetCharAt(start - 1) == '.'))
                start--;
            doc.Replace(start, completionSegment.EndOffset - start, Text);
        }
    }
}
