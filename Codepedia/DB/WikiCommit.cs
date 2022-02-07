using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class WikiCommit
    {
        public WikiCommit()
        {
            CommitDrafts = new HashSet<CommitDraft>();
        }

        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Markdown { get; set; }
        public string Words { get; set; }
        public string Message { get; set; }
        public DateTime TimeCreated { get; set; }

        public virtual EntryCommit EntryCommit { get; set; }
        public virtual SuggestionCommit SuggestionCommit { get; set; }
        public virtual ICollection<CommitDraft> CommitDrafts { get; set; }
    }
}
