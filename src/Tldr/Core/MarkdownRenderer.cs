using System.Text.RegularExpressions;
using Markdig;

namespace Tldr.Core;

public static partial class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public static string ToHtml(string markdown)
    {
        return Markdig.Markdown.ToHtml(markdown, Pipeline);
    }

    /// <summary>
    /// Wraps each block-level element (li, p, tr) with data-sentence="N" for TTS highlighting.
    /// </summary>
    public static string AddSentenceMarkers(string html)
    {
        int n = 0;
        html = SentenceHRegex().Replace(html, m => $"<h{m.Groups[1].Value} data-sentence=\"{n++}\">");
        html = SentenceLiRegex().Replace(html, _ => $"<li data-sentence=\"{n++}\">");
        html = SentencePRegex().Replace(html, _ => $"<p data-sentence=\"{n++}\">");
        html = SentenceTrRegex().Replace(html, _ => $"<tr data-sentence=\"{n++}\">");
        return html;
    }

    /// <summary>Returns the total sentence count after marking.</summary>
    public static int CountSentences(string markedHtml)
    {
        return SentenceAttrRegex().Count(markedHtml);
    }

    [GeneratedRegex(@"<h(\d)(?=>)", RegexOptions.IgnoreCase)]
    private static partial Regex SentenceHRegex();

    [GeneratedRegex(@"<li(?=>)", RegexOptions.IgnoreCase)]
    private static partial Regex SentenceLiRegex();

    [GeneratedRegex(@"<p(?=>)", RegexOptions.IgnoreCase)]
    private static partial Regex SentencePRegex();

    [GeneratedRegex(@"<tr(?=>)", RegexOptions.IgnoreCase)]
    private static partial Regex SentenceTrRegex();

    [GeneratedRegex(@"data-sentence=""\d+""")]
    private static partial Regex SentenceAttrRegex();
}
