using Codepedia.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codepedia.Pages.admin
{
    [Authorize(Roles = "Admin")]
    public class suggestionsModel : PageModel
    {
        public suggestionsModel(CodepediaContext db) => DB = db;

        public readonly CodepediaContext DB;
        public List<(WikiSuggestion Suggestion, User SuggestedBy, WikiCommit Commit)> SuggestedEdits;
        public List<(WikiPostSuggestion Suggestion, User SuggestedBy)> SuggestedPosts;

        public async Task OnGetAsync()
        {
            SuggestedEdits = (
                from suggestion in DB.WikiSuggestions
                where suggestion.Status == "Unreviewed"
                select new
                {
                    Suggestion = suggestion,
                    SuggestedBy = suggestion.SuggestedByNavigation,
                    Commit = (suggestion.Commit.Entry.WikiCommits.OrderByDescending(c => c.TimeCreated).Last())
                }).AsEnumerable().Select(t => (t.Suggestion, t.SuggestedBy, t.Commit)).ToList();

            SuggestedPosts = (
                from post in DB.WikiPostSuggestions
                where post.Status == "Unreviewed"
                select new
                {
                    Suggestion = post,
                    SuggestedBy = post.SuggestedByNavigation
                }).AsEnumerable().Select(t => (t.Suggestion, t.SuggestedBy)).ToList();
        }
    }
}
