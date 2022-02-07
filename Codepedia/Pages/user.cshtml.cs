using Codepedia.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Codepedia.Pages
{
    public class userModel : PageModel
    {
        public userModel(CodepediaContext db) => DB = db;

        public readonly CodepediaContext DB;

        public User UserData;
        public List<SuggestionInfo> Suggestions;
        public int Reputation;
        public bool IsUserDataVisible;

        public void OnGet (string username)
        {
            UserData = DB.Users.First(u => u.Username == username);
            var suggestions = DB.WikiSuggestions.Where(s => s.SuggestedBy == UserData.Id);
            switch (HttpContext.UserRole())
            {
                case UserRole.Admin:
                    IsUserDataVisible = true;
                    break;
                case UserRole.User:
                    IsUserDataVisible = HttpContext.UserID() == UserData.Id;
                    break;
            }
            if (!IsUserDataVisible)
                suggestions = suggestions.Where(s => s.Status == "Accepted");
            Suggestions = Functions.SuggestionsInfo(suggestions).ToList();
            Reputation = (from s in Suggestions
                          let rep = s.Suggestion.ReputationAwarded
                          where rep.HasValue
                          select rep.Value).Sum();
        }
    }
}
