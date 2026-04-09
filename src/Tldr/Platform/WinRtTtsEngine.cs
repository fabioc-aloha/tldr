using Windows.Media.SpeechSynthesis;
using System.Runtime.InteropServices.WindowsRuntime;
using NAudio.Wave;
using Tldr.Core;

namespace Tldr.Platform;

public sealed class WinRtTtsEngine : ITtsEngine, IDisposable
{
    private readonly SpeechSynthesizer _synth = new();
    private WaveOutEvent? _player;
    private CancellationTokenSource? _cts;
    private readonly ManualResetEventSlim _pauseGate = new(true);

    public event Action<int>? SentenceReached;
    public event Action? PlaybackFinished;

    public async Task SpeakAsync(string text, string voice, float rate, CancellationToken ct)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        if (!string.IsNullOrEmpty(voice))
        {
            var match = SpeechSynthesizer.AllVoices
                .FirstOrDefault(v => v.DisplayName.Contains(voice, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                _synth.Voice = match;
        }

        // Rate: WinRT SpeakingRate is 0.5 to 6.0, default 1.0
        _synth.Options.SpeakingRate = Math.Clamp((double)rate, 0.5, 6.0);

        var sentences = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < sentences.Length; i++)
        {
            if (_cts.Token.IsCancellationRequested)
                break;

            _pauseGate.Wait(_cts.Token);

            if (_cts.Token.IsCancellationRequested)
                break;

            SentenceReached?.Invoke(i);

            using var stream = await _synth.SynthesizeTextToStreamAsync(sentences[i]).AsTask(_cts.Token);
            using var msStream = new System.IO.MemoryStream();
            var inputStream = stream.GetInputStreamAt(0);
            var dataReader = new Windows.Storage.Streams.DataReader(inputStream);
            uint size = (uint)stream.Size;
            await dataReader.LoadAsync(size).AsTask(_cts.Token);
            var buffer = new byte[size];
            dataReader.ReadBytes(buffer);
            msStream.Write(buffer, 0, buffer.Length);
            msStream.Position = 0;

            using var reader = new StreamMediaFoundationReader(msStream);
            using var player = new WaveOutEvent();
            _player = player;

            var tcs = new TaskCompletionSource();
            player.PlaybackStopped += (_, _) => tcs.TrySetResult();
            player.Init(reader);
            player.Play();

            using var reg = _cts.Token.Register(() =>
            {
                player.Stop();
                tcs.TrySetResult();
            });

            await tcs.Task;
            _player = null;
        }

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

    public void Dispose()
    {
        Stop();
        _synth.Dispose();
        _cts?.Dispose();
        _pauseGate.Dispose();
    }
}
