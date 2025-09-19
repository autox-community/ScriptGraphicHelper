using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScriptGraphicHelper.Views;

namespace ScriptGraphicHelper.Panels
{
    public class Controls : UserControl
    {
        public Controls()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
