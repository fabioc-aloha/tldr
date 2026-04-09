using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Tldr.Core;

namespace Tldr.Platform;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private AppState _state = AppState.Ready;
    private string _inputText = string.Empty;
    private string _summaryMarkdown = string.Empty;
    private string _summaryHtml = string.Empty;
    private SummaryStyle _style = SummaryStyle.Bullets;
    private DetailLevel _detail = DetailLevel.Standard;
    private Tone _tone = Tone.Neutral;
    private bool _isBusy;
    private string _statusText = "Paste or drop a file. I'll distill it.";
    private double _downloadProgress;
    private string _inputFileName = string.Empty;
    private int _wordCount;

    private Summarizer? _summarizer;
    private PromptBuilder? _promptBuilder;
    private UserSettings _settings = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public AppState State
    {
        get => _state;
        set { _state = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsReady)); OnPropertyChanged(nameof(IsLoaded)); OnPropertyChanged(nameof(IsResult)); OnPropertyChanged(nameof(IsReading)); }
    }

    public bool IsReady => State == AppState.Ready;
    public bool IsLoaded => State == AppState.Loaded;
    public bool IsResult => State == AppState.Result;
    public bool IsReading => State == AppState.Reading;

    public string InputText
    {
        get => _inputText;
        set { _inputText = value; WordCount = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length; OnPropertyChanged(); }
    }

    public string SummaryMarkdown
    {
        get => _summaryMarkdown;
        set { _summaryMarkdown = value; OnPropertyChanged(); }
    }

    public string SummaryHtml
    {
        get => _summaryHtml;
        set { _summaryHtml = value; OnPropertyChanged(); }
    }

    public SummaryStyle Style
    {
        get => _style;
        set { _style = value; OnPropertyChanged(); _settings.Style = value.ToString(); _settings.Save(); }
    }

    public DetailLevel Detail
    {
        get => _detail;
        set { _detail = value; OnPropertyChanged(); _settings.Detail = value.ToString(); _settings.Save(); }
    }

    public Tone Tone
    {
        get => _tone;
        set { _tone = value; OnPropertyChanged(); _settings.Tone = value.ToString(); _settings.Save(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public double DownloadProgress
    {
        get => _downloadProgress;
        set { _downloadProgress = value; OnPropertyChanged(); }
    }

    public string InputFileName
    {
        get => _inputFileName;
        set { _inputFileName = value; OnPropertyChanged(); }
    }

    public int WordCount
    {
        get => _wordCount;
        set { _wordCount = value; OnPropertyChanged(); }
    }

    public async Task InitializeAsync()
    {
        _settings = UserSettings.Load();
        if (Enum.TryParse<SummaryStyle>(_settings.Style, out var style)) _style = style;
        if (Enum.TryParse<DetailLevel>(_settings.Detail, out var detail)) _detail = detail;
        if (Enum.TryParse<Tone>(_settings.Tone, out var tone)) _tone = tone;
        OnPropertyChanged(nameof(Style));
        OnPropertyChanged(nameof(Detail));
        OnPropertyChanged(nameof(Tone));

        _promptBuilder = new PromptBuilder(Path.Combine(AppContext.BaseDirectory, "prompts.json"));
        _summarizer = new Summarizer();

        StatusText = "Loading model...";
        IsBusy = true;

        await _summarizer.InitializeAsync(
            onProgress: p => Application.Current.Dispatcher.Invoke(() =>
            {
                DownloadProgress = p;
                StatusText = p < 100f ? $"Downloading model: {p:F0}%" : "Loading model...";
            }));

        StatusText = "Paste or drop a file. I'll distill it.";
        IsBusy = false;
    }

    public void LoadText(string text, string fileName = "")
    {
        InputText = text;
        InputFileName = fileName;
        State = AppState.Loaded;
    }

    public void LoadFile(string filePath)
    {
        if (!FileExtractor.IsSupported(filePath))
        {
            StatusText = $"Unsupported file type: {Path.GetExtension(filePath)}";
            return;
        }

        var text = FileExtractor.Extract(filePath);
        LoadText(text, Path.GetFileName(filePath));
    }

    public async Task DistillAsync()
    {
        if (_summarizer is null || _promptBuilder is null)
            return;

        IsBusy = true;
        StatusText = "Distilling...";

        try
        {
            var systemPrompt = _promptBuilder.Build(Style, Detail, Tone);
            var summary = await Task.Run(() => _summarizer.SummarizeAsync(InputText, systemPrompt));

            SummaryMarkdown = summary;
            SummaryHtml = MarkdownRenderer.ToHtml(summary);
            State = AppState.Result;
            StatusText = "Done.";
        }
        catch (ArgumentException ex)
        {
            StatusText = ex.Message;
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void CopyToClipboard()
    {
        var data = new DataObject();
        data.SetData(DataFormats.Html, FormatHtmlClipboard(SummaryHtml));
        data.SetData(DataFormats.UnicodeText, SummaryMarkdown);
        Clipboard.SetDataObject(data, true);
        StatusText = "Copied to clipboard.";
    }

    public void BackToLoaded()
    {
        State = AppState.Loaded;
    }

    public void BackToReady()
    {
        InputText = string.Empty;
        InputFileName = string.Empty;
        SummaryMarkdown = string.Empty;
        SummaryHtml = string.Empty;
        State = AppState.Ready;
        StatusText = "Paste or drop a file. I'll distill it.";
    }

    private static string FormatHtmlClipboard(string htmlFragment)
    {
        const string header = "Version:0.9\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\n";
        const string startFragment = "<!--StartFragment-->";
        const string endFragment = "<!--EndFragment-->";
        var htmlStart = "<html><body>" + startFragment;
        var htmlEnd = endFragment + "</body></html>";

        var headerLength = string.Format(header, 0, 0, 0, 0).Length;
        var startHtml = headerLength;
        var startFrag = startHtml + htmlStart.Length;
        var endFrag = startFrag + htmlFragment.Length;
        var endHtml = endFrag + htmlEnd.Length;

        return string.Format(header, startHtml, endHtml, startFrag, endFrag) + htmlStart + htmlFragment + htmlEnd;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
