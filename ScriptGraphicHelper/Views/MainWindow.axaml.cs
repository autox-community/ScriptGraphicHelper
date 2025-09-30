using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.DependencyInjection;

using Newtonsoft.Json;

using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Tools;
using ScriptGraphicHelper.ViewModels;

namespace ScriptGraphicHelper.Views
{
    public partial class MainWindow : Window
    {
        public IntPtr Handle { get; private set; }

        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 窗口打开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Opened(object sender, EventArgs e)
        {
            // 拖放事件 (拖动图片到窗口,可以快速打开图片)
            AddHandler(DragDrop.DropEvent, (this.DataContext as MainWindowViewModel).DropImage_Event);

            this.Handle = this.TryGetPlatformHandle()?.Handle ?? -1;

            // 设置窗口大小
            this.ClientSize = new Size(Settings.Instance.Width, Settings.Instance.Height);
        }

        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            // 保存窗口的宽高
            if (this.WindowState != WindowState.FullScreen)
            {
                Settings.Instance.Width = this.Width;
                Settings.Instance.Height = this.Height;
            }

            SettingsTools.SaveSettings();
        }

        /// <summary>
        /// 方向键 控制鼠标移动 1 像素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 标题栏移拖动窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_DragMove(object sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        /// <summary>
        /// 最小化窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minsize_Tapped(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 显示信息窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Info_Tapped(object sender, RoutedEventArgs e)
        {
            var info = new InfoWindow();
            info.ShowDialog(this);
        }

        private double defaultWidth;
        private double defaultHeight;

        /// <summary>
        /// 最大化/还原 窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowStateChange_Tapped(object sender, RoutedEventArgs e)
        {
            // NOTE: Avalonia v11 可以给设置 name 的组件自动生成 Find 代码, 不用再通过 FindControl 手动获取
            var default_btn = Default_btn;
            var fullScreen_btn = FullScreen_btn;

            this.CanResize = true;

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

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Tapped(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
