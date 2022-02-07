using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class CommitDraft
    {
        public int Id { get; set; }
        public int Owner { get; set; }
        public int? BaseCommitId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Markdown { get; set; }
        public string Message { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual WikiCommit BaseCommit { get; set; }
        public virtual User OwnerNavigation { get; set; }
    }
}
