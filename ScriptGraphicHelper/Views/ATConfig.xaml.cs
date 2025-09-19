using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace ScriptGraphicHelper.Views
{
    public class ATConfig : Window
    {
        private static string remoteAddress = "192.168.";

        public ATConfig()
        {
            InitializeComponent();
            this.FindControl<TextBox>("RemoteAddress").Text = remoteAddress;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            var address = this.FindControl<TextBox>("RemoteAddress").Text.Trim();
            remoteAddress = address;
            Close(address);
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
                    var address = this.FindControl<TextBox>("RemoteAddress").Text.Trim();
                    remoteAddress = address;
                    Close(address);
                    break;

                case Key.Escape: Close(); break;

                default: return;
            }
            e.Handled = true;
        }

    }
}
