using Markdig;
using Markdig.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Codepedia
{
    public class UnresolvedMethodCallInfo
    {
        public InvocationExpressionSyntax? Invocation;
        public SimpleNameSyntax NameSyntax;
        public string Name => NameSyntax.Identifier.ValueText;
    }

    public static class CodeAnalyzer
    {
        public static async Task<SyntaxNode?> GetOuterNodeAsync (SemanticModel semanticModel, int tokenPosition, CancellationToken cancellationToken)
        {
            SyntaxNode syntaxTree = await semanticModel.SyntaxTree.GetRootAsync(cancellationToken);

            SyntaxNode? parent = syntaxTree.FindToken(tokenPosition).Parent, expressionNode;
            if (parent == null) return null;

            do
            {
                expressionNode = parent;
                parent = parent.Parent;
            }
            while (parent is (ExpressionOrPatternSyntax or StatementSyntax));

            return expressionNode;
        }

        public static UnresolvedMethodCallInfo? FindUnresolvedInvocation(this SyntaxNode outerNode, SemanticModel semanticModel)
        {
            foreach (SyntaxNode syntax in outerNode.DescendantNodesAndSelf())
            {
                SimpleNameSyntax? nameSyntax;
                switch (syntax)
                {
                    case InvocationExpressionSyntax invocation:
                        nameSyntax = invocation.Expression as SimpleNameSyntax;
                        break;
                    case IdentifierNameSyntax identifier:
                        nameSyntax = identifier;
                        break;
                    case MemberAccessExpressionSyntax memberAccessSyntax:
                        if (memberAccessSyntax.IsKind(SyntaxKind.PointerMemberAccessExpression)) continue;
                        nameSyntax = memberAccessSyntax.Name;
                        break;
                    default:
                        continue;
                }

                if (nameSyntax == null || semanticModel.GetSymbolInfo(nameSyntax) is { Symbol: ISymbol } or { CandidateSymbols.Length: >0 }) continue;

                return new UnresolvedMethodCallInfo
                {
                    Invocation = syntax as InvocationExpressionSyntax,
                    NameSyntax = nameSyntax
                };
            }

            return null;
        }
    }
}
