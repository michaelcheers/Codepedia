using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class SuggestionCommit
    {
        public int SuggestionId { get; set; }
        public int CommitId { get; set; }
        public int? BaseEntryCommitId { get; set; }

        public virtual EntryCommit BaseEntryCommit { get; set; }
        public virtual WikiCommit Commit { get; set; }
        public virtual WikiSuggestion Suggestion { get; set; }
    }
}
