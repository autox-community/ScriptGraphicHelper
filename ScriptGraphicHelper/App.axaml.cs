using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;

using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using ScriptGraphicHelper.ViewModels;
using ScriptGraphicHelper.Views;

namespace ScriptGraphicHelper
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {

                DisableAvaloniaDataAnnotationValidation();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                // IOC
                var serviceCollection = new ServiceCollection();
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel != null)
                {
                    // 将 toplevel 设置为单例,可以在 mvvm 中获取
                    serviceCollection.AddSingleton(topLevel);
                }
                var serviceProvider = serviceCollection.BuildServiceProvider();
                Ioc.Default.ConfigureServices(serviceProvider);
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
        /// More info: 
        /// https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
        /// </summary>
        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
