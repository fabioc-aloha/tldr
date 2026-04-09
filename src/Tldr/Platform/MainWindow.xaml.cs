using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Tldr.Core;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Tldr.Platform;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _vm = new();
    private TrayIconManager? _tray;
    private HotkeyManager? _hotkeys;
    private bool _forceClose;

    public MainWindow()
    {
        DataContext = _vm;
        InitializeComponent();

        AllowDrop = true;
        Drop += OnDrop;
        DragOver += OnDragOver;
        DragLeave += (_, _) => DragOverlay.Visibility = Visibility.Collapsed;
        KeyDown += OnKeyDown;

        Loaded += async (_, _) =>
        {
            // Tray icon
            _tray = new TrayIconManager(
                showWindow: () => Dispatcher.Invoke(() => { Show(); Activate(); }),
                exitApp: () => Dispatcher.Invoke(() => { _forceClose = true; Close(); }));

            // Global hotkeys
            _hotkeys = new HotkeyManager();
            _hotkeys.OpenRequested += () => Dispatcher.Invoke(() => { Show(); Activate(); if (_vm.State == AppState.Ready && System.Windows.Clipboard.ContainsText()) _vm.LoadText(System.Windows.Clipboard.GetText()); });
            _hotkeys.StopRequested += () => Dispatcher.Invoke(() => _vm.StopTts());
            _hotkeys.Register(this);

            await _vm.InitializeAsync();
        };

        Closing += (_, e) =>
        {
            if (!_forceClose)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            _hotkeys?.Dispose();
            _tray?.Dispose();
            _vm.Dispose();
        };

        _vm.SentenceHighlightRequested += async n =>
        {
            try
            {
                await OutputWebView.EnsureCoreWebView2Async();
                await OutputWebView.ExecuteScriptAsync($"highlightSentence({n})");
            }
            catch { }
        };

        _vm.SentenceHighlightCleared += async () =>
        {
            try
            {
                await OutputWebView.EnsureCoreWebView2Async();
                await OutputWebView.ExecuteScriptAsync("clearHighlight()");
            }
            catch { }
        };
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.V &&
            (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
        {
            if (Clipboard.ContainsText())
            {
                _vm.LoadText(Clipboard.GetText());
                e.Handled = true;
            }
        }
        else if (e.Key == System.Windows.Input.Key.Escape && SettingsPopup.IsOpen)
        {
            SettingsPopup.IsOpen = false;
            e.Handled = true;
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            DragOverlay.Visibility = Visibility.Visible;
        }
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        DragOverlay.Visibility = Visibility.Collapsed;
        if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] files &&
            files.Length > 0)
        {
            _vm.LoadFile(files[0]);
        }
    }

    private void StylePill_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag &&
            Enum.TryParse<SummaryStyle>(tag, out var style))
        {
            _vm.Style = style;
        }
    }

    private void DetailSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _vm.Detail = (DetailLevel)(int)e.NewValue;
    }

    private async void Distill_Click(object sender, RoutedEventArgs e)
    {
        await _vm.DistillAsync();
        if (_vm.State == AppState.Result)
        {
            await LoadHtmlIntoWebView();
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        _vm.CopyToClipboard();
    }

    private void Redistill_Click(object sender, RoutedEventArgs e)
    {
        _vm.BackToLoaded();
    }

    private async void ReadAloud_Click(object sender, RoutedEventArgs e)
    {
        await _vm.ReadAloudAsync();
    }

    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        _vm.TogglePauseTts();
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _vm.StopTts();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _vm.BackToReady();
    }

    private void TonePill_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag &&
            Enum.TryParse<Tone>(tag, out var tone))
        {
            _vm.Tone = tone;
        }
    }

    private void ThemePill_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag)
        {
            switch (tag)
            {
                case "Dark":
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    _ = SyncWebViewTheme("dark");
                    break;
                case "Light":
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    _ = SyncWebViewTheme("light");
                    break;
                default:
                    ApplicationThemeManager.ApplySystemTheme();
                    _ = SyncWebViewTheme("system");
                    break;
            }
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
    }

    private async Task LoadHtmlIntoWebView()
    {
        await OutputWebView.EnsureCoreWebView2Async();

        // S2: Block navigation to external URLs (defense-in-depth)
        OutputWebView.CoreWebView2.NavigationStarting += (s, args) =>
        {
            if (args.Uri is not null &&
                !args.Uri.StartsWith("file://", StringComparison.OrdinalIgnoreCase) &&
                !args.Uri.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
            {
                args.Cancel = true;
            }
        };

        if (OutputWebView.Source is null || OutputWebView.Source.AbsoluteUri == "about:blank")
        {
            var templatePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "output.html");
            OutputWebView.Source = new Uri(templatePath);
            // Wait for navigation to complete
            var tcs = new TaskCompletionSource();
            void onNav(object? s, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs a)
            {
                OutputWebView.NavigationCompleted -= onNav;
                tcs.TrySetResult();
            }
            OutputWebView.NavigationCompleted += onNav;
            await tcs.Task;
        }

        var escaped = System.Text.Json.JsonSerializer.Serialize(_vm.SummaryHtml);
        await OutputWebView.ExecuteScriptAsync($"setContent({escaped})");
    }

    private async Task SyncWebViewTheme(string theme)
    {
        if (OutputWebView.Source is null || OutputWebView.Source.AbsoluteUri == "about:blank")
            return;

        try
        {
            await OutputWebView.EnsureCoreWebView2Async();
            await OutputWebView.ExecuteScriptAsync($"setTheme('{theme}')");
        }
        catch { /* WebView2 not ready yet */ }
    }
}

public class InvertBoolConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
