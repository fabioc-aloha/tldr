using Markdig;

namespace Tldr.Core;

public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public static string ToHtml(string markdown)
    {
        return Markdig.Markdown.ToHtml(markdown, Pipeline);
    }
}
