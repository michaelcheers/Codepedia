using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class Hierachy
    {
        public int Parent { get; set; }
        public int Child { get; set; }
        public int Idx { get; set; }

        public virtual HierachyEntry ChildNavigation { get; set; }
        public virtual HierachyEntry ParentNavigation { get; set; }
    }
}
