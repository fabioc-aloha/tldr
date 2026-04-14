using System.IO;
using System.Text;
using Windows.Media.SpeechSynthesis;
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
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Prefer a natural/neural voice if available, otherwise use requested or default
        var bestVoice = SpeechSynthesizer.AllVoices
            .Where(v => v.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(v => v.DisplayName.Contains("Natural", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(v => v.Gender == VoiceGender.Female) // Female voices tend to sound clearer
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(voice))
        {
            var match = SpeechSynthesizer.AllVoices
                .FirstOrDefault(v => v.DisplayName.Contains(voice, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                bestVoice = match;
        }

        if (bestVoice != null)
            _synth.Voice = bestVoice;

        // Build SSML for better prosody (paragraph structure, natural pacing)
        var ssml = BuildSsml(text, _synth.Voice.DisplayName, rate);

        SentenceReached?.Invoke(0);

        using var stream = await _synth.SynthesizeSsmlToStreamAsync(ssml).AsTask(_cts.Token);
        using var msStream = new MemoryStream();
        using var inputStream = stream.GetInputStreamAt(0);
        using var dataReader = new Windows.Storage.Streams.DataReader(inputStream);
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

        // Pause/resume loop
        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested && player.PlaybackState != PlaybackState.Stopped)
            {
                _pauseGate.Wait(_cts.Token);
                await Task.Delay(50, _cts.Token).ConfigureAwait(false);
            }
        }, _cts.Token).ContinueWith(_ => { }, TaskScheduler.Default);

        using var reg = _cts.Token.Register(() =>
        {
            player.Stop();
            tcs.TrySetResult();
        });

        await tcs.Task;
        _player = null;

        if (!_cts.Token.IsCancellationRequested)
            PlaybackFinished?.Invoke();
    }

    private static string BuildSsml(string text, string voiceName, float rate)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var sb = new StringBuilder();
        sb.Append("<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>");
        sb.Append($"<voice name='{EscapeXml(voiceName)}'>");

        // Slight pitch reduction and calm rate for a warmer tone
        var ratePercent = (int)((rate - 1.0f) * 100);
        sb.Append($"<prosody rate='{ratePercent}%' pitch='-5%'>");

        foreach (var line in lines)
        {
            sb.Append("<s>");
            sb.Append(EscapeXml(line));
            sb.Append("</s>");
        }

        sb.Append("</prosody></voice></speak>");
        return sb.ToString();
    }

    private static string EscapeXml(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;");

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
