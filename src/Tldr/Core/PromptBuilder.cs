using System.IO;
using System.Text.Json;

namespace Tldr.Core;

public sealed class PromptBuilder
{
    private readonly Dictionary<string, Dictionary<string, string>> _fragments;
    private readonly string _base;

    public PromptBuilder(string promptsJsonPath)
    {
        var json = File.ReadAllText(promptsJsonPath);
        using var doc = JsonDocument.Parse(json);

        _base = doc.RootElement.GetProperty("base").GetString()
            ?? throw new InvalidOperationException("Missing 'base' prompt fragment.");

        _fragments = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var category in new[] { "style", "detail", "tone" })
        {
            var section = doc.RootElement.GetProperty(category);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in section.EnumerateObject())
                dict[prop.Name] = prop.Value.GetString() ?? "";
            _fragments[category] = dict;
        }

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
        if (!_fragments.TryGetValue(category, out var section) || !section.TryGetValue(key, out var value))
            throw new InvalidOperationException($"Missing prompt fragment: {category}.{key}");
        return value;
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
