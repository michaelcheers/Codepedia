using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class BlogPost
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BlogContent { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
