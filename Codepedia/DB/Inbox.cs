using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class Inbox
    {
        public int Id { get; set; }
        public int User { get; set; }
        public string Text { get; set; }
        public int? ReputationChange { get; set; }
        public bool ReadByUser { get; set; }
        public DateTime TimeCreated { get; set; }

        public virtual User UserNavigation { get; set; }
    }
}
