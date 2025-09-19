using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;

namespace ScriptGraphicHelper.Views
{
    public partial class MessageBox : Window
    {
        public static async void ShowAsync(string msg)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await new MessageBox(msg).ShowDialog(MainWindow.Instance);
            });
        }

        public static async void ShowAsync(string title, string msg)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await new MessageBox(title, msg).ShowDialog(MainWindow.Instance);
            });
        }

        public MessageBox()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private new string Title { get; set; } = string.Empty;
        private string Message { get; set; } = string.Empty;

        public MessageBox(string title, string msg) : this()
        {
            this.Title = title;
            this.Message = msg;

            this.ExtendClientAreaToDecorationsHint = true;
            this.ExtendClientAreaTitleBarHeightHint = -1;
            this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        }

        public MessageBox(string msg) : this("提示", msg) { }


        private void Window_Opened(object sender, EventArgs e)
        {
            var title = this.FindControl<TextBlock>("Title");
            title.Text = this.Title;
            var tb = this.FindControl<TextBlock>("Message");
            tb.Text = this.Message;


            this.MaxWidth = this.Screens.Primary.WorkingArea.Width * 0.9;
            this.MaxHeight = this.Screens.Primary.WorkingArea.Height * 0.8;

            tb.MaxWidth = this.MaxWidth - 100;
        }

        private async void Close_Tapped(object sender, RoutedEventArgs e)
        {
            await Application.Current.Clipboard.SetTextAsync(this.Message);
            Close();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await Application.Current.Clipboard.SetTextAsync(this.Message);
                Close();
            }
        }

        private void Window_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Width" || e.Property.Name == "Height")
            {
                this.Position = new PixelPoint((int)(this.Screens.Primary.WorkingArea.Width / 2 - this.Width / 2), (int)(this.Screens.Primary.WorkingArea.Height / 2 - this.Height / 2));
            }
        }
    }
}
