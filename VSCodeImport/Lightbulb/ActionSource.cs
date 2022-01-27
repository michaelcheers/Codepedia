using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System.Threading;
using RestSharp;
using Codepedia.Code_Analysis;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codepedia
{
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("Test Suggested Actions")]
    [ContentType("text")]
    internal class ActionSourceProvider : ISuggestedActionsSourceProvider
    {
        private CodeProvider codeProvider;

        [ImportingConstructor]
        public ActionSourceProvider([Import(typeof(VisualStudioWorkspace), AllowDefault = true)] Workspace workspace)
        {
            this.codeProvider = new CodeProvider(workspace);
        }

        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        {
            if (textBuffer == null || textView == null)
            {
                return null;
            }
            return new ActionSource(codeProvider);
        }
    }

    internal class ActionSource : ISuggestedActionsSource
    {
        private CodeProvider _codeProvider;

        public ActionSource (CodeProvider codeProvider)
        {
            this._codeProvider = codeProvider;
        }

        private static async IAsyncEnumerable<(InvocationExpressionSyntax invocation, string name, SearchResult searchResult)> SearchCodepediaAPI (
            IAsyncEnumerable<(InvocationExpressionSyntax invocation, string name)> methods, CancellationToken cancellationToken
        )
        {
            await foreach ((InvocationExpressionSyntax invocation, string name) in methods)
            {
                foreach (SearchResult result in await Search(name, cancellationToken))
                {
                    yield return (invocation, name, result);
                }
            }
        }

        private async Task<(InvocationExpressionSyntax invocation, string name, SearchResult searchResult)?> GetResult (
            SnapshotSpan range, CancellationToken cancellationToken
        )
        {
            Document document = range.Snapshot.TextBuffer.GetRelatedDocuments().FirstOrDefault();
            if (document == null) return null;
            await foreach (
                (InvocationExpressionSyntax invocation, string name, SearchResult searchResult) result in
                SearchCodepediaAPI(_codeProvider.FindUnresolvedInvocation(document, range.Start, cancellationToken), cancellationToken))
            {
                return result;
            }
            return null;
        }

        public async Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            return GetResult(range, cancellationToken) != null;
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            if (GetResult(range, cancellationToken).Result is not {} searchResult) yield break;
            IEnumerable<CodepediaImportAction> Actions() { yield return new CodepediaImportAction(searchResult); }
            yield return new SuggestedActionSet(Actions()); PredefinedSuggestedActionCategoryNames
        }

        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample provider and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
