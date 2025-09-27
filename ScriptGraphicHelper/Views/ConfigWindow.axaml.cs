using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Newtonsoft.Json;

using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Tools;

using System;
using System.IO;

namespace ScriptGraphicHelper.Views
{
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            this.InitializeComponent();
        }

        private void Window_Opened(object sender, EventArgs e)
        {
            AddRange.IsChecked = Settings.Instance.AddRange;
            AddInfo.IsChecked = Settings.Instance.AddInfo;
            IsOffset.IsChecked = Settings.Instance.IsOffset;
            DiySim.Text = Settings.Instance.DiySim.ToString();
            DmRegcode.Text = Settings.Instance.DmRegcode;
        }

        private void Ok_Tapped(object sender, RoutedEventArgs e)
        {
            Settings.Instance.AddRange = AddRange.IsChecked ?? false;
            Settings.Instance.AddInfo = AddInfo.IsChecked ?? false;
            Settings.Instance.IsOffset = IsOffset.IsChecked ?? false;

            Settings.Instance.DmRegcode = DmRegcode.Text ?? string.Empty;

            if (int.TryParse(DiySim.Text.Trim(), out var sim))
            {
                Settings.Instance.DiySim = sim;
            }

            SettingsTools.SaveSettings();

            Close();
        }
    }
}
