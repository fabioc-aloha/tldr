using System.Speech.Synthesis;
using Tldr.Core;

namespace Tldr.Platform;

public sealed class SapiTtsEngine : ITtsEngine, IDisposable
{
    private readonly SpeechSynthesizer _synth = new();
    private CancellationTokenSource? _cts;
    private bool _isPaused;

    public event Action<int>? SentenceReached;
    public event Action? PlaybackFinished;

    /// <summary>
    /// Speaks sentences sequentially, raising SentenceReached(n) before each.
    /// Pass sentences as newline-delimited text (one sentence per line).
    /// </summary>
    public async Task SpeakAsync(string text, string voice, float rate, CancellationToken ct)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        if (!string.IsNullOrEmpty(voice))
        {
            try { _synth.SelectVoice(voice); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SAPI] Voice '{voice}' not found, using default: {ex.Message}");
            }
        }

        // Rate: SAPI range is -10 to 10, map our 0.5-2.0 range
        _synth.Rate = Math.Clamp((int)((rate - 1f) * 10f), -10, 10);

        var sentences = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < sentences.Length; i++)
        {
            if (_cts.Token.IsCancellationRequested)
                break;

            SentenceReached?.Invoke(i);

            var prompt = new Prompt(sentences[i]);
            _synth.SpeakAsync(prompt);

            // Wait for this sentence to finish
            var tcs = new TaskCompletionSource();
            void handler(object? s, SpeakCompletedEventArgs e) => tcs.TrySetResult();
            _synth.SpeakCompleted += handler;

            try
            {
                using var reg = _cts.Token.Register(() =>
                {
                    _synth.SpeakAsyncCancelAll();
                    tcs.TrySetResult();
                });
                await tcs.Task;
            }
            finally
            {
                _synth.SpeakCompleted -= handler;
            }

            // NASA Rule 2: Bounded pause loop — 30 min max prevents infinite spin
            const int maxPauseIterations = 18_000; // 100ms * 18,000 = 30 min
            int pauseIter = 0;
            while (_isPaused && !_cts.Token.IsCancellationRequested && pauseIter++ < maxPauseIterations)
                await Task.Delay(100, CancellationToken.None);
            if (pauseIter >= maxPauseIterations)
            {
                System.Diagnostics.Debug.WriteLine("[SAPI] Pause exceeded 30 min, auto-resuming");
                _isPaused = false;
            }
        }

        if (!_cts.Token.IsCancellationRequested)
            PlaybackFinished?.Invoke();
    }

    public void Pause()
    {
        _isPaused = true;
        _synth.Pause();
    }

    public void Resume()
    {
        _isPaused = false;
        _synth.Resume();
    }

    public void Stop()
    {
        _isPaused = false;
        _synth.SpeakAsyncCancelAll();
        _cts?.Cancel();
    }

    public void Dispose()
    {
        Stop();
        _synth.Dispose();
        _cts?.Dispose();
    }
}
