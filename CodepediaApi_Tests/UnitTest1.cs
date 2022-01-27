using Codepedia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CodepediaApi_Tests
{
    [TestClass]
    public class UnitTest1
    {
        static string markdown =
                @"<div data-lang=""c#"">

# Approaches

## Using iTextSharp

```csharp
// @nuget iTextSharp.LGPLv2.Core

using iTextSharp.text;
using iTextSharp.text.pdf;
public static MemoryStream MergePDFs (IEnumerable<string> pdfFiles)
{
    Document document = new();
    MemoryStream newFileStream = new();
    PdfCopy writer = new PdfCopy(document, newFileStream) { CloseStream = false };

    document.Open();

    foreach (string file in pdfFiles)
    {
        PdfReader reader = new PdfReader(file);
        reader.ConsolidateNamedDestinations();

        for (int i = 1; i <= reader.NumberOfPages; i++)
        {
            PdfImportedPage page = writer.GetImportedPage(reader, i);
            writer.AddPage(page);
        }

        PrAcroForm form = reader.AcroForm;
        if (form != null)
        {
            writer.CopyAcroForm(reader);
        }

        reader.Close();
    }

    writer.Close();
    document.Close();

    return newFileStream;
}
```

## Using Something Else

# Super Approaches

## A

## B
</div>";

        [TestMethod]
        public async Task InterpretMarkdownTest ()
        {
            WikiEntry wikiEntry = await CodepediaApi.InterpretMarkdown(markdown, default);
            Assert.AreEqual(2, wikiEntry.Children.Count);
            Assert.IsTrue(wikiEntry.Children.All(h => ((Heading)h).Level == 1));
            Assert.IsTrue(wikiEntry.Children.Select(node => ((Heading)node).Text).SequenceEqual(new[] { "Approaches", "Super Approaches" }));
            Heading h1 = (Heading)wikiEntry.Children[0];
            Assert.AreEqual("Approaches", h1.Text);
            Assert.AreEqual(1, h1.Level);
            Assert.AreEqual(2, h1.Children.Count);
            Assert.IsTrue(h1.Children.All(h => ((Heading)h).Level == 2));
            Assert.IsTrue(h1.Children.Select(node => ((Heading)node).Text).SequenceEqual(new[] { "Using iTextSharp", "Using Something Else" }));
            CodeBlock codeBlock = (CodeBlock)h1.Children[0].Children[0];
            Assert.IsTrue(codeBlock.Annotations.NugetPackages.SequenceEqual(new[] { "iTextSharp.LGPLv2.Core" }));
            Assert.AreEqual(0, codeBlock.Annotations.Usings.Count);
            Assert.AreEqual(1, codeBlock.Children.Count);
            Method m = (Method)codeBlock.Children[0];
            Assert.AreEqual(0, m.Annotations.NugetPackages.Count);
            Assert.IsTrue(m.Annotations.Usings.Select(u => u.Name.ToFullString()).SequenceEqual(new[] { "iTextSharp.text", "iTextSharp.text.pdf" }));
            Heading h2 = (Heading)wikiEntry.Children[1];
            Assert.IsTrue(h2.Children.All(h => ((Heading)h).Level == 2));
            Assert.IsTrue(h2.Children.Select(node => ((Heading)node).Text).SequenceEqual(new[] { "A", "B" }));
        }
    }
}
