using Codepedia.DB;
using Markdig;
using Markdown.ColorCode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Codepedia
{
    public static class Functions
    {
        static MarkdownPipeline CreateMarkdownPipeline (bool colorCode)
        {
            MarkdownPipelineBuilder result = new();
            result.UseAdvancedExtensions();
            if (colorCode)
                result.UseColorCode();
            return result.Build();
        }
            
        public static string DisplayMarkdown (string markdown, bool colorCode = false)
        {
            return "<div style=\"tab-size:4\">" +
                Markdig.Markdown.ToHtml(markdown, CreateMarkdownPipeline(colorCode)) +
                "</div>";
        }

        public static async Task<IActionResult> SignUserInAsync(this PageModel pageModel, User user, string redirectTo, string authType = "password")
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("ID", user.Id.ToString()),
                new Claim("Username", user.Username),
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

#nullable enable
        public static IEnumerable<SuggestionInfo> SuggestionsInfo (IQueryable<WikiSuggestion> suggestions)
        {
            return (from suggestion in suggestions
                    let Entry = suggestion.Entry
                    let FirstCommitInfo = suggestion.SuggestionCommits.OrderBy(c => c.Commit.TimeCreated).First()
                    let LastCommitInfo = suggestion.SuggestionCommits.OrderByDescending(c => c.Commit.TimeCreated).First()
                    let MergingCommit = suggestion.MergingCommit
                    select new
                    {
                        Suggestion = suggestion,
                        SuggestedBy = suggestion.SuggestedByNavigation,
                    
                        CommitsBehind = Entry == null ? null : (int?)
                            Entry.EntryCommits.Count(c => c.TimeCommited >
                            (MergingCommit != null ? MergingCommit.TimeCommited : LastCommitInfo.BaseEntryCommit.TimeCommited)
                        ),
                    
                        EntryEditing_LatestVersion = Entry == null ? null :
                            Entry.EntryCommits.OrderByDescending(c => c.TimeCommited).First().Commit, // Last Commit on the entry
                    
                        TimeCreated = FirstCommitInfo.Commit.TimeCreated,
                        LastUpdated = MergingCommit != null ? MergingCommit.Commit.TimeCreated : LastCommitInfo.Commit.TimeCreated,
                        Commit = LastCommitInfo.Commit,
                        NumberOfCommits = suggestion.SuggestionCommits.Count,
                        MergingCommit = MergingCommit
                    }).AsEnumerable().Select(t => new SuggestionInfo(
                        t.Suggestion, t.SuggestedBy, t.CommitsBehind, t.EntryEditing_LatestVersion, t.TimeCreated, t.LastUpdated, t.Commit, t.NumberOfCommits, t.MergingCommit
                    ));
        }
    }

    public record SuggestionInfo (WikiSuggestion Suggestion, User SuggestedBy, int? CommitsBehind, WikiCommit? EntryEditing_LatestVersion, DateTime TimeCreated, DateTime LastUpdated, WikiCommit Commit, int NumberOfCommits, EntryCommit? MergingCommit);
#nullable disable
}
