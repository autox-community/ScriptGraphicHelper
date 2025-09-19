using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace ScriptGraphicHelper.Panels
{
    public class Img : UserControl
    {
        public Img()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Img_PointerEnter(object sender, PointerEventArgs e)
        {
            Focus();
        }
    }
}
