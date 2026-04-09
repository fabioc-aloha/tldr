namespace Tldr.Core;

public interface ITtsEngine
{
    Task SpeakAsync(string text, string voice, float rate, CancellationToken ct);
    void Pause();
    void Resume();
    void Stop();
    event Action<int>? SentenceReached;
    event Action? PlaybackFinished;
}
