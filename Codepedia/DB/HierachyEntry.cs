using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class HierachyEntry
    {
        public HierachyEntry()
        {
            HierachyParentNavigations = new HashSet<Hierachy>();
        }

        public int Id { get; set; }
        public int? EntryId { get; set; }
        public string Title { get; set; }

        public virtual WikiEntry Entry { get; set; }
        public virtual Hierachy HierachyChildNavigation { get; set; }
        public virtual ICollection<Hierachy> HierachyParentNavigations { get; set; }
    }
}
