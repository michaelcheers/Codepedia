using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Markdig;
using Markdig.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Codepedia
{
    public static class CodepediaApi
    {
        private static readonly RestClient client = new RestClient("https://localhost:5001");
        private static readonly Dictionary<string, TaskManager<SearchResult[]>> responses = new Dictionary<string, TaskManager<SearchResult[]>>();

        public static async Task<SearchResult[]> Search(string query, CancellationToken cancellationToken)
        {
            try
            {
                return await responses.TryGetOrAdd(SearchRegex.Replace(query),
                    () => new TaskManager<SearchResult[]>(async c =>
                    {
                        RestRequest request = new RestRequest("/api/search").AddQueryParameter("q", query);
                        RestResponse response = await client.GetAsync(request, c);

                        string? content = response.Content;
                        if (content == null) return new SearchResult[0];

                        SearchResult[]? result = JsonSerializer.Deserialize<SearchResult[]>(content, new() { IncludeFields = true, PropertyNameCaseInsensitive = true });
                        if (result == null) return new SearchResult[0];

                        return result;
                    })
                ).Value(cancellationToken);
            }
            catch
            {
                return new SearchResult[0];
            }
        }

        // > Search Result
        // -> Headings*
        // --> Lang Blocks
        // ---> Code Blocks
        // ----> (Functions & Classes)*
        // -----> Dependencies

        // using SearchResultsAnnotations = Dictionary<SearchResult, List<Heading>>;
        // class Heading { string heading; List<Heading> subHeadings; List<CodeRoot> codeBlocks; }
        // class CodeEncloser { List<Class> classes; List<MethodDeclarationSyntax> methods; }
        // class CodeRoot : CodeEncloser { }
        // class Class : CodeEncloser { ClassDeclarationSyntax @class; }

        //    public static async Task<List<MethodDeclarationSyntax>> GetMethodDeclarationsAsync(this IEnumerable<SearchResult> searchResults, CancellationToken cancellationToken)
        //    {
        //        List<MethodDeclarationSyntax> methodDeclarations = new List<MethodDeclarationSyntax>();

        //        foreach (SearchResult searchResult in searchResults)
        //        {
        //            MarkdownDocument markdown = Markdown.Parse(searchResult.Markdown);

        //            foreach (MarkdownObject descendant in markdown.Descendants())
        //            {
        //                if (descendant is HeadingBlock heading)
        //                {
        //                }
        //            }

        //            foreach (FencedCodeBlock codeBlock in markdown.Descendants().OfType<FencedCodeBlock>().Where(block => block.IsCSharpCode()))
        //            {
        //                string code = codeBlock.GetCode();
        //                SyntaxNode rootNode = await CSharpSyntaxTree.ParseText(code).GetRootAsync(cancellationToken);

        //                foreach (MethodDeclarationSyntax methodDeclaration in rootNode.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>())
        //                {
        //                    methodDeclarations.Add(methodDeclaration);
        //                }
        //            }
        //        }

        //        return methodDeclarations;
        //    }
        //}

        public static async Task<WikiEntry> InterpretMarkdown(string markdown, CancellationToken cancellationToken)
        {
            string html = Markdown.ToHtml(markdown);
            IHtmlDocument doc = await new HtmlParser().ParseDocumentAsync(html, cancellationToken);

            WikiEntry wikiEntry = new WikiEntry();

            Node topNode = wikiEntry;

            foreach (IElement csBlock in doc.QuerySelectorAll(@"div[data-lang=""c#""]"))
                await ExploreChildren(csBlock);

            async Task ExploreChildren(IElement upper)
            {
                foreach (IElement node in upper.Children)
                {
                    switch (node.TagName.ToLowerInvariant())
                    {
                        case "code":
                            if (node.ClassList.Any(c => c is "language-c#" or "language-csharp"))
                            {
                                CodeBlock codeBlock = new CodeBlock(topNode);
                                SyntaxTree syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(node.TextContent);
                                SyntaxNode root = await syntaxTree.GetRootAsync();
                                ExploreCodeNode(codeBlock, root);
                            }
                            break;
                        case "h1" or "h2" or "h3" or "h4" or "h5" or "h6":
                            int level = int.Parse(node.TagName.Substring(1));
                            while (topNode is Heading previousHeading && previousHeading.Level >= level)
                                topNode = previousHeading.Parent;
                            topNode = new Heading(topNode) { Level = level, Text = node.TextContent };
                            break;
                        default:
                            await ExploreChildren(node);
                            break;
                    }
                }
            }

            return wikiEntry;
        }

        public static async Task<Dictionary<SearchResult, WikiEntry>> InterpretSearchResults (IEnumerable<SearchResult> searchResults, CancellationToken cancellationToken)
        {
            Dictionary<SearchResult, WikiEntry> result = new Dictionary<SearchResult, WikiEntry>();
            foreach (SearchResult searchResult in searchResults)
                result.Add(searchResult, await InterpretMarkdown(searchResult.Markdown, cancellationToken));
            return result;
        }

        public static void ExploreCodeNode (CodeNode root, SyntaxNode rootNode)
        {
            CodeAnnotations annotations = new CodeAnnotations();
            IEnumerable<SyntaxTrivia> lastTrivia = new SyntaxTrivia[0];

            void AddAnnotations ()
            {
                //if (!root.Children.Any() && root.Annotations == null)
                root.Annotations.NugetPackages.AddRange(annotations.NugetPackages);
                root.Annotations.Usings.AddRange(annotations.Usings);
                annotations = new CodeAnnotations();
            }

            void ExploreTriviaList(IEnumerable<SyntaxTrivia> triviaList)
            {
                SyntaxTrivia? last = null;
                foreach (SyntaxTrivia trivia in triviaList)
                {
                    switch (trivia.Kind())
                    {
                        case SyntaxKind.EndOfFileToken:
                        case SyntaxKind.EndOfLineTrivia when last?.Kind() == SyntaxKind.EndOfLineTrivia:
                            AddAnnotations();
                            break;
                        case SyntaxKind.SingleLineCommentTrivia:
                            string commentStr = trivia.ToFullString();
                            if (commentStr.StartsWith("// @nuget "))
                                annotations.NugetPackages.Add(commentStr.Substring("// @nuget ".Length));
                                break;
                    }
                    last = trivia;
                }
            }

            foreach (SyntaxNode node in rootNode.ChildNodes())
            {
                ExploreTriviaList(lastTrivia.Concat(node.GetLeadingTrivia()));

                switch (node)
                {
                    case UsingDirectiveSyntax usingDirective:
                        annotations.Usings.Add(usingDirective);
                        break;
                    case GlobalStatementSyntax globalStatement:
                        IEnumerable<SyntaxNode> childNodes = globalStatement.ChildNodes();
                        if (childNodes.Count() == 1 && childNodes.Single() is LocalFunctionStatementSyntax function
                            && function.Modifiers.Any(SyntaxKind.StaticKeyword) && function.Modifiers.Any(SyntaxKind.PublicKeyword)
                        )
                        {
                            new Method(root) { Annotations = annotations, Declaration = function };
                            annotations = new();
                        }
                        break;
                    case MethodDeclarationSyntax methodDeclaration when methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) && methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword):
                        new Method(root) { Annotations = annotations, Declaration = methodDeclaration };
                        annotations = new();
                        break;
                    case TypeDeclarationSyntax typeDeclaration:
                        Type type = new Type(root) { Declaration = typeDeclaration };
                        ExploreCodeNode(type, typeDeclaration);
                        break;
                }

                lastTrivia = node.GetTrailingTrivia();
            }

            ExploreTriviaList(lastTrivia);
            AddAnnotations();
        }
    }
}
