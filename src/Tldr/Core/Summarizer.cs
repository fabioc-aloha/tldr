using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using System.Text;

namespace Tldr.Core;

public sealed class Summarizer : IAsyncDisposable
{
    private readonly string _modelAlias;
    private dynamic? _chatClient;
    private dynamic? _model;

    public Summarizer(string modelAlias = "phi-4-mini")
    {
        _modelAlias = modelAlias;
    }

    public async Task InitializeAsync(Action<float>? onProgress = null, ILogger? logger = null, CancellationToken ct = default)
    {
        var config = new Configuration
        {
            AppName = "tldr",
            LogLevel = Microsoft.AI.Foundry.Local.LogLevel.Warning
        };

        await FoundryLocalManager.CreateAsync(config, logger ?? NullLogger.Instance);
        var mgr = FoundryLocalManager.Instance;

        // Register hardware execution providers (NPU/GPU) for acceleration.
        // Skips download if already cached; built-in CPU fallback if this fails.
        try
        {
            await mgr.DownloadAndRegisterEpsAsync();
        }
        catch
        {
            // Non-fatal: built-in execution providers still work.
        }

        var catalog = await mgr.GetCatalogAsync();
        _model = await catalog.GetModelAsync(_modelAlias)
            ?? throw new InvalidOperationException($"Model '{_modelAlias}' not found in catalog.");

        await _model.DownloadAsync(new Action<float>(progress =>
        {
            onProgress?.Invoke(progress);
        }));

        await _model.LoadAsync();
        _chatClient = await _model.GetChatClientAsync();
    }

    public async Task<string> SummarizeAsync(string text, string systemPrompt, int maxOutputTokens = 1024, CancellationToken ct = default)
    {
        if (_chatClient is null)
            throw new InvalidOperationException("Call InitializeAsync before summarizing.");

        _chatClient.Settings.Temperature = 0f;
        _chatClient.Settings.MaxTokens = maxOutputTokens;

        var messages = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = systemPrompt },
            new ChatMessage { Role = "user", Content = text }
        };

        var response = await _chatClient.CompleteChatAsync(messages, ct);
        return (string)response.Choices[0].Message.Content;
    }

    public static int EstimateTokens(string text) => text.Length / 4;

    public async ValueTask DisposeAsync()
    {
        if (_model is not null)
        {
            try { await _model.UnloadAsync(); } catch { }
            _model = null;
        }
        _chatClient = null;
    }
}
