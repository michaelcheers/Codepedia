using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Codepedia
{
    public static partial class Extensions
    {
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => (from s in source
               let k = source.Max(keySelector)
               where k.Equals(keySelector(s))
               select s
            ).First();

        public static DateTime GetUTCDateTime(this MySqlDataReader reader, string k) =>
            DateTime.SpecifyKind(reader.GetDateTime(k), DateTimeKind.Utc);
    }
}
