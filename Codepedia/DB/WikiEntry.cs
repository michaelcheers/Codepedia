using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class WikiEntry
    {
        public WikiEntry()
        {
            EntryCommits = new HashSet<EntryCommit>();
            HierachyEntries = new HashSet<HierachyEntry>();
            WikiSuggestions = new HashSet<WikiSuggestion>();
        }

        public int Id { get; set; }

        public virtual ICollection<EntryCommit> EntryCommits { get; set; }
        public virtual ICollection<HierachyEntry> HierachyEntries { get; set; }
        public virtual ICollection<WikiSuggestion> WikiSuggestions { get; set; }
    }
}
