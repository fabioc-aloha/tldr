using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Tldr.Core;

namespace Tldr.Platform;

public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
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
    private int _sentenceCount;

    private bool _isPaused;
    private Summarizer? _summarizer;
    private PromptBuilder? _promptBuilder;
    private UserSettings _settings = new();
    private ITtsEngine? _tts;
    private CancellationTokenSource? _distillCts;
    private System.Threading.Timer? _saveTimer;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<int>? SentenceHighlightRequested;
    public event Action? SentenceHighlightCleared;

    public AppState State
    {
        get => _state;
        set { _state = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsReady)); OnPropertyChanged(nameof(IsLoaded)); OnPropertyChanged(nameof(IsResult)); OnPropertyChanged(nameof(IsReading)); OnPropertyChanged(nameof(IsWebViewVisible)); }
    }

    public bool IsReady => State == AppState.Ready;
    public bool IsLoaded => State == AppState.Loaded;
    public bool IsResult => State == AppState.Result;
    public bool IsReading => State == AppState.Reading;
    public bool IsWebViewVisible => State == AppState.Result || State == AppState.Reading;

    public bool IsPaused
    {
        get => _isPaused;
        private set { _isPaused = value; OnPropertyChanged(); }
    }

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
        set { _style = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsStyleBullets)); OnPropertyChanged(nameof(IsStyleList)); OnPropertyChanged(nameof(IsStyleTable)); OnPropertyChanged(nameof(IsStyleProse)); OnPropertyChanged(nameof(IsStyleSame)); _settings.Style = value.ToString(); ScheduleSave(); }
    }

    public DetailLevel Detail
    {
        get => _detail;
        set { _detail = value; OnPropertyChanged(); _settings.Detail = value.ToString(); ScheduleSave(); }
    }

    public Tone Tone
    {
        get => _tone;
        set { _tone = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsToneNeutral)); OnPropertyChanged(nameof(IsToneFormal)); OnPropertyChanged(nameof(IsToneCasual)); _settings.Tone = value.ToString(); ScheduleSave(); }
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

    public int SentenceCount
    {
        get => _sentenceCount;
        set { _sentenceCount = value; OnPropertyChanged(); }
    }

    // Computed properties for radio button bindings
    public bool IsStyleBullets => Style == SummaryStyle.Bullets;
    public bool IsStyleList => Style == SummaryStyle.List;
    public bool IsStyleTable => Style == SummaryStyle.Table;
    public bool IsStyleProse => Style == SummaryStyle.Prose;
    public bool IsStyleSame => Style == SummaryStyle.Same;

    public bool IsToneNeutral => Tone == Tone.Neutral;
    public bool IsToneFormal => Tone == Tone.Formal;
    public bool IsToneCasual => Tone == Tone.Casual;

    public async Task InitializeAsync()
    {
        // Load user settings (last-used style/detail/tone)
        _settings = UserSettings.Load();
        if (Enum.TryParse<SummaryStyle>(_settings.Style, out var style)) _style = style;
        if (Enum.TryParse<DetailLevel>(_settings.Detail, out var detail)) _detail = detail;
        if (Enum.TryParse<Tone>(_settings.Tone, out var tone)) _tone = tone;
        OnPropertyChanged(nameof(Style));
        OnPropertyChanged(nameof(Detail));
        OnPropertyChanged(nameof(Tone));
        OnPropertyChanged(nameof(IsStyleBullets));
        OnPropertyChanged(nameof(IsStyleList));
        OnPropertyChanged(nameof(IsStyleTable));
        OnPropertyChanged(nameof(IsStyleProse));
        OnPropertyChanged(nameof(IsStyleSame));
        OnPropertyChanged(nameof(IsToneNeutral));
        OnPropertyChanged(nameof(IsToneFormal));
        OnPropertyChanged(nameof(IsToneCasual));

        // Load app config
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var config = TldrConfig.Load(configPath);

        _promptBuilder = new PromptBuilder(Path.Combine(AppContext.BaseDirectory, "prompts.json"));
        _summarizer = new Summarizer(config.Llm.Model, config.Llm.MaxOutputTokens);

        StatusText = "Loading model...";
        IsBusy = true;

        try
        {
            await _summarizer.InitializeAsync(
                onProgress: p => Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadProgress = p;
                    StatusText = p < 100f ? $"Downloading model: {p:F0}%" : "Loading model...";
                }));

            StatusText = "Paste or drop a file. I'll distill it.";
        }
        catch (Exception ex)
        {
            StatusText = "Model failed to load. Ensure Foundry Local is installed.";
            System.Diagnostics.Debug.WriteLine($"[Init] {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private const int MaxInputChars = 2_000_000; // ~500K tokens, well within 10MB extracted text

    public void LoadText(string text, string fileName = "")
    {
        if (text.Length > MaxInputChars)
        {
            StatusText = $"Input too large ({text.Length:N0} chars). Maximum is {MaxInputChars:N0}.";
            return;
        }

        // Stop any in-progress TTS or distill when re-entering
        if (State == AppState.Reading)
            StopTts();
        _distillCts?.Cancel();

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

        try
        {
            var text = FileExtractor.Extract(filePath);
            LoadText(text, Path.GetFileName(filePath));
        }
        catch (InvalidOperationException ex)
        {
            StatusText = ex.Message; // file-too-large message
        }
        catch (Exception)
        {
            StatusText = $"Failed to read file: {Path.GetFileName(filePath)}";
        }
    }

    public async Task DistillAsync()
    {
        if (_summarizer is null || _promptBuilder is null)
            return;

        IsBusy = true;
        StatusText = "Distilling...";

        _distillCts?.Cancel();
        _distillCts = new CancellationTokenSource();
        var ct = _distillCts.Token;

        try
        {
            var systemPrompt = _promptBuilder.Build(Style, Detail, Tone);
            var summary = await Task.Run(() => _summarizer.SummarizeAsync(InputText, systemPrompt, ct), ct);

            SummaryMarkdown = summary;

            // R3: Grounding guard - flag if output is suspiciously longer than input
            var inputLen = InputText.Length;
            var outputLen = summary.Length;
            if (outputLen > inputLen)
            {
                SummaryMarkdown = $"> **Note:** This summary is longer than the original text. The model may have added content not in the source document. Review carefully.\n\n{summary}";
            }

            var html = MarkdownRenderer.ToHtml(SummaryMarkdown);
            SummaryHtml = MarkdownRenderer.AddSentenceMarkers(html);
            SentenceCount = MarkdownRenderer.CountSentences(SummaryHtml);
            State = AppState.Result;
            StatusText = "Done.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Cancelled.";
        }
        catch (ArgumentException ex)
        {
            StatusText = ex.Message;
        }
        catch (Exception ex)
        {
            // Show generic message; avoid leaking document content from inner exceptions
            StatusText = ex is InvalidOperationException
                ? $"Error: {ex.Message}"
                : "An unexpected error occurred. Please try again.";
            System.Diagnostics.Debug.WriteLine($"[Distill] {ex}");
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
        // Release potentially large text buffers
        _inputText = string.Empty;
        _inputFileName = string.Empty;
        _summaryMarkdown = string.Empty;
        _summaryHtml = string.Empty;
        OnPropertyChanged(nameof(InputText));
        OnPropertyChanged(nameof(InputFileName));
        OnPropertyChanged(nameof(SummaryMarkdown));
        OnPropertyChanged(nameof(SummaryHtml));
        State = AppState.Ready;
        StatusText = "Paste or drop a file. I'll distill it.";
    }

    public async Task ReadAloudAsync()
    {
        if (string.IsNullOrEmpty(SummaryMarkdown))
            return;

        (_tts as IDisposable)?.Dispose();
        _tts = CreateTtsEngine();
        _tts.SentenceReached += n =>
            Application.Current.Dispatcher.Invoke(() => SentenceHighlightRequested?.Invoke(n));
        _tts.PlaybackFinished += () =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                SentenceHighlightCleared?.Invoke();
                State = AppState.Result;
                StatusText = "Done.";
            });

        State = AppState.Reading;
        IsPaused = false;
        StatusText = "Reading aloud...";

        // Strip markdown syntax line-by-line for clean TTS
        var lines = SummaryMarkdown.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            if (line.StartsWith('#'))
                sb.AppendLine(line.TrimStart('#').TrimStart());
            else if (line.StartsWith("- ") || line.StartsWith("* "))
                sb.AppendLine(line[2..]);
            else if (line.Contains("---"))
                continue;
            else if (line.StartsWith('|') && line.EndsWith('|'))
                sb.AppendLine(line.Trim('|').Replace('|', ',').Trim());
            else
                sb.AppendLine(line);
        }

        await _tts.SpeakAsync(sb.ToString(), string.Empty, 1f, CancellationToken.None);
    }

    public void TogglePauseTts()
    {
        if (_isPaused)
        {
            _tts?.Resume();
            IsPaused = false;
            StatusText = "Reading aloud...";
        }
        else
        {
            _tts?.Pause();
            IsPaused = true;
            StatusText = "Paused.";
        }
    }

    public void ResumeTts()
    {
        _tts?.Resume();
        StatusText = "Reading aloud...";
    }

    public void StopTts()
    {
        _tts?.Stop();
        IsPaused = false;
        SentenceHighlightCleared?.Invoke();
        State = AppState.Result;
        StatusText = "Done.";
    }

    private void ScheduleSave()
    {
        _saveTimer?.Dispose();
        _saveTimer = new System.Threading.Timer(_ => _settings.Save(), null, 500, Timeout.Infinite);
    }

    private static ITtsEngine CreateTtsEngine()
    {
        try
        {
            return new WinRtTtsEngine();
        }
        catch
        {
            return new SapiTtsEngine();
        }
    }

    public void Dispose()
    {
        _saveTimer?.Dispose();
        (_tts as IDisposable)?.Dispose();
        _distillCts?.Dispose();
    }

    private static string FormatHtmlClipboard(string htmlFragment)
    {
        const string header = "Version:0.9\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\n";
        const string startFragment = "<!--StartFragment-->";
        const string endFragment = "<!--EndFragment-->";
        var htmlStart = "<html><body>" + startFragment;
        var htmlEnd = endFragment + "</body></html>";

        var enc = Encoding.UTF8;
        var headerLength = enc.GetByteCount(string.Format(header, 0, 0, 0, 0));
        var startHtml = headerLength;
        var startFrag = startHtml + enc.GetByteCount(htmlStart);
        var endFrag = startFrag + enc.GetByteCount(htmlFragment);
        var endHtml = endFrag + enc.GetByteCount(htmlEnd);

        return string.Format(header, startHtml, endHtml, startFrag, endFrag) + htmlStart + htmlFragment + htmlEnd;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
