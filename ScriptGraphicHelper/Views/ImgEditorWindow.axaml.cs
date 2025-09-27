using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.ViewModels;
using System.Collections.Generic;
using MyRange = ScriptGraphicHelper.Models.MyRange;

namespace ScriptGraphicHelper.Views
{
    public partial class ImgEditorWindow : Window
    {
        public static bool Result_ACK { get; set; } = false;
        public static List<ColorInfo>? ResultColorInfos { get; set; }
        public ImgEditorWindow()
        {
            this.InitializeComponent();
        }
        public ImgEditorWindow(MyRange range, byte[] data)
        {
            this.InitializeComponent();
            this.DataContext = new ImgEditorViewModel(range, data);
        }
        private void ACK_Tapped(object sender, RoutedEventArgs e)
        {
            Result_ACK = true;
            Close();
        }
    }
}
