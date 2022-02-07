using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class EntryCommit
    {
        public EntryCommit()
        {
            SuggestionCommits = new HashSet<SuggestionCommit>();
            WikiSuggestions = new HashSet<WikiSuggestion>();
        }

        public int EntryId { get; set; }
        public int CommitId { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime TimeCommited { get; set; }

        public virtual User ApprovedByNavigation { get; set; }
        public virtual WikiCommit Commit { get; set; }
        public virtual WikiEntry Entry { get; set; }
        public virtual ICollection<SuggestionCommit> SuggestionCommits { get; set; }
        public virtual ICollection<WikiSuggestion> WikiSuggestions { get; set; }
    }
}
