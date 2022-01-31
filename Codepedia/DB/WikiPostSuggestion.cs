using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class WikiPostSuggestion
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Markdown { get; set; }
        public int SuggestedBy { get; set; }
        public DateTime TimeCreated { get; set; }
        public string Status { get; set; }

        public virtual User SuggestedByNavigation { get; set; }
        public virtual WikiCommit WikiCommit { get; set; }
    }
}
