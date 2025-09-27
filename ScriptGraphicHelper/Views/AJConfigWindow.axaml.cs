using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace ScriptGraphicHelper.Views
{
    public partial class AJConfigWindow : Window
    {
        private static string remoteAddress = "192.168.";
        public AJConfigWindow()
        {
            this.InitializeComponent();
        }

        private List<string> Addresses;

        public AJConfigWindow(List<string> addresses)
        {
            this.Addresses = addresses;
            this.InitializeComponent();
        }

        private void WindowOpened(object sender, EventArgs e)
        {
            var comboBox = this.FindControl<ComboBox>("AddressList");
            comboBox.ItemsSource = this.Addresses;

            foreach (var address in this.Addresses)
            {
                if (address.StartsWith("192.168"))
                {
                    comboBox.SelectedItem = address;
                    break;
                }
            }

            this.FindControl<TextBox>("RemoteAddress").Text = remoteAddress;
        }

        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            var address = (string)this.FindControl<ComboBox>("AddressList").SelectedItem;
            var remoteAddress = this.FindControl<TextBox>("RemoteAddress").Text.Trim();
            AJConfigWindow.remoteAddress = remoteAddress;
            Close((address, remoteAddress));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            switch (key)
            {
                case Key.Enter:
                    var address = (string)this.FindControl<ComboBox>("AddressList").SelectedItem;
                    var remoteAddress = this.FindControl<TextBox>("RemoteAddress").Text.Trim();
                    AJConfigWindow.remoteAddress = remoteAddress;
                    Close((address, remoteAddress));
                    break;

                case Key.Escape: Close(); break;

                default: return;
            }
            e.Handled = true;
        }

    }
}
