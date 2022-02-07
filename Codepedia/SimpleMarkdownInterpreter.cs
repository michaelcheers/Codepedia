using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Text;
using System.Web;

namespace Codepedia
{
    public static class SimpleMarkdownInterpreter
    {
        public static string ToHTMLSummary (string markdown)
            => ToHTMLSummary(InterpretMarkdown(markdown));
        public static string ToHTMLSummary (this Node node)
        {
            StringBuilder builder = new();
            node.ToHTMLSummary(builder);
            return builder.ToString();
        }
        public static void ToHTMLSummary (this Node node, StringBuilder builder)
        {
            if (node is Heading heading)
                builder.Append(HttpUtility.HtmlEncode(heading.Text));

            if (node.Children.Count == 0) return;

            builder.Append("<ul>");
            foreach (Node child in node.Children)
            {
                builder.Append("<li>");
                child.ToHTMLSummary(builder);
                builder.Append("</li>");
            }
            builder.Append("</ul>");
        }
        public static EntryInfo InterpretMarkdown (string markdown)
        {
            IHtmlDocument doc = new HtmlParser().ParseDocument(Functions.DisplayMarkdown(markdown));

            EntryInfo wikiEntry = new EntryInfo();
            Node topNode = wikiEntry;

            ExploreChildren(doc.Body);

            return wikiEntry;

            void ExploreChildren (IElement upper)
            {
                foreach (IElement node in upper.Children)
                {
                    switch (node.TagName.ToLowerInvariant())
                    {
                        case "h1" or "h2" or "h3" or "h4" or "h5" or "h6":
                            int level = int.Parse(node.TagName.Substring(1));
                            while (topNode is Heading previousHeading && previousHeading.Level >= level)
                                topNode = previousHeading.Parent;
                            topNode = new Heading(topNode) { Level = level, Text = node.TextContent };
                            break;
                        default:
                            ExploreChildren(node);
                            break;
                    }
                }
            }
        }
    }
}
