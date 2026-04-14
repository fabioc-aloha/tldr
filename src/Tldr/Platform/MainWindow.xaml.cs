using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Web.WebView2.Core;
using Tldr.Core;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Tldr.Platform;

public partial class MainWindow : FluentWindow
{
    private static readonly Regex ScriptTagRegex = new(@"<script\b[^<]*(?:(?!</script>)<[^<]*)*</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex EventHandlerAttributeRegex = new(@"\son\w+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex JavaScriptHrefRegex = new(@"href\s*=\s*(?<quote>['""])\s*javascript:[^'""]*(\k<quote>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RemoteSourceRegex = new(@"\s(?:src|href)\s*=\s*(?<quote>['""])\s*https?://[^'""]*(\k<quote>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly MainViewModel _vm = new();
    private TrayIconManager? _tray;
    private HotkeyManager? _hotkeys;
    private bool _forceClose;
    private string _activeTheme = "System";
    private bool _webViewInitialized;
    private string? _outputTemplateHtml;

    public MainWindow()
    {
        DataContext = _vm;
        InitializeComponent();

        AllowDrop = true;
        Drop += OnDrop;
        DragOver += OnDragOver;
        DragLeave += OnDragLeave;
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

            // Restore saved theme
            _activeTheme = _vm.Theme;
            switch (_activeTheme)
            {
                case "Dark":
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    break;
                case "Light":
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    break;
                default:
                    ApplicationThemeManager.ApplySystemTheme();
                    break;
            }
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebView] Highlight failed: {ex.Message}");
            }
        };

        _vm.SentenceHighlightCleared += async () =>
        {
            try
            {
                await OutputWebView.EnsureCoreWebView2Async();
                await OutputWebView.ExecuteScriptAsync("clearHighlight()");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebView] Clear highlight failed: {ex.Message}");
            }
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
            SetDragOverlayVisible(true);
        }
        else
        {
            e.Effects = DragDropEffects.None;
            SetDragOverlayVisible(false);
        }
        e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        SetDragOverlayVisible(false);
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        SetDragOverlayVisible(false);
        if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] files &&
            files.Length > 0)
        {
            var filePath = files[0];

            // Block UNC and remote paths to prevent NTLM credential leaks
            if (filePath.StartsWith(@"\\", StringComparison.Ordinal) ||
                !System.IO.Path.IsPathRooted(filePath) ||
                (filePath.Length >= 2 && filePath[1] != ':'))
            {
                _vm.StatusText = "Only local files are supported. Remote/network paths are not allowed.";
                return;
            }

            _vm.LoadFile(filePath);
        }
    }

    private void SetDragOverlayVisible(bool visible)
    {
        if (visible)
        {
            if (DragOverlay.Visibility != Visibility.Visible)
                DragOverlay.Visibility = Visibility.Visible;

            AnimateDragOverlay(1.0, 1.0, collapseWhenDone: false);
            return;
        }

        if (DragOverlay.Visibility == Visibility.Collapsed)
            return;

        AnimateDragOverlay(0.0, 0.96, collapseWhenDone: true);
    }

    private void AnimateDragOverlay(double targetOpacity, double targetScale, bool collapseWhenDone)
    {
        var duration = TimeSpan.FromMilliseconds(160);
        var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        var opacityAnimation = new DoubleAnimation(targetOpacity, duration) { EasingFunction = easing };
        if (collapseWhenDone)
        {
            opacityAnimation.Completed += (_, _) =>
            {
                if (targetOpacity <= 0)
                    DragOverlay.Visibility = Visibility.Collapsed;
            };
        }

        DragOverlay.BeginAnimation(OpacityProperty, opacityAnimation, HandoffBehavior.SnapshotAndReplace);

        if (DragOverlayPanel.RenderTransform is ScaleTransform scaleTransform)
        {
            var scaleAnimation = new DoubleAnimation(targetScale, duration) { EasingFunction = easing };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation, HandoffBehavior.SnapshotAndReplace);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation, HandoffBehavior.SnapshotAndReplace);
        }
    }

    private void StyleChip_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is string tag &&
            Enum.TryParse<SummaryStyle>(tag, out var style))
        {
            _vm.Style = style;
        }
    }

    private void StyleChip_Unchecked(object sender, RoutedEventArgs e)
    {
        // Only prevent uncheck if this chip is still the active style
        if (sender is ToggleButton tb && tb.Tag is string tag &&
            Enum.TryParse<SummaryStyle>(tag, out var style) && _vm.Style == style)
            tb.IsChecked = true;
    }

    private void DetailChip_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is string tag && int.TryParse(tag, out var level))
        {
            _vm.Detail = (DetailLevel)level;
        }
    }

    private void DetailChip_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is string tag &&
            int.TryParse(tag, out var level) && _vm.Detail == (DetailLevel)level)
            tb.IsChecked = true;
    }

    private async void Distill_Click(object sender, RoutedEventArgs e)
    {
        await _vm.DistillAsync();
        if (_vm.State == AppState.Result)
        {
            try
            {
                await LoadHtmlIntoWebView();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebView] LoadHtml failed: {ex.Message}");
                _vm.StatusText = $"Render failed: {ex.Message}";
            }
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

    private void ToneChip_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is string tag &&
            Enum.TryParse<Tone>(tag, out var tone))
        {
            _vm.Tone = tone;
        }
    }

    private void ToneChip_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is string tag &&
            Enum.TryParse<Tone>(tag, out var tone) && _vm.Tone == tone)
            tb.IsChecked = true;
    }

    private void ThemeChip_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is string tag)
        {
            _activeTheme = tag;
            _vm.Theme = tag;

            // Uncheck sibling theme chips
            if (tb.Parent is System.Windows.Controls.WrapPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is ToggleButton sibling && sibling != tb)
                        sibling.IsChecked = false;
                }
            }

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

    private void ThemeChip_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb && tb.Tag is string tag && tag == _activeTheme)
            tb.IsChecked = true;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
    }

    private async Task LoadHtmlIntoWebView()
    {
        await OutputWebView.EnsureCoreWebView2Async();

        // S2: Block navigation to external URLs (defense-in-depth) — register once
        if (!_webViewInitialized)
        {
            _webViewInitialized = true;
            OutputWebView.CoreWebView2.NavigationStarting += (s, args) =>
            {
                // Only block user-initiated navigations (link clicks).
                // Programmatic NavigateToString calls must pass through.
                if (args.IsUserInitiated)
                {
                    args.Cancel = true;
                }
            };
        }

        var themeMode = _activeTheme switch
        {
            "Dark" => "dark",
            "Light" => "light",
            _ => ApplicationThemeManager.GetSystemTheme() == SystemTheme.Dark ? "dark" : "light"
        };

        var document = BuildRenderedHtmlDocument(themeMode, _vm.SummaryHtml);
        await NavigateToRenderedDocumentAsync(document);
    }

    private async Task SyncWebViewTheme(string theme)
    {
        if (OutputWebView.CoreWebView2 is null)
            return;

        try
        {
            await OutputWebView.ExecuteScriptAsync($"setTheme({JsonSerializer.Serialize(theme)})");
        }
        catch { /* WebView2 not ready yet */ }
    }

    private async Task NavigateToRenderedDocumentAsync(string html)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            OutputWebView.NavigationCompleted -= OnNavigationCompleted;
            if (args.IsSuccess)
                tcs.TrySetResult();
            else
                tcs.TrySetException(new InvalidOperationException($"WebView navigation failed: {args.WebErrorStatus}"));
        }

        OutputWebView.NavigationCompleted += OnNavigationCompleted;
        OutputWebView.NavigateToString(html);
        await tcs.Task;
    }

    private string BuildRenderedHtmlDocument(string themeMode, string summaryHtml)
    {
        _outputTemplateHtml ??= System.IO.File.ReadAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "output.html"));

        var document = _outputTemplateHtml;
        var sanitizedHtml = SanitizeHtmlFragment(summaryHtml);

        document = document.Replace("<html lang=\"en\">", $"<html lang=\"en\" data-theme=\"{themeMode}\">", StringComparison.Ordinal);
        document = document.Replace("<div id=\"content\" role=\"article\"></div>", $"<div id=\"content\" role=\"article\">{sanitizedHtml}</div>", StringComparison.Ordinal);
        document = document.Replace("<div id=\"ai-disclosure\" class=\"ai-disclosure\" style=\"display:none;\">", "<div id=\"ai-disclosure\" class=\"ai-disclosure\" style=\"display:block;\">", StringComparison.Ordinal);

        return document;
    }

    private static string SanitizeHtmlFragment(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var sanitized = ScriptTagRegex.Replace(html, string.Empty);
        sanitized = EventHandlerAttributeRegex.Replace(sanitized, string.Empty);
        sanitized = JavaScriptHrefRegex.Replace(sanitized, "href=\"#\"");
        sanitized = RemoteSourceRegex.Replace(sanitized, string.Empty);
        return sanitized;
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

public class InvertBoolToVisConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
