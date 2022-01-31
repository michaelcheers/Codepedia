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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public static async Task<IActionResult> SignUserInAsync (this PageModel pageModel, User user, string redirectTo, string authType = "password")
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("ID", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.AuthenticationMethod, authType)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                // Refreshing the authentication session should be allowed.

                ExpiresUtc = DateTimeOffset.UtcNow.AddYears(10),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                IsPersistent = true,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                RedirectUri = redirectTo
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            await pageModel.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);

            return pageModel.LocalRedirect(redirectTo);
        }

        public static int UserID (this HttpContext context) =>
            int.Parse(context.User.FindFirstValue("ID"));

        public static UserRole UserRole (this HttpContext context) =>
            context.User.HasClaim(ClaimTypes.Role, "Admin") ?
                Codepedia.UserRole.Admin :
                context.User.HasClaim(ClaimTypes.Role, "User") ?
                    Codepedia.UserRole.User :
                    Codepedia.UserRole.Anonymous;
    }

    public enum UserRole
    {
        Anonymous,
        User,
        Admin
    }
}
