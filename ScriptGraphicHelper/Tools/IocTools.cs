using System;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.DependencyInjection;
using ScriptGraphicHelper.Views;

namespace ScriptGraphicHelper.Tools;

public static class IocTools
{
    public static MainWindow GetMainWindow()
    {
        return Ioc.Default.GetRequiredService<MainWindow>();
    }

    public static TopLevel GetTopLevel()
    {
        return Ioc.Default.GetRequiredService<TopLevel>();
    }

    public static IClipboard GetClipboard()
    {
        var tl = GetTopLevel();
        return tl.Clipboard ?? throw new NullReferenceException();
    }
}