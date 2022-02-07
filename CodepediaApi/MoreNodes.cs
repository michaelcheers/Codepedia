using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codepedia
{
    public class CodeAnnotations
    {
        public readonly List<string> NugetPackages = new List<string>();
        public readonly List<UsingDirectiveSyntax> Usings = new List<UsingDirectiveSyntax>();
    }

    public abstract class CodeNode : ChildNode
    {
        public CodeAnnotations Annotations = new CodeAnnotations();

        public CodeNode(Node parent) : base(parent) { }

        public CodeBlock CodeBlock
        {
            get
            {
                CodeNode node = this;
                while (true)
                {
                    if (node is CodeBlock codeBlock) return codeBlock;
                    node = (CodeNode)node.Parent;
                }
            }
        }

        public CodeAnnotations AllRequiredAnnotations
        {
            get
            {
                CodeAnnotations annotations = new CodeAnnotations();

                CodeNode node = this;

                while (true)
                {
                    annotations.NugetPackages.AddRange(node.Annotations.NugetPackages);
                    annotations.Usings.AddRange(node.Annotations.Usings);

                    if (node.Parent is CodeNode codeNode)
                        node = codeNode;
                    else
                        return annotations;
                }
            }
        }
    }

    public class CodeBlock : CodeNode
    {
        public CodeBlock(Node parent) : base(parent) { }
    }

    public class Type : CodeNode
    {
        public TypeDeclarationSyntax Declaration;

        public Type(Node parent) : base(parent) { }
    }

    public class Method : CodeNode
    {
        public SyntaxNode Declaration;

        public string Name => Declaration switch
        {
            MethodDeclarationSyntax mds => mds.Identifier.ValueText,
            LocalFunctionStatementSyntax lfss => lfss.Identifier.ValueText
        };
        public ParameterListSyntax ParameterList => Declaration switch
        {
            MethodDeclarationSyntax mds => mds.ParameterList,
            LocalFunctionStatementSyntax lfss => lfss.ParameterList
        };
        public TypeSyntax ReturnType => Declaration switch
        {
            MethodDeclarationSyntax mds => mds.ReturnType,
            LocalFunctionStatementSyntax lfss => lfss.ReturnType
        };

        public Method(Node parent) : base(parent) { }
    }
}
