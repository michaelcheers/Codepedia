using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using MoreLinq;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

#nullable enable

namespace Codepedia.Lightbulb
{
    //public class Config
    //{
    //    public Func<SnapshotSpan, CancellationToken, Task<bool>> HasSuggestedActions;
    //    public Func<SnapshotSpan, CancellationToken, Task<IEnumerable<SuggestedActionSet>>> GetSuggestedActions;
    //}

    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("Codepedia Import")]
    [ContentType("text")]
    internal class SourceProvider : ISuggestedActionsSourceProvider
    {
        public Workspace Workspace;

        [ImportingConstructor]
        public SourceProvider([Import(typeof(VisualStudioWorkspace), AllowDefault = true)] Workspace workspace)
        {
            Workspace = workspace;
        }

        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer) => new ActionSource
        {
            Workspace = Workspace
            //Config = new()
            //{

            //HasSuggestedActions = async (SnapshotSpan span, CancellationToken cancellationToken) =>
            //    (await new CodepediaUtil(Workspace, span).SearchResults.Value(cancellationToken)).Any(),

            //GetSuggestedActions = async (SnapshotSpan span, CancellationToken cancellationToken) =>
            //    (await new CodepediaUtil(Workspace, span).SearchResultsInfo.Value()).Any()
            //}
        };
    }

    internal class ActionSource : ISuggestedActionsSource
    {
        public Workspace Workspace;

        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public Dictionary<SnapshotSpan, CodepediaUtil> Utils = new Dictionary<SnapshotSpan, CodepediaUtil>();

        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }

        public CodepediaUtil GetUtil(SnapshotSpan range)
            => /*Utils.TryGetOrAdd(range, () => */new CodepediaUtil(Workspace, range);//);

        public async Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
            => (await (GetUtil(range).SearchResults.Value(cancellationToken))).Any();

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken ct)
        {
            async Task<IEnumerable<SuggestedActionSet>> _ ()
            {
                CodepediaUtil util = GetUtil(range);
                var nullableCodeAnalysisInfo = await util.CodeAnalysisInfo.Value(ct);
                if (nullableCodeAnalysisInfo is not
                    (Document document, SemanticModel semanticModel, SyntaxNode outerNode, UnresolvedMethodCallInfo unresolvedInvocation) codeAnalysisInfo
                ) return Enumerable.Empty<SuggestedActionSet>();
                return new SuggestedActionSet[]
                {
                    new SuggestedActionSet
                    (
                        PredefinedSuggestedActionCategoryNames.ErrorFix,
                        new ISuggestedAction[] 
                        {
                            new ActionParent($"Explore {codeAnalysisInfo.unresolvedInvocation.Name} on Codepedia", async cancellationToken =>
                            {
                                Document doc = nullableCodeAnalysisInfo.Value.document;

                                SearchResult[] searchResults = await util.SearchResults.Value(cancellationToken);
                                Dictionary<SearchResult, WikiEntry> wikiEntries = await util.SearchResultsInfo.Value(cancellationToken);
                                KeyValuePair<SearchResult, WikiEntry>[] searchResultsInfo = searchResults.Select(res => new KeyValuePair<SearchResult, WikiEntry>(res, wikiEntries[res])).ToArray();

                                foreach (KeyValuePair<SearchResult, WikiEntry> searchResultInfo in searchResultsInfo)
                                {
                                    BaseAction CreateImportAction(string displayText, Method method)
                                    {
                                        return new BaseAction(displayText, method, doc);
                                    }

                                    Method[] methods = searchResultInfo.Value.Descendants().OfType<Method>().ToArray();
                                    if (methods.Length == 0)
                                        continue;
                                    Method met = methods.First();
                                    return new SuggestedActionSet[]
                                    {
                                        new SuggestedActionSet
                                        (
                                            PredefinedSuggestedActionCategoryNames.ErrorFix,
                                            new[] { CreateImportAction($"Import '{met.Name}' from Codepedia", met) },
                                            title: "Codepedia",
                                            applicableToSpan: new Span(codeAnalysisInfo.outerNode.Span.Start, codeAnalysisInfo.outerNode.Span.Length)
                                        )
                                    };
                                }

                                return Enumerable.Empty<SuggestedActionSet>();

                            })
                        },
                        title: "Codepedia",
                        applicableToSpan: new Span(codeAnalysisInfo.outerNode.Span.Start, codeAnalysisInfo.outerNode.Span.Length)
                    )
                };
            }
            return ThreadHelper.JoinableTaskFactory.Run(_);
        }
    }

    internal class ActionParent : ISuggestedAction
    {
        public ActionParent(string displayText, Func<CancellationToken, Task<IEnumerable<SuggestedActionSet>>> suggestedActionsF)
        {
            DisplayText = displayText;
            _suggestedActionsManager = new TaskManager<IEnumerable<SuggestedActionSet>>(suggestedActionsF);
        }

        readonly TaskManager<IEnumerable<SuggestedActionSet>> _suggestedActionsManager;
        public string DisplayText { get; }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
            => _suggestedActionsManager.Value(cancellationToken);

        public void Invoke(CancellationToken cancellationToken) { }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken) => null;
        public bool HasActionSets => true;
        public ImageMoniker IconMoniker => default(ImageMoniker);
        public string IconAutomationText => null;
        public string InputGestureText => null;
        public bool HasPreview => false;

        public void Dispose() { }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }

    internal class BaseAction : ISuggestedAction
    {
        public BaseAction (string displayText, Method method, Document document)
        {
            DisplayText = displayText;
            Method = method;
            Document = document;
        }

        public Project Project => Document.Project;
        public VisualStudioWorkspace Workspace => (VisualStudioWorkspace)Project.Solution.Workspace;

        public readonly Document Document;
        public readonly Method Method;
        public string DisplayText { get; }

        public async Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken) => null;

        public bool HasActionSets => false;
        public ImageMoniker IconMoniker => default(ImageMoniker);
        public string IconAutomationText => null;
        public string InputGestureText => null;
        public bool HasPreview => false;

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            return null;
            //foreach (string nuget in Method.AllRequiredAnnotations.NugetPackages)

            // ...
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            async Task _ ()
            {
                MethodDeclarationSyntax declaration =
                    (MethodDeclarationSyntax)(
                        ((ClassDeclarationSyntax)(await CSharpSyntaxTree.ParseText("class _ { " + Method.Declaration + " }").GetRootAsync()).ChildNodes().Single())
                        .ChildNodes().Single()
                    );
                string? text = await Project.GetDocumentText("Codepedia.cs");
                text ??= string.Concat(new[]
                    {
                    "System",
                    "System.Collections.Immutable",
                    "System.Collections.Generic",
                    "System.IO",
                    "System.Linq",
                    "System.Text",
                    "System.Text.RegularExpressions",
                    "System.Threading",
                    "System.Threading.Tasks"
                    }.Select(u => $"using {u};"));
                CompilationUnitSyntax codepediaTree = CSharpSyntaxTree.ParseText(text).GetCompilationUnitRoot();
                ClassDeclarationSyntax? codepediaClass =
                    codepediaTree.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(n => n.Identifier.ValueText == "Codepedia" );
                if (codepediaClass == null)
                    codepediaClass = SyntaxFactory.ClassDeclaration("Codepedia")
                        .WithModifiers(
                            SyntaxFactory.TokenList(
                                SyntaxFactory.Token(SyntaxKind.InternalKeyword),
                                SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                            )
                        );
                else
                    codepediaTree = codepediaTree.RemoveNode(codepediaClass, SyntaxRemoveOptions.KeepNoTrivia)!;
                codepediaTree = codepediaTree.AddMembers(codepediaClass.AddMembers(declaration));
                IEnumerable<UsingDirectiveSyntax> usings = codepediaTree.Usings.Concat(Method.AllRequiredAnnotations.Usings).Select(u => u.WithoutTrivia())
                    .DistinctBy(u => u.Name.NormalizeWhitespace().ToString());
                codepediaTree = codepediaTree.RemoveNodes(codepediaTree.Usings, SyntaxRemoveOptions.KeepNoTrivia)!.WithUsings(SyntaxFactory.List(usings));
                Project.SetDocumentText("Codepedia.cs", codepediaTree.NormalizeWhitespace().ToFullString());
                Project.InstallNugetPackages(Method.AllRequiredAnnotations.NugetPackages);
            }
            ThreadHelper.JoinableTaskFactory.Run(_);
        }

        public void Dispose() { }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }

    public static class VSExtensions
    {
        public static void InstallNugetPackages(this Project project, IEnumerable<string> nugetPackages)
        {
            foreach (var package in nugetPackages)
            {
                Process.Start(new ProcessStartInfo("dotnet", $"add package {package}") { WorkingDirectory = Path.GetDirectoryName(project.FilePath)} ).WaitForExit();
            }
        }
    }
}
