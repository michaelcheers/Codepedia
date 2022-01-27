using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Codepedia
{
    public static class CodeModifier
    {
        public static async Task<string?> GetDocumentText (this Project project, string documentName)
        {
            Document? document = project.Documents.FirstOrDefault(d => d.Name == documentName);
            if (document == null) return null;
            return (await document.GetTextAsync()).ToString();
        }
        
        public static bool SetDocumentText (this Project project, string documentName, string documentContent)
        {
            Document? document = project.Documents.FirstOrDefault(d => d.Name == documentName);
            
            SourceText text = SourceText.From(documentContent);

            if (document == null)
            {
                document = project.AddDocument(documentName, text);
            }
            else
            {
                document = document.WithText(text);
            }

            return project.Solution.Workspace.TryApplyChanges(document.Project.Solution);
        }
    }
}
