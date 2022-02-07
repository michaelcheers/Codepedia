using Codepedia.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace Codepedia.Pages
{
    public class allModel : PageModel
    {
        public allModel(CodepediaContext db) { DB = db; }

        public CodepediaContext DB;
        public List<(WikiEntry, WikiCommit)> Entries;

        public void OnGet()
        {
            var entries = (
                from commit in DB.WikiCommits
                orderby commit.TimeCreated descending
                let entry = commit.EntryCommit
                where entry != null
                select new { commit, entry.Entry });
            Entries = entries.AsEnumerable().DistinctBy(o => o.Entry).Select(e => (e.Entry, e.commit)).ToList();
        }
    }
}
