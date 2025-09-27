using System;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Tmds.DBus.Protocol;

namespace ScriptGraphicHelper.Views
{
    public partial class AdbConfigWindow : Window
    {
        private static string? _lastAddress;

        public AdbConfigWindow()
        {
            this.InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        /// <summary>
        /// 窗口已经打开_事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowOpened(object sender, EventArgs e)
        {
            // 设置默认值
            Address_TextBox.Text = _lastAddress ?? "192.168.";
        }

        /// <summary>
        /// 确定_点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            Ok();
        }

        private void Ok()
        {
            var address = Address_TextBox.Text.Trim();

            _lastAddress = address;

            var port = int.Parse(Port_TextBox.Text.Trim());

            // 关闭弹窗,并返回数据
            Close($"true,{address},{port}");
        }

        /// <summary>
        /// 跳过_点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Skip_Tapped(object sender, RoutedEventArgs e)
        {
            Close("false");
        }

        /// <summary>
        /// 快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            switch (key)
            {
                // 回车键
                case Key.Enter:
                    Ok();
                    break;

                // ESC
                case Key.Escape:
                    Close();
                    break;

                default: return;
            }
            e.Handled = true;
        }

    }
}
