using System;
using System.Collections.Generic;

#nullable disable

namespace Codepedia.DB
{
    public partial class User
    {
        public User()
        {
            CommitDrafts = new HashSet<CommitDraft>();
            EntryCommits = new HashSet<EntryCommit>();
            Inboxes = new HashSet<Inbox>();
            WikiSuggestionSuggestedByNavigations = new HashSet<WikiSuggestion>();
            WikiSuggestionUserRejectedNavigations = new HashSet<WikiSuggestion>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string GoogleUserId { get; set; }

        public virtual ICollection<CommitDraft> CommitDrafts { get; set; }
        public virtual ICollection<EntryCommit> EntryCommits { get; set; }
        public virtual ICollection<Inbox> Inboxes { get; set; }
        public virtual ICollection<WikiSuggestion> WikiSuggestionSuggestedByNavigations { get; set; }
        public virtual ICollection<WikiSuggestion> WikiSuggestionUserRejectedNavigations { get; set; }
    }
}
