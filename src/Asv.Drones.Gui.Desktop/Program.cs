using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppArgs.Instance.TryParse(args);
        AppArgs.Instance.TryParseFile();
        // This is for catch unhandled exception
        // https://docs.avaloniaui.net/ru/docs/concepts/unhandledexceptions
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            if (Debugger.IsAttached) Debugger.Break();
        }
        finally
        {
            // Ensure the logs are flushed and archived before exiting the application
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // Windows
            .With(new Win32PlatformOptions { OverlayPopups = true })
            // Unix/Linux
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false })
            // Mac
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true })
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}