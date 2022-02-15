using Codepedia.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System;

namespace Codepedia.Pages
{
    public class allModel : PageModel
    {
        public allModel(CodepediaContext db) { DB = db; }

        public CodepediaContext DB;
        public List<(WikiEntry, WikiCommit, DateTime timeCreated)> Entries;

        public void OnGet()
        {
            var commits = (
                from commit in DB.WikiCommits
                orderby commit.TimeCreated descending
                let entry = commit.EntryCommit
                where entry != null
                select new { commit, entry.Entry });
            Entries = (from entry in commits.AsEnumerable().GroupBy(o => o.Entry)
                       let commit = entry.MaxBy(commit => commit.commit.TimeCreated).commit
                       let timeCreated = entry.Min(commit => commit.commit.TimeCreated)
                       orderby timeCreated descending
                       select (entry.Key, commit, timeCreated)
                      ).ToList();
        }
    }
}
