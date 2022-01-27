using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Codepedia
{
    public static partial class Extensions
    {
        public static string Replace(this IEnumerable<(Regex start, string end)> replacementList, string str)
        {
            foreach ((Regex start, string end) in replacementList)
                str = start.Replace(str, end);
            return str;
        }
    }
    public static class SearchRegex
    {
        // Source: generateSearchRegex.js
        private static readonly (Regex start, string end)[] searchRegexList = new (Regex start, string end)[]
        {
            (new Regex(@"\W"), @"  "),
            (new Regex(@" [a-zA-Z]([a-z]*) "), @"  "),
            (new Regex(@"([A-Z]{2,})([a-z]+)"), @"$0 $1 $2"),
            (new Regex(@"([a-zA-Z][A-Z]*)([A-Z]| )"), @"$1  $2"),
            (new Regex(@"([a-zA-Z][a-z]+)"), @"$1  "),
            (new Regex(@"_"), @"  "),
            (new Regex(@"([a-zA-Z]([A-Z][A-Z]*|[a-z][a-z]*)|[0-9]+)"), @"$1 "),
            (new Regex(@" +"), @" "),
            (new Regex(@"^( ?)(.*)( +)$"), @"$2")
        };

        public static string Replace (string str) => searchRegexList.Replace(str);
    }
}
