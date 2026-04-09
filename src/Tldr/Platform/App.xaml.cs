using System.Windows;
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
}
