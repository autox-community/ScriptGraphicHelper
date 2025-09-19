using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScriptGraphicHelper.ViewModels;

namespace ScriptGraphicHelper.Views
{
    public class HwndConfig : Window
    {
        public HwndConfig()
        {
            InitializeComponent();
            this.DataContext = new HwndConfigViewModel(this);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            var tv = this.FindControl<TreeView>("HwndInfos");
        }
    }
}
