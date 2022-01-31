using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class User
    {
        public User()
        {
            WikiCommitApprovedByNavigations = new HashSet<WikiCommit>();
            WikiCommitSuggestedByNavigations = new HashSet<WikiCommit>();
            WikiPostSuggestions = new HashSet<WikiPostSuggestion>();
            WikiSuggestions = new HashSet<WikiSuggestion>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string GoogleUserId { get; set; }

        public virtual ICollection<WikiCommit> WikiCommitApprovedByNavigations { get; set; }
        public virtual ICollection<WikiCommit> WikiCommitSuggestedByNavigations { get; set; }
        public virtual ICollection<WikiPostSuggestion> WikiPostSuggestions { get; set; }
        public virtual ICollection<WikiSuggestion> WikiSuggestions { get; set; }
    }
}
