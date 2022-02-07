using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class WikiSuggestion
    {
        public WikiSuggestion()
        {
            SuggestionCommits = new HashSet<SuggestionCommit>();
        }

        public int Id { get; set; }
        public int? EntryId { get; set; }
        public int SuggestedBy { get; set; }
        public string Status { get; set; }
        public int? MergingCommitId { get; set; }
        public int? ReputationAwarded { get; set; }
        public DateTime? TimeRejected { get; set; }
        public int? UserRejected { get; set; }

        public virtual WikiEntry Entry { get; set; }
        public virtual EntryCommit MergingCommit { get; set; }
        public virtual User SuggestedByNavigation { get; set; }
        public virtual User UserRejectedNavigation { get; set; }
        public virtual ICollection<SuggestionCommit> SuggestionCommits { get; set; }
    }
}
