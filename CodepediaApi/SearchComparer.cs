using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codepedia
{
    public static class SearchComparer
    {
        static string[] GetSearchTerms(string name) => SearchRegex.Replace(name).ToLowerInvariant().Split(' ');

        public static double Compare (string a, string b) =>
            (from term_a in GetSearchTerms(a)
             from term_b in GetSearchTerms(b)
             select CompareSearchTerms(term_a, term_b)).Average();

        static double CompareSearchTerms (string a, string b) =>
            (a.Length <= 1 || b.Length <= 1) switch
            {
                false => 0,
                true => (a.Contains(b), b.Contains(a)) switch
                {
                    (false, false) => 0,
                    (false, true) or (true, false) => 1,
                    (true, true) => 1.01
                }
            };
    }
}
