using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Newtonsoft.Json;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Models.UnmanagedMethods;
using ScriptGraphicHelper.ViewModels;
using System;
using System.ComponentModel;
using System.IO;

namespace ScriptGraphicHelper.Views
{
    public class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public IntPtr Handle { get; private set; }

        public MainWindow()
        {
            this.ExtendClientAreaToDecorationsHint = true;
            this.ExtendClientAreaTitleBarHeightHint = -1;
            this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            Instance = this;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FontWeight = Avalonia.Media.FontWeight.Medium;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private DispatcherTimer Timer = new();

        private void Window_Opened(object sender, EventArgs e)
        {
            AddHandler(DragDrop.DropEvent, (this.DataContext as MainWindowViewModel).DropImage_Event);
            this.Handle = this.PlatformImpl.Handle.Handle;
            this.ClientSize = new Size(Settings.Instance.Width, Settings.Instance.Height);
            this.Timer.Tick += new EventHandler(HintMessage_Closed);
            this.Timer.Interval = new TimeSpan(0, 0, 8);
            this.Timer.Start();
        }

        private void HintMessage_Closed(object? sender, EventArgs e)
        {
            var hint = this.FindControl<Border>("HintMessage");
            hint.IsVisible = false;
            this.Timer.IsEnabled = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.WindowState != WindowState.FullScreen)
            {
                Settings.Instance.Width = this.Width;
                Settings.Instance.Height = this.Height;
            }
            var settingStr = JsonConvert.SerializeObject(Settings.Instance, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"assets\settings.json", settingStr);

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            switch (key)
            {
                case Key.Left: NativeApi.Move2Left(); break;
                case Key.Up: NativeApi.Move2Top(); break;
                case Key.Right: NativeApi.Move2Right(); break;
                case Key.Down: NativeApi.Move2Bottom(); break;
                default: return;
            }
            e.Handled = true;
        }

        private void TitleBar_DragMove(object sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void Minsize_Tapped(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Info_Tapped(object sender, RoutedEventArgs e)
        {
            var info = new Info();
            info.ShowDialog(this);
            // this.WindowState = WindowState.Minimized;
        }

        private double defaultWidth;
        private double defaultHeight;
        private void WindowStateChange_Tapped(object sender, RoutedEventArgs e)
        {
            this.CanResize = true;
            var default_btn = this.FindControl<Button>("Default_btn");
            var fullScreen_btn = this.FindControl<Button>("FullScreen_btn");
            if (this.WindowState == WindowState.FullScreen)
            {
                default_btn.IsVisible = false;
                fullScreen_btn.IsVisible = true;
                this.WindowState = WindowState.Normal;

                this.Width = this.defaultWidth;
                this.Height = this.defaultHeight;
                var workingAreaSize = this.Screens.Primary.WorkingArea.Size;
                this.Position = new PixelPoint((int)((workingAreaSize.Width - this.Width) / 2), (int)((workingAreaSize.Height - this.Height) / 2));
            }
            else
            {
                this.defaultWidth = this.Width;
                this.defaultHeight = this.Height;
                default_btn.IsVisible = true;
                fullScreen_btn.IsVisible = false;
                this.WindowState = WindowState.FullScreen;
            }
        }

        private void Close_Tapped(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
