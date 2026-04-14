using System.Speech.Synthesis;
using Tldr.Core;

namespace Tldr.Platform;

public sealed class SapiTtsEngine : ITtsEngine, IDisposable
{
    private readonly SpeechSynthesizer _synth = new();
    private CancellationTokenSource? _cts;
    private readonly ManualResetEventSlim _pauseGate = new(true);

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

            // Wait for pause gate to be signaled (or cancellation)
            while (!_pauseGate.IsSet && !_cts.Token.IsCancellationRequested)
            {
                // Use bounded wait to remain responsive to cancellation
                _pauseGate.Wait(TimeSpan.FromMilliseconds(500));
            }
        }

        if (!_cts.Token.IsCancellationRequested)
            PlaybackFinished?.Invoke();
    }

    public void Pause()
    {
        _pauseGate.Reset();
        _synth.Pause();
    }

    public void Resume()
    {
        _synth.Resume();
        _pauseGate.Set();
    }

    public void Stop()
    {
        _pauseGate.Set(); // Release any paused wait
        _synth.SpeakAsyncCancelAll();
        _cts?.Cancel();
    }

    public void Dispose()
    {
        Stop();
        _pauseGate.Dispose();
        _synth.Dispose();
        _cts?.Dispose();
    }
}
