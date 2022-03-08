using Codepedia.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Codepedia.Pages
{
    public partial class viewWikiModel : PageModel
    {
        public viewWikiModel (CodepediaContext db) { DB = db; }

        public CodepediaContext DB;
        public DateTime TimeCreated;
        public DateTime LastUpdated;
        public WikiCommit Commit;
        public List<FolderNode> TopNodes = new();

        public IActionResult OnGet(string slug)
        {
            slug = slug.Split('/').Last();
            var commitInfo = (
                from c in DB.WikiCommits
                where c.Slug == slug
                orderby c.TimeCreated descending
                let entriesCommit = c.EntryCommit
                where entriesCommit != null
                let entry = entriesCommit.Entry
                let firstCommit = entry.EntryCommits.OrderBy(c => c.TimeCommited).First()
                let lastCommit = entry.EntryCommits.OrderByDescending(c => c.TimeCommited).First()
                select new
                {
                    TimeCreated = firstCommit.TimeCommited,
                    LastUpdated = lastCommit.TimeCommited,
                    Commit = lastCommit.Commit,
                    EntryID = entry.Id
                }
            ).FirstOrDefault();
            if (commitInfo == null) return NotFound();
            if (slug != commitInfo.Commit.Slug) return RedirectPermanent($"/{commitInfo.Commit.Slug}");

            (TimeCreated, LastUpdated, Commit) = (commitInfo.TimeCreated, commitInfo.LastUpdated, commitInfo.Commit);

            TopNodes = FolderNode.FetchTopNodes(DB, commitInfo.EntryID);

            return Page();
        }
    }
}
