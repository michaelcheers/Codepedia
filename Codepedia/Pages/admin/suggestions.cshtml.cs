using Codepedia.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
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

        public List<SuggestionInfo> Suggestions;

        public void OnGet ()
        {
            Suggestions = Functions.SuggestionsInfo(DB.WikiSuggestions.Where(s => s.Status == "Unreviewed")).ToList();
        }
    }
}
