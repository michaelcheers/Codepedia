using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class WikiEntry
    {
        public WikiEntry()
        {
            WikiCommits = new HashSet<WikiCommit>();
        }

        public int Id { get; set; }

        public virtual ICollection<WikiCommit> WikiCommits { get; set; }
    }
}
