using Codepedia.DB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codepedia
{
    public static partial class Extensions
    {
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => (from s in source
                let k = source.Min(keySelector)
                where k.Equals(keySelector(s))
                select s
            ).First();

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => (from s in source
               let k = source.Max(keySelector)
               where k.Equals(keySelector(s))
               select s
            ).First();

        public static DateTime GetUTCDateTime(this MySqlDataReader reader, string k) =>
            DateTime.SpecifyKind(reader.GetDateTime(k), DateTimeKind.Utc);

        /// <summary>
        /// Only use after confirming the user is already logged in. Either with [Authorize] or HttpContext.UserRole() != UserRole.Anonymous
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static int UserID (this HttpContext context) =>
            int.Parse(context.User.FindFirstValue("ID"));

        /// <summary>
        /// Only use after confirming the user is already logged in. Either with [Authorize] or HttpContext.UserRole() != UserRole.Anonymous
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string Username (this HttpContext context) =>
            context.User.FindFirst("Username").Value;

        public static UserRole UserRole (this HttpContext context) =>
            context.User.HasClaim(ClaimTypes.Role, "Admin") ?
                Codepedia.UserRole.Admin :
                context.User.HasClaim(ClaimTypes.Role, "User") ?
                    Codepedia.UserRole.User :
                    Codepedia.UserRole.Anonymous;

        public static bool CanEdit(this WikiSuggestion suggestion) =>
            suggestion.Status == "Unreviewed" && suggestion.MergingCommitId == null && suggestion.ReputationAwarded == null;

        public static string ToRelativeTime (this DateTime dateTime)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }

        public static string ToJson (this object o) => JsonSerializer.Serialize(o, new()
        {
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        public static T ToJson<T> (this string json) => JsonSerializer.Deserialize<T>(json, new()
        {
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        public static TValue TryGetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, Func<TValue> toAdd) where TValue : class
        {
            if (d.TryGetValue(key, out var value))
                return value;
            else
            {
                var adding = toAdd();
                if (adding == null) return null;
                d.Add(key, adding);
                return adding;
            }
        }
    }

    public enum UserRole
    {
        Anonymous,
        User,
        Admin
    }
}
