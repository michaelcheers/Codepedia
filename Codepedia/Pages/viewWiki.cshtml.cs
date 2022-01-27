using Codepedia.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace Codepedia.Pages
{
    public class viewWikiModel : PageModel
    {
        public viewWikiModel (CodepediaContext db) { DB = db; }

        public CodepediaContext DB;
        public WikiCommit Commit;

        public IActionResult OnGet(string slug)
        {
            var commit = (
                from c in DB.WikiCommits
                where c.Slug == slug
                orderby c.TimeCreated descending
                select c.Entry.WikiCommits.OrderByDescending(c => c.TimeCreated).FirstOrDefault()
            ).FirstOrDefault();
            if (commit == null) return NotFound();
            Commit = commit;
            return Page();
        }
    }
}
