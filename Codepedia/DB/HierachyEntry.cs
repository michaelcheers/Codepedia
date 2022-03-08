using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class HierachyEntry
    {
        public HierachyEntry()
        {
            ChildEntryParentNavigations = new HashSet<ChildEntry>();
        }

        public int Id { get; set; }
        public int? EntryId { get; set; }
        public string Title { get; set; }

        public virtual WikiEntry Entry { get; set; }
        public virtual ChildEntry ChildEntryChildNavigation { get; set; }
        public virtual ICollection<ChildEntry> ChildEntryParentNavigations { get; set; }
    }
}
