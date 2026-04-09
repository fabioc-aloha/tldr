using System.IO;
using System.Text.Json;

namespace Tldr.Core;

public sealed class PromptBuilder
{
    private readonly JsonDocument _prompts;
    private readonly string _base;

    public PromptBuilder(string promptsJsonPath)
    {
        var json = File.ReadAllText(promptsJsonPath);
        _prompts = JsonDocument.Parse(json);

        _base = _prompts.RootElement.GetProperty("base").GetString()
            ?? throw new InvalidOperationException("Missing 'base' prompt fragment.");

        ValidateFragments();
    }

    public string Build(SummaryStyle style, DetailLevel detail, Tone tone)
    {
        var parts = new List<string> { _base };

        parts.Add(GetFragment("style", style.ToString()));
        parts.Add(GetFragment("detail", detail.ToString()));

        var toneFragment = GetFragment("tone", tone.ToString());
        if (!string.IsNullOrWhiteSpace(toneFragment))
            parts.Add(toneFragment);

        return string.Join(" ", parts);
    }

    private string GetFragment(string category, string key)
    {
        var section = _prompts.RootElement.GetProperty(category);
        return section.GetProperty(key).GetString()
            ?? throw new InvalidOperationException($"Missing prompt fragment: {category}.{key}");
    }

    private void ValidateFragments()
    {
        foreach (var style in Enum.GetNames<SummaryStyle>())
            GetFragment("style", style);

        foreach (var detail in Enum.GetNames<DetailLevel>())
            GetFragment("detail", detail);

        foreach (var tone in Enum.GetNames<Tone>())
            GetFragment("tone", tone);
    }
}
