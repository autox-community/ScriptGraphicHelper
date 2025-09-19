using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScriptGraphicHelper.Views
{
    public class Info : Window
    {
        public Info()
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

        private void AddQQ_Tapped(object sender, RoutedEventArgs e)
        {
            var url = "http://wpa.qq.com/msgrd?v=3&uin=1104573225&site=qq&menu=yes";
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"{url}" : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }

        private void JoinGroup_Tapped(object sender, RoutedEventArgs e)
        {
            var url = "https://qm.qq.com/cgi-bin/qm/qr?k=gbGRZ7zi6Vtv8Ei3S8Eiq3lxHAWuPTXq&jump_from=webapi";
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"{url}" : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }

        private void Address_Tapped(object sender, RoutedEventArgs e)
        {
            var url = "https://gitee.com/yiszza/ScriptGraphicHelper";
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"{url}" : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }
    }
}