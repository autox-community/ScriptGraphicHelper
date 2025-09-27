using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScriptGraphicHelper.Views
{
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            this.InitializeComponent();
        }

        private void Address_Tapped(object sender, RoutedEventArgs e)
        {
            var url = "https://github.com/autox-community/ScriptGraphicHelper";
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