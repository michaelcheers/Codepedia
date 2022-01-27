using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Threading;
using Markdig;
using Markdig.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Imaging.Interop;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Codepedia
{

    internal class CodepediaImportAction : ISuggestedAction
    {
        InvocationExpressionSyntax Invocation;
        string Name;
        SearchResult SearchResult;
        public string Code;

        public CodepediaImportAction(InvocationExpressionSyntax invocation, string name, SearchResult searchResult)
        {
            Invocation = invocation;
            Name = name;
            SearchResult = searchResult;

            string code = Markdown.Parse(searchResult.Markdown)
                .Descendants().OfType<FencedCodeBlock>().FirstOrDefault(block => block.IsCSharpCode())?.GetCode();

            if (code == null) return;

            IEnumerable<MethodDeclarationSyntax> methodsDeclared = CSharpSyntaxTree.ParseText(code).GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>();

            
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
        }

        public async Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken) => null;

        public bool HasActionSets => false;
        public string DisplayText => $"Import '{Name}' from Codepedia";
        public ImageMoniker IconMoniker => default(ImageMoniker);
        public string IconAutomationText => null;
        public string InputGestureText => null;
        public bool HasPreview => true;

        public void Invoke(CancellationToken cancellationToken)
        {
        }

        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample action and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
