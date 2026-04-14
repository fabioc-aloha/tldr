using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;

namespace Tldr.Core;

public sealed class Summarizer : IAsyncDisposable
{
    private readonly string _modelAlias;
    private readonly int _maxOutputTokens;
    private OpenAIChatClient? _chatClient;
    private IModel? _model;

    /// <summary>Context window size in tokens. Defaults to 128k (Phi-4 Mini). Updated at load time if metadata is available.</summary>
    public int ContextWindowTokens { get; private set; } = 128_000;

    public int MaxOutputTokens => _maxOutputTokens;

    public Summarizer(string modelAlias = "phi-4-mini", int maxOutputTokens = 1024)
    {
        _modelAlias = modelAlias;
        _maxOutputTokens = maxOutputTokens;
    }

    public async Task InitializeAsync(Action<float>? onProgress = null, ILogger? logger = null, CancellationToken ct = default)
    {
        var config = new Configuration
        {
            AppName = "tldr",
            LogLevel = Microsoft.AI.Foundry.Local.LogLevel.Warning
        };

        ct.ThrowIfCancellationRequested();
        await FoundryLocalManager.CreateAsync(config, logger ?? NullLogger.Instance);
        var mgr = FoundryLocalManager.Instance;

        // Register hardware execution providers (NPU/GPU) for acceleration.
        // Skips download if already cached; built-in CPU fallback if this fails.
        try
        {
            ct.ThrowIfCancellationRequested();
            await mgr.DownloadAndRegisterEpsAsync();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            // Non-fatal: built-in execution providers still work.
            System.Diagnostics.Debug.WriteLine($"[Summarizer] EP registration failed (CPU fallback): {ex.Message}");
        }

        var catalog = await mgr.GetCatalogAsync();
        _model = await catalog.GetModelAsync(_modelAlias)
            ?? throw new InvalidOperationException($"Model '{_modelAlias}' not found in catalog.");

        ct.ThrowIfCancellationRequested();
        await _model.DownloadAsync(new Action<float>(progress =>
        {
            onProgress?.Invoke(progress);
        }));

        ct.ThrowIfCancellationRequested();
        await _model.LoadAsync();
        _chatClient = await _model.GetChatClientAsync();
    }

    public async Task<string> SummarizeAsync(string text, string systemPrompt, CancellationToken ct = default)
    {
        // NASA Rule 5: Assertions at system boundary
        if (_chatClient is null)
            throw new InvalidOperationException("Call InitializeAsync before summarizing.");
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Input text must not be empty.", nameof(text));
        if (string.IsNullOrWhiteSpace(systemPrompt))
            throw new ArgumentException("System prompt must not be empty.", nameof(systemPrompt));

        var inputTokens = EstimateTokens(text);
        var promptTokens = EstimateTokens(systemPrompt);
        var totalInput = inputTokens + promptTokens;
        var available = GetAvailableInputTokens(systemPrompt);

        if (totalInput > available)
            throw new ArgumentException(
                $"Input too long: ~{totalInput:N0} tokens (limit ~{available:N0}). " +
                "Shorten the text or use a lower detail level. Chunked summarization is planned for a future version.");

        _chatClient.Settings.Temperature = 0f;
        _chatClient.Settings.MaxTokens = _maxOutputTokens;

        var messages = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = systemPrompt },
            new ChatMessage { Role = "user", Content = $"<document>\n{text}\n</document>" }
        };

        var response = await _chatClient.CompleteChatAsync(messages, ct);

        // NASA Rule 5: Validate model output at system boundary
        string? content = (string?)response.Choices[0].Message.Content;
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Model returned an empty response. Try again or use a different detail level.");

        return content;
    }

    public int GetAvailableInputTokens(string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
            throw new ArgumentException("System prompt must not be empty.", nameof(systemPrompt));

        var promptTokens = EstimateTokens(systemPrompt);
        return Math.Max(1, ContextWindowTokens - _maxOutputTokens - promptTokens);
    }

    public static int EstimateTokens(string text) => text.Length / 4;

    public async ValueTask DisposeAsync()
    {
        if (_model is not null)
        {
            try { await _model.UnloadAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Summarizer] Model unload failed: {ex.Message}");
            }
            _model = null;
        }
        _chatClient = null;
    }
}
