using Microsoft.Extensions.Configuration;

namespace Tldr.Core;

public sealed class TldrConfig
{
    public HotkeyConfig Hotkeys { get; set; } = new();
    public LlmConfig Llm { get; set; } = new();
    public SummarizationConfig Summarization { get; set; } = new();
    public TtsConfig Tts { get; set; } = new();
    public WindowConfig Window { get; set; } = new();

    public static TldrConfig Load(string path)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile(path, optional: false)
            .Build();

        var tldr = new TldrConfig();
        config.Bind(tldr);
        return tldr;
    }
}

public sealed class HotkeyConfig
{
    public string OpenWindow { get; set; } = "Ctrl+Shift+S";
    public string StopReading { get; set; } = "Ctrl+Shift+X";
}

public sealed class LlmConfig
{
    public string Model { get; set; } = "phi-4-mini";
    public int MaxOutputTokens { get; set; } = 1024;
    public float Temperature { get; set; } = 0f;
}

public sealed class SummarizationConfig
{
    public string DefaultStyle { get; set; } = "Bullets";
    public string DefaultDetail { get; set; } = "Standard";
    public string DefaultTone { get; set; } = "Neutral";
}

public sealed class TtsConfig
{
    public string Engine { get; set; } = "winrt";
    public string Voice { get; set; } = "";
    public string Rate { get; set; } = "Normal";
}

public sealed class WindowConfig
{
    public bool AutoPasteClipboard { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
    public int Width { get; set; } = 480;
    public int Height { get; set; } = 640;
    public string Theme { get; set; } = "System";
}
