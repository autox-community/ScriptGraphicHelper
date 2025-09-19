using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScriptGraphicHelper.Panels
{
    public class TitleBar : UserControl
    {
        public TitleBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
