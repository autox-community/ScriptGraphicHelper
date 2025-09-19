using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels;
using System.Collections.Generic;
using Range = ScriptGraphicHelper.Models.Range;

namespace ScriptGraphicHelper.Views
{
    public class ImgEditor : Window
    {
        public static bool Result_ACK { get; set; } = false;
        public static List<ColorInfo>? ResultColorInfos { get; set; }
        public ImgEditor()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        public ImgEditor(Range range, byte[] data)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.DataContext = new ImgEditorViewModel(range, data);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ACK_Tapped(object sender, RoutedEventArgs e)
        {
            Result_ACK = true;
            Close();
        }
    }
}
