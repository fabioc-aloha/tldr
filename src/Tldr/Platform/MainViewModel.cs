using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
    private bool _isModelLoading;
    private Summarizer? _summarizer;
    private PromptBuilder? _promptBuilder;
    private UserSettings _settings = new();
    private ITtsEngine? _tts;
    private CancellationTokenSource? _distillCts;
    private CancellationTokenSource? _ttsCts;
    private System.Threading.Timer? _saveTimer;
    private Task? _modelLoadTask;
    private string _selectedVoice = string.Empty;
    private string _ttsEngineName = "Edge (Neural)";
    private string _ttsEnginePreference = "edge";
    private string _machineInfo = string.Empty;

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
        set { _inputText = value; WordCount = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length; OnPropertyChanged(); OnPropertyChanged(nameof(InputPreview)); }
    }

    public string InputPreview => StripMarkdown(_inputText);

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
        set { _style = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsStyleBullets)); OnPropertyChanged(nameof(IsStyleList)); OnPropertyChanged(nameof(IsStyleTable)); OnPropertyChanged(nameof(IsStyleProse)); OnPropertyChanged(nameof(IsStyleSimple)); OnPropertyChanged(nameof(IsStyleSame)); _settings.Style = value.ToString(); ScheduleSave(); }
    }

    public DetailLevel Detail
    {
        get => _detail;
        set { _detail = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDetailBrief)); OnPropertyChanged(nameof(IsDetailStandard)); OnPropertyChanged(nameof(IsDetailDetailed)); _settings.Detail = value.ToString(); ScheduleSave(); }
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

    public bool IsModelLoading
    {
        get => _isModelLoading;
        private set { _isModelLoading = value; OnPropertyChanged(); }
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

    // Computed properties for chip toggle bindings
    public bool IsStyleBullets => Style == SummaryStyle.Bullets;
    public bool IsStyleList => Style == SummaryStyle.List;
    public bool IsStyleTable => Style == SummaryStyle.Table;
    public bool IsStyleProse => Style == SummaryStyle.Prose;
    public bool IsStyleSimple => Style == SummaryStyle.Simple;
    public bool IsStyleSame => Style == SummaryStyle.Same;

    public bool IsDetailBrief => Detail == DetailLevel.Brief;
    public bool IsDetailStandard => Detail == DetailLevel.Standard;
    public bool IsDetailDetailed => Detail == DetailLevel.Detailed;

    public bool IsToneNeutral => Tone == Tone.Neutral;
    public bool IsToneFormal => Tone == Tone.Formal;
    public bool IsToneCasual => Tone == Tone.Casual;

    public string SelectedVoice
    {
        get => _selectedVoice;
        set { _selectedVoice = value; OnPropertyChanged(); _settings.Voice = value; ScheduleSave(); }
    }

    public string TtsEngineName
    {
        get => _ttsEngineName;
        private set { _ttsEngineName = value; OnPropertyChanged(); }
    }

    public string MachineInfo
    {
        get => _machineInfo;
        private set { _machineInfo = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> AvailableVoices { get; } = new();

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
        OnPropertyChanged(nameof(IsDetailBrief));
        OnPropertyChanged(nameof(IsDetailStandard));
        OnPropertyChanged(nameof(IsDetailDetailed));
        OnPropertyChanged(nameof(IsToneNeutral));
        OnPropertyChanged(nameof(IsToneFormal));
        OnPropertyChanged(nameof(IsToneCasual));

        // Load app config
        var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var config = TldrConfig.Load(configPath);

        _promptBuilder = new PromptBuilder(Path.Combine(AppContext.BaseDirectory, "prompts.json"));
        _summarizer = new Summarizer(config.Llm.Model, config.Llm.MaxOutputTokens);
        _ttsEnginePreference = config.Tts.Engine?.ToLowerInvariant() ?? "edge";

        // Start model loading in background — user can paste/drop immediately
        IsModelLoading = true;
        StatusText = "Loading model in background...";
        _modelLoadTask = Task.Run(() => LoadModelAsync());

        // Load voice from saved settings
        if (!string.IsNullOrEmpty(_settings.Voice))
        {
            _selectedVoice = _settings.Voice;
            OnPropertyChanged(nameof(SelectedVoice));
        }

        // Populate machine info
        PopulateMachineInfo(config.Llm.Model);

        // Populate available voices (background, non-blocking)
        _ = Task.Run(() =>
        {
            try
            {
                var voices = Edge_tts_sharp.Edge_tts.GetVoice();
                var enVoices = voices
                    .Where(v => v.Locale != null && v.Locale.StartsWith("en-", StringComparison.OrdinalIgnoreCase) && v.ShortName != null)
                    .Select(v => v.ShortName!)
                    .OrderBy(n => n)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableVoices.Clear();
                    foreach (var v in enVoices) AvailableVoices.Add(v);
                    if (string.IsNullOrEmpty(SelectedVoice) && AvailableVoices.Count > 0)
                    {
                        var aria = AvailableVoices.FirstOrDefault(v => v.Contains("AriaNeural", StringComparison.OrdinalIgnoreCase));
                        SelectedVoice = aria ?? AvailableVoices[0];
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Voices] Failed to enumerate: {ex.Message}");
            }
        });
    }

    private async Task LoadModelAsync()
    {
        try
        {
            await _summarizer!.InitializeAsync(
                onProgress: p => Application.Current.Dispatcher.Invoke(() =>
                {
                    DownloadProgress = p;
                    StatusText = p < 100f ? $"Downloading model: {p:F0}%" : "Loading model...";
                }));

            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText = State == AppState.Ready
                    ? "Paste or drop a file. I'll distill it."
                    : "Model ready.";
            });
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() => StatusText = ClassifyError(ex, "Init"));
            LogError("Init", ex);
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() => IsModelLoading = false);
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadFile] {ex.GetType().Name}: {ex.Message}");
            StatusText = $"Failed to read file: {Path.GetFileName(filePath)}";
        }
    }

    public async Task DistillAsync()
    {
        if (_summarizer is null || _promptBuilder is null)
        {
            StatusText = "Model not loaded. Restart the app.";
            System.Diagnostics.Debug.WriteLine("[Distill] Called before InitializeAsync completed");
            return;
        }

        if (string.IsNullOrWhiteSpace(InputText))
        {
            StatusText = "Nothing to distill. Paste some text first.";
            return;
        }

        // Wait for model if still loading in background
        if (_modelLoadTask is { IsCompleted: false })
        {
            IsBusy = true;
            StatusText = "Waiting for model to finish loading...";
            await _modelLoadTask;
        }

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
            StatusText = ClassifyError(ex, "Distill");
            LogError("Distill", ex);
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

        // Cancel any in-progress TTS before starting new one (race condition guard)
        _ttsCts?.Cancel();
        (_tts as IDisposable)?.Dispose();

        _ttsCts = new CancellationTokenSource();
        var ct = _ttsCts.Token;

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

        try
        {
            await _tts.SpeakAsync(sb.ToString(), _selectedVoice, 1f, ct);
        }
        catch (Exception ex)
        {
            LogError("ReadAloud", ex);
            StatusText = $"Read aloud failed: {ex.Message}";
            State = AppState.Result;
        }
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
        _ttsCts?.Cancel();
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

    private void PopulateMachineInfo(string modelAlias)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Model: {modelAlias}");
        sb.AppendLine($"Context: {_summarizer?.ContextWindowTokens ?? 128_000:N0} tokens");
        sb.AppendLine($"CPU: {Environment.ProcessorCount} cores");
        try
        {
            var mem = GC.GetGCMemoryInfo();
            sb.AppendLine($"Memory: {mem.TotalAvailableMemoryBytes / (1024 * 1024 * 1024.0):F1} GB");
        }
        catch { sb.AppendLine("Memory: unknown"); }
        sb.AppendLine($"TTS: {TtsEngineName}");
        MachineInfo = sb.ToString().TrimEnd();
    }

    private ITtsEngine CreateTtsEngine()
    {
        // Respect config preference: "edge", "winrt", or "sapi"
        if (_ttsEnginePreference == "sapi")
        {
            TtsEngineName = "SAPI5 (Offline)";
            return new SapiTtsEngine();
        }

        if (_ttsEnginePreference == "winrt")
        {
            try
            {
                TtsEngineName = "WinRT (Local)";
                return new WinRtTtsEngine();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TTS] WinRT engine failed, falling back: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // Default: Edge TTS → WinRT → SAPI5 cascade
        try
        {
            TtsEngineName = "Edge (Neural)";
            return new EdgeTtsEngine();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TTS] Edge engine failed, falling back: {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            TtsEngineName = "WinRT (Local)";
            return new WinRtTtsEngine();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TTS] WinRT engine failed, falling back: {ex.GetType().Name}: {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("[TTS] Using SAPI5 fallback engine");
        TtsEngineName = "SAPI5 (Offline)";
        return new SapiTtsEngine();
    }

    public void Dispose()
    {
        _saveTimer?.Dispose();
        _ttsCts?.Cancel();
        (_tts as IDisposable)?.Dispose();
        _ttsCts?.Dispose();
        _distillCts?.Dispose();

        // Summarizer holds the loaded LLM model; fire-and-forget async dispose
        if (_summarizer is not null)
        {
            _ = _summarizer.DisposeAsync().AsTask().ContinueWith(
                t => System.Diagnostics.Debug.WriteLine($"[Dispose] Summarizer: {t.Exception?.InnerException?.Message}"),
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }

    private static string ClassifyError(Exception ex, string phase)
    {
        var inner = ex.InnerException ?? ex;
        var msg = inner.Message;

        // Connection/network errors (Foundry Local not running)
        if (inner is System.Net.Http.HttpRequestException or System.Net.Sockets.SocketException
            || msg.Contains("connection", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("refused", StringComparison.OrdinalIgnoreCase))
            return "Cannot reach Foundry Local. Is it running? Start it and try again.";

        // Model not found or not loaded
        if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("catalog", StringComparison.OrdinalIgnoreCase))
            return $"Model not found. Ensure the model is installed in Foundry Local.";

        // Timeout
        if (inner is TimeoutException || inner is TaskCanceledException { InnerException: TimeoutException })
            return "Request timed out. The model may be overloaded. Try a shorter text.";

        // Dynamic/runtime binding (response structure changed)
        if (inner is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            return $"Unexpected model response format. ({inner.Message})";

        // InvalidOperationException from our code (show directly)
        if (ex is InvalidOperationException)
            return $"Error: {ex.Message}";

        // Fallback: show the exception type + message (never the full stack)
        return $"{phase} error: [{inner.GetType().Name}] {msg}";
    }

    private const long MaxLogFileBytes = 512 * 1024; // 512 KB log rotation threshold

    private static void LogError(string phase, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[{phase}] {ex}");
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TLDR");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, "errors.log");

            // NASA Rule 3: Bounded data — rotate log to prevent unbounded disk growth
            if (File.Exists(logFile) && new FileInfo(logFile).Length > MaxLogFileBytes)
            {
                var archivePath = logFile + ".old";
                File.Copy(logFile, archivePath, overwrite: true);
                File.WriteAllText(logFile, string.Empty);
            }

            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{phase}] {ex}\n";
            File.AppendAllText(logFile, entry);
        }
        catch (Exception logEx)
        {
            // Logging must not throw, but never silently swallow
            System.Diagnostics.Debug.WriteLine($"[LogError] Failed to write log: {logEx.Message}");
        }
    }

    private static readonly Regex MarkdownPattern = new(
        @"\*\*|__|\*|_|~~|`{1,3}|^#{1,6}\s|^>\s?|^[-*+]\s|^\d+\.\s|!?\[([^\]]*)\]\([^)]*\)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static string StripMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        // Replace links with just their display text
        var result = Regex.Replace(text, @"!?\[([^\]]*)\]\([^)]*\)", "$1");
        // Remove remaining markdown syntax
        result = MarkdownPattern.Replace(result, "");
        return result.Trim();
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
