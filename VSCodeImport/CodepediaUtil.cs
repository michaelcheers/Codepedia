using Markdig;
using Markdig.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Codepedia
{
    internal class CodepediaUtil
    {
        public readonly Workspace Workspace;
        public readonly SnapshotSpan Range;

        public readonly TaskManager<(Document document, SemanticModel semanticModel, SyntaxNode outerNode, UnresolvedMethodCallInfo unresolvedInvocation)?> CodeAnalysisInfo;
        public readonly TaskManager<SearchResult[]> SearchResults;
        public readonly TaskManager<Dictionary<SearchResult, WikiEntry>> SearchResultsInfo;

        public async Task<UnresolvedMethodCallInfo?> UnresolvedInvocation (CancellationToken cancellationToken)
        {
            var unresolvedInvocationInfo = await CodeAnalysisInfo.Value(cancellationToken);
            if (unresolvedInvocationInfo is not (_, _, _, UnresolvedMethodCallInfo unresolvedInvocation)) return null;

            return unresolvedInvocation;
        }

        public CodepediaUtil (Workspace workspace, SnapshotSpan range)
        {
            Workspace = workspace;
            Range = range;

            CodeAnalysisInfo = new TaskManager<(Document document, SemanticModel semanticModel, SyntaxNode outerNode, UnresolvedMethodCallInfo unresolvedInvocation)?>
            (
                async (CancellationToken cancellationToken) =>
                {
                    Document? document = Range.Snapshot.TextBuffer.GetRelatedDocuments().FirstOrDefault();
                    if (document == null) return null;

                    SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                    if (semanticModel == null) return null;

                    SyntaxNode? outerNode = await CodeAnalyzer.GetOuterNodeAsync(semanticModel, Range.Start, cancellationToken);
                    if (outerNode == null) return null;

                    UnresolvedMethodCallInfo? unresolvedInvocation = outerNode.FindUnresolvedInvocation(semanticModel);
                    if (unresolvedInvocation == null) return null;

                    return (document, semanticModel, outerNode, unresolvedInvocation);
                }
            );
            SearchResults = new TaskManager<SearchResult[]>
            (
                async (CancellationToken cancellationToken) =>
                {
                    UnresolvedMethodCallInfo? unresolvedInvocation = await UnresolvedInvocation(cancellationToken);
                    if (unresolvedInvocation == null) return new SearchResult[0];

                    return await CodepediaApi.Search(unresolvedInvocation.Name, cancellationToken);
                }
            );
            SearchResultsInfo = new TaskManager<Dictionary<SearchResult, WikiEntry>>
            (
                async (CancellationToken cancellationToken) =>
                {
                    UnresolvedMethodCallInfo? unresolvedInvocation = await UnresolvedInvocation(cancellationToken);
                    if (unresolvedInvocation == null) return new Dictionary<SearchResult, WikiEntry>();
                    string invocationName = unresolvedInvocation.Name;

                    return await CodepediaApi.InterpretSearchResults(await SearchResults.Value(cancellationToken), cancellationToken);
                }
            );
        }
    }
}
