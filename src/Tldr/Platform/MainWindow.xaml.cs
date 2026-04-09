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

    public MainWindow()
    {
        DataContext = _vm;
        InitializeComponent();

        AllowDrop = true;
        Drop += OnDrop;
        DragOver += OnDragOver;
        KeyDown += OnKeyDown;

        Loaded += async (_, _) => await _vm.InitializeAsync();
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.V &&
            (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
        {
            if (_vm.State == AppState.Ready && Clipboard.ContainsText())
            {
                _vm.LoadText(Clipboard.GetText());
                e.Handled = true;
            }
        }
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
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

    private async void Redistill_Click(object sender, RoutedEventArgs e)
    {
        _vm.BackToLoaded();
    }

    private void ReadAloud_Click(object sender, RoutedEventArgs e)
    {
        _vm.State = AppState.Reading;
        // TTS wiring in Phase 6
    }

    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        // TTS pause in Phase 6
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _vm.State = AppState.Result;
        // TTS stop in Phase 6
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
                    break;
                case "Light":
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    break;
                default:
                    ApplicationThemeManager.ApplySystemTheme();
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

        if (OutputWebView.Source is null || OutputWebView.Source.AbsoluteUri == "about:blank")
        {
            var templatePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "output.html");
            OutputWebView.Source = new Uri(templatePath);
            // Wait for navigation to complete
            var tcs = new TaskCompletionSource();
            OutputWebView.NavigationCompleted += (_, _) => tcs.TrySetResult();
            await tcs.Task;
        }

        var escaped = System.Text.Json.JsonSerializer.Serialize(_vm.SummaryHtml);
        await OutputWebView.ExecuteScriptAsync($"setContent({escaped})");
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
