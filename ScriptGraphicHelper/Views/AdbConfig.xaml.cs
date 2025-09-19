using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace ScriptGraphicHelper.Views
{
    public class AdbConfig : Window
    {
        private static string LastAddress;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        }

        public AdbConfig()
        {
            InitializeComponent();
        }

        private void WindowOpened(object sender, EventArgs e)
        {
            this.FindControl<TextBox>("Address").Text = LastAddress ?? "192.168.";
        }

        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            var address = this.FindControl<TextBox>("Address").Text.Trim();
            LastAddress = address;
            var port = int.Parse(this.FindControl<TextBox>("Port").Text.Trim());
            Close((address, port));
        }

        private void Skip_Tapped(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            switch (key)
            {
                case Key.Enter:
                    var address = this.FindControl<TextBox>("Address").Text.Trim();
                    LastAddress = address;
                    var port = int.Parse(this.FindControl<TextBox>("Port").Text.Trim());
                    Close((address, port));
                    break;

                case Key.Escape: Close(); break;

                default: return;
            }
            e.Handled = true;
        }

    }
}
