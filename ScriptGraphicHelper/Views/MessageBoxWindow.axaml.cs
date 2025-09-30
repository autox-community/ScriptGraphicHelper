using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.DependencyInjection;
using ScriptGraphicHelper.Tools;

namespace ScriptGraphicHelper.Views
{
    public partial class MessageBoxWindow : Window
    {
        public static async void ShowAsync(string msg)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await new MessageBoxWindow(msg).ShowDialog(IocTools.GetMainWindow());
            });
        }

        public static async void ShowAsync(string title, string msg)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await new MessageBoxWindow(title, msg).ShowDialog(IocTools.GetMainWindow());
            });
        }

        public MessageBoxWindow()
        {
            this.InitializeComponent();
        }
        private new string Title { get; set; } = string.Empty;
        private string Message { get; set; } = string.Empty;

        public MessageBoxWindow(string title, string msg) : this()
        {
            this.Title = title;
            this.Message = msg;

            this.ExtendClientAreaToDecorationsHint = true;
            this.ExtendClientAreaTitleBarHeightHint = -1;
            this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        }

        public MessageBoxWindow(string msg) : this("提示", msg) { }


        private void Window_Opened(object sender, EventArgs e)
        {
            var title = this.FindControl<TextBlock>("TitleTextBlock");
            title.Text = this.Title;
            var tb = this.FindControl<TextBlock>("MessageTextBlock");
            tb.Text = this.Message;


            this.MaxWidth = this.Screens.Primary.WorkingArea.Width * 0.9;
            this.MaxHeight = this.Screens.Primary.WorkingArea.Height * 0.8;

            tb.MaxWidth = this.MaxWidth - 100;
        }

        private async void Close_Tapped(object sender, RoutedEventArgs e)
        {
            var clipboard = IocTools.GetClipboard();
            await clipboard.SetTextAsync(this.Message);
            Close();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var clipboard = IocTools.GetClipboard();
                await clipboard.SetTextAsync(this.Message);
                Close();
            }
        }

        private void Window_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Width" || e.Property.Name == "Height")
            {
                this.Position = new PixelPoint(
                    (int)(this.Screens.Primary.WorkingArea.Width / 2 - this.Width / 2),
                    (int)(this.Screens.Primary.WorkingArea.Height / 2 - this.Height / 2)
                    );
            }
        }
    }
}
