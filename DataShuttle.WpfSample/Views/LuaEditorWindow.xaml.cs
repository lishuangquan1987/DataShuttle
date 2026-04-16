using DataShuttle.WpfSample.Helpers;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace DataShuttle.WpfSample.Views
{
    public partial class LuaEditorWindow : Window
    {
        private static readonly string DefaultScript =
            "-- 参数 data: byte[]\r\n-- 返回 byte[] 继续转发，返回 nil 丢弃\r\nfunction intercept(data)\r\n    return data\r\nend";

        private CompletionWindow _completionWindow;

        /// <summary>保存后的脚本内容</summary>
        public string ResultScript { get; private set; }
        public bool Saved { get; private set; }

        public LuaEditorWindow(string title, string script)
        {
            InitializeComponent();
            TitleText.Text = title;
            LoadHighlighting();
            Editor.Text = string.IsNullOrEmpty(script) ? DefaultScript : script;
            Editor.TextArea.TextEntered += TextArea_TextEntered;
            Editor.TextArea.TextEntering += TextArea_TextEntering;
            KeyDown += (s, e) => { if (e.Key == Key.F5) RunTest(); };

            // 光标位置状态栏
            Editor.TextArea.Caret.PositionChanged += (s, e) =>
            {
                var pos = Editor.TextArea.Caret.Position;
                StatusText.Text = $"行 {pos.Line}  列 {pos.Column}";
            };
        }

        private void LoadHighlighting()
        {
            var asm = typeof(LuaEditorWindow).Assembly;
            var resourceName = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("Lua.xshd"));
            if (resourceName == null) return;

            using (var stream = asm.GetManifestResourceStream(resourceName))
            using (var reader = new XmlTextReader(stream))
            {
                Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }

        // ── 智能补全 ──────────────────────────────────────────────
        private static readonly string[] LuaKeywords =
        {
            "and", "break", "do", "else", "elseif", "end", "false", "for",
            "function", "goto", "if", "in", "local", "nil", "not", "or",
            "repeat", "return", "then", "true", "until", "while"
        };

        private static readonly string[] LuaBuiltins =
        {
            "print(", "tostring(", "tonumber(", "type(", "pairs(", "ipairs(",
            "next(", "select(", "error(", "assert(", "pcall(", "xpcall(",
            "setmetatable(", "getmetatable(", "rawget(", "rawset(",
            "string.format(", "string.len(", "string.sub(", "string.find(",
            "string.gsub(", "string.byte(", "string.char(", "string.rep(",
            "table.insert(", "table.remove(", "table.concat(", "table.sort(",
            "math.floor(", "math.ceil(", "math.abs(", "math.max(", "math.min(",
            "math.sqrt(", "math.random(",
            "bit.band(", "bit.bor(", "bit.bxor(", "bit.bnot(", "bit.lshift(", "bit.rshift("
        };

        private static readonly string[] DataShuttleApi =
        {
            "data", "intercept("
        };

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_' && e.Text[0] != '.')
                    _completionWindow.CompletionList.RequestInsertion(e);
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsLetter(e.Text[0]) && e.Text[0] != '_') return;

            // 获取当前输入的单词前缀
            var word = GetCurrentWord();
            if (word.Length < 1) return;

            var candidates = LuaKeywords
                .Concat(LuaBuiltins)
                .Concat(DataShuttleApi)
                .Where(k => k.StartsWith(word, StringComparison.OrdinalIgnoreCase) && k != word)
                .ToList();

            if (candidates.Count == 0) return;

            _completionWindow = new CompletionWindow(Editor.TextArea);
            _completionWindow.Width = 260;
            foreach (var c in candidates)
                _completionWindow.CompletionList.CompletionData.Add(new LuaCompletionData(c));

            _completionWindow.Show();
            _completionWindow.Closed += (o, args) => _completionWindow = null;
        }

        private string GetCurrentWord()
        {
            var doc = Editor.Document;
            int offset = Editor.CaretOffset;
            int start = offset;
            while (start > 0)
            {
                char c = doc.GetCharAt(start - 1);
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '.') break;
                start--;
            }
            return doc.GetText(start, offset - start);
        }

        // ── 测试 ──────────────────────────────────────────────────
        private void RunTest()
        {
            var hexInput = TestInputBox.Text.Trim();
            byte[] inputBytes;
            try
            {
                inputBytes = hexInput
                    .Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Convert.ToByte(s, 16))
                    .ToArray();
            }
            catch
            {
                TestOutputBox.Text = "⚠ 输入格式错误，请输入十六进制字节，如：01 02 03";
                TestOutputBox.Foreground = System.Windows.Media.Brushes.Orange;
                return;
            }

            using (var svc = new LuaInterceptorService())
            {
                if (!svc.Load(Editor.Text))
                {
                    TestOutputBox.Text = "✗ 脚本错误：\r\n" + svc.LastError;
                    TestOutputBox.Foreground = System.Windows.Media.Brushes.Salmon;
                    return;
                }

                var ctx = new DataShuttle.Core.Models.InterceptContext { Data = inputBytes };
                svc.Intercept(ctx);

                if (ctx.IsCancel)
                {
                    TestOutputBox.Text = "→ 数据已丢弃（返回 nil）";
                    TestOutputBox.Foreground = System.Windows.Media.Brushes.Orange;
                    return;
                }

                var hex = BitConverter.ToString(ctx.Data).Replace("-", " ");
                var ascii = Encoding.ASCII.GetString(
                    Array.ConvertAll(ctx.Data, b => b >= 32 && b < 127 ? b : (byte)'.'));
                TestOutputBox.Text = $"✓ 输出 [{ctx.Data.Length} 字节]\r\nHEX:   {hex}\r\nASCII: {ascii}";
                TestOutputBox.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
        }

        // ── 按钮事件 ──────────────────────────────────────────────
        private void BtnTest_Click(object sender, RoutedEventArgs e) => RunTest();

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("重置为默认模板？", "确认", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                Editor.Text = DefaultScript;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ResultScript = Editor.Text;
            Saved = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }

    // ── 补全数据项 ────────────────────────────────────────────────
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
            // 替换当前单词
            var doc = textArea.Document;
            int start = completionSegment.Offset;
            // 向前找单词起始
            while (start > 0 && (char.IsLetterOrDigit(doc.GetCharAt(start - 1)) || doc.GetCharAt(start - 1) == '_' || doc.GetCharAt(start - 1) == '.'))
                start--;
            doc.Replace(start, completionSegment.EndOffset - start, Text);
        }
    }
}
