using DataShuttle.WpfSample.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DataShuttle.WpfSample.Views
{
    public partial class ScriptEditorView : UserControl
    {
        public ScriptEditorView()
        {
            InitializeComponent();
        }

        private ScriptEditorViewModel VM => DataContext as ScriptEditorViewModel;

        private void EditFromScript_Click(object sender, RoutedEventArgs e)
        {
            if (VM == null) return;
            var win = new LuaEditorWindow("左 → 右  拦截脚本", VM.FromScript)
            {
                Owner = Window.GetWindow(this)
            };
            if (win.ShowDialog() == true && win.Saved)
                VM.FromScript = win.ResultScript;
        }

        private void EditToScript_Click(object sender, RoutedEventArgs e)
        {
            if (VM == null) return;
            var win = new LuaEditorWindow("右 → 左  拦截脚本", VM.ToScript)
            {
                Owner = Window.GetWindow(this)
            };
            if (win.ShowDialog() == true && win.Saved)
                VM.ToScript = win.ResultScript;
        }
    }
}
