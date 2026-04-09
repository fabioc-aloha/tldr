using System.IO;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Appearance;

namespace Tldr.Platform;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch unhandled exceptions so the app doesn't vanish silently
        DispatcherUnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
                LogFatal("AppDomain", ex);
        };
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            LogFatal("UnobservedTask", args.Exception);
            args.SetObserved();
        };

        if (e.Args.Length > 0 && e.Args[0] == "--smoke-test")
        {
            try
            {
                await SmokeTest.RunAsync();
                Shutdown(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FAIL] {ex.Message}");
                Shutdown(1);
            }
            return;
        }

        // Follow system dark/light theme
        ApplicationThemeManager.ApplySystemTheme();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogFatal("Dispatcher", e.Exception);
        e.Handled = true; // Prevent crash; app may be in bad state but user sees the error
        System.Windows.MessageBox.Show($"An unexpected error occurred:\n{e.Exception.Message}", "TL;DR", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private static void LogFatal(string source, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[FATAL:{source}] {ex}");
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TLDR");
            Directory.CreateDirectory(logDir);
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL:{source}] {ex}\n";
            File.AppendAllText(Path.Combine(logDir, "errors.log"), entry);
        }
        catch { /* logging must not throw */ }
    }
}
