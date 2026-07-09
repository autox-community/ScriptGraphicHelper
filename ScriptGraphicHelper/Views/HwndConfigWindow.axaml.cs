using Avalonia.Controls;

using ScriptGraphicHelper.ViewModels;

namespace ScriptGraphicHelper.Views
{
    public partial class HwndConfigWindow : Window
    {
        public HwndConfigWindow()
        {
            this.InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            var tv = this.FindControl<TreeView>("HwndInfos");
            this.DataContext = new HwndConfigViewModel(this);
        }
    }
}
