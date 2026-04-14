using System.IO;
using Edge_tts_sharp;
using Edge_tts_sharp.Model;
using NAudio.Wave;
using Tldr.Core;

namespace Tldr.Platform;

/// <summary>
/// Neural TTS engine using Microsoft Edge's free cloud TTS service.
/// Produces high-quality natural speech via WebSocket (no API key required).
/// Falls back gracefully if network is unavailable.
/// </summary>
public sealed class EdgeTtsEngine : ITtsEngine, IDisposable
{
    private WaveOutEvent? _player;
    private MemoryStream? _audioStream;
    private Mp3FileReader? _mp3Reader;
    private CancellationTokenSource? _cts;
    private readonly ManualResetEventSlim _pauseGate = new(true);
    private TaskCompletionSource? _playbackTcs;

    // Default to Aria (female, neural, natural-sounding)
    private const string DefaultVoiceName = "AriaNeural";

    public event Action<int>? SentenceReached;
    public event Action? PlaybackFinished;

    public async Task SpeakAsync(string text, string voice, float rate, CancellationToken ct)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Pick voice: prefer explicit, otherwise Aria
        var voiceSearch = string.IsNullOrEmpty(voice) ? DefaultVoiceName : voice;

        // Run blocking network calls off the UI thread
        byte[]? audioBytes = null;
        try
        {
            audioBytes = await Task.Run(() =>
            {
                List<eVoice> voices;
                try
                {
                    voices = Edge_tts.GetVoice();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EdgeTts] GetVoice failed: {ex.Message}");
                    return null;
                }

                if (voices.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[EdgeTts] No voices returned");
                    return null;
                }

                var chosen = voices.FirstOrDefault(v =>
                        v.ShortName != null &&
                        v.ShortName.Contains(voiceSearch, StringComparison.OrdinalIgnoreCase) &&
                        v.Locale != null &&
                        v.Locale.StartsWith("en-", StringComparison.OrdinalIgnoreCase))
                    ?? voices.FirstOrDefault(v =>
                        v.ShortName != null &&
                        v.ShortName.Contains("AriaNeural", StringComparison.OrdinalIgnoreCase))
                    ?? voices.FirstOrDefault(v =>
                        v.Locale != null &&
                        v.Locale.StartsWith("en-", StringComparison.OrdinalIgnoreCase));

                if (chosen == null)
                {
                    System.Diagnostics.Debug.WriteLine("[EdgeTts] No suitable voice found");
                    return null;
                }

                // Rate: map float (0.5-2.0) to Edge TTS int (-50 to +100)
                var edgeRate = (int)((rate - 1.0f) * 100);
                edgeRate = Math.Clamp(edgeRate, -100, 100);

                var option = new PlayOption
                {
                    Text = text,
                    Rate = edgeRate,
                    Volume = 100f,
                };

                byte[]? result = null;
                try
                {
                    Edge_tts.Await = true;
                    Edge_tts.Invoke(option, chosen, data =>
                    {
                        result = data.ToArray();
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EdgeTts] Invoke failed: {ex.Message}");
                    return null;
                }

                return result;
            }, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (audioBytes == null || audioBytes.Length == 0 || _cts.Token.IsCancellationRequested)
        {
            if (!_cts.Token.IsCancellationRequested)
                PlaybackFinished?.Invoke();
            return;
        }

        SentenceReached?.Invoke(0);

        // Play the MP3 audio via NAudio
        _audioStream = new MemoryStream(audioBytes);
        _mp3Reader = new Mp3FileReader(_audioStream);
        var player = new WaveOutEvent();
        _player = player;

        _playbackTcs = new TaskCompletionSource();
        player.PlaybackStopped += (_, _) => _playbackTcs.TrySetResult();
        player.Init(_mp3Reader);
        player.Play();

        using var reg = _cts.Token.Register(() =>
        {
            player.Stop();
            _playbackTcs.TrySetResult();
        });

        await _playbackTcs.Task;

        CleanupPlayback();

        if (!_cts.Token.IsCancellationRequested)
            PlaybackFinished?.Invoke();
    }

    public void Pause()
    {
        _pauseGate.Reset();
        _player?.Pause();
    }

    public void Resume()
    {
        _player?.Play();
        _pauseGate.Set();
    }

    public void Stop()
    {
        _pauseGate.Set();
        _player?.Stop();
        _cts?.Cancel();
    }

    private void CleanupPlayback()
    {
        _player?.Dispose();
        _player = null;
        _mp3Reader?.Dispose();
        _mp3Reader = null;
        _audioStream?.Dispose();
        _audioStream = null;
    }

    public void Dispose()
    {
        Stop();
        CleanupPlayback();
        _cts?.Dispose();
        _pauseGate.Dispose();
    }
}
