using Markdig;
using Markdown.ColorCode;

namespace Codepedia
{
    public static class Functions
    {
        public static string DisplayMarkdown (string markdown)
        {
            return "<div style=\"tab-size:4\">" +
                Markdig.Markdown.ToHtml(markdown, new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .UseColorCode()
                    .Build()) +
                "</div>";
        }
    }
}
