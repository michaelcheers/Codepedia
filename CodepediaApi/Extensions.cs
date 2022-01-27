using AngleSharp.Dom;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Codepedia
{
    public static partial class Extensions
    {
        //public static string GetCode(this FencedCodeBlock block)
        //{
        //    var code = new StringBuilder();
        //    string? firstLine = null;
        //    foreach (var line in block.Lines.Lines)
        //    {
        //        var slice = line.Slice;
        //        if (slice.Text == null)
        //        {
        //            continue;
        //        }

        //        var lineText = slice.Text.Substring(slice.Start, slice.Length);

        //        if (firstLine == null)
        //        {
        //            firstLine = lineText;
        //        }
        //        else
        //        {
        //            code.AppendLine();
        //        }

        //        code.Append(lineText);
        //    }
        //    return code.ToString();
        //}
        //public static string GetText (this HeadingBlock heading)
        //{
        //    ContainerInline? headingInline = heading.Inline;
        //    if (headingInline == null) return "";
        //    LiteralInline? inline = headingInline.FirstChild as LiteralInline;
        //    if (inline == null) return "";
        //    return inline.Content.ToString();
        //}
        public static bool IsCSharpCode(this FencedCodeBlock block)
            => block.Info!.Replace(((FencedCodeBlockParser)block.Parser!).InfoPrefix, string.Empty).ToLower() is "c#" or "csharp";
        public static TValue TryGetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, Func<TValue> toAdd)
        {
            if (d.TryGetValue(key, out TValue value))
                return value;
            TValue adding = toAdd();
            lock (d)
            {
                if (d.TryGetValue(key, out value))
                    return value;
                d.Add(key, adding);
            }
            return adding;
        }
    }
}
