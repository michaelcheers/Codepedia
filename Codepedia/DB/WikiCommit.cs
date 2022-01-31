using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class WikiCommit
    {
        public WikiCommit()
        {
            WikiSuggestions = new HashSet<WikiSuggestion>();
        }

        public int Id { get; set; }
        public int EntryId { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Markdown { get; set; }
        public string Words { get; set; }
        public int SuggestedBy { get; set; }
        public int ApprovedBy { get; set; }
        public int? WikiPostSuggestionId { get; set; }
        public int? WikiSuggestionId { get; set; }
        public DateTime TimeCreated { get; set; }

        public virtual User ApprovedByNavigation { get; set; }
        public virtual WikiEntry Entry { get; set; }
        public virtual User SuggestedByNavigation { get; set; }
        public virtual WikiPostSuggestion WikiPostSuggestion { get; set; }
        public virtual WikiSuggestion WikiSuggestion { get; set; }
        public virtual ICollection<WikiSuggestion> WikiSuggestions { get; set; }
    }
}
