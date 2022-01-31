using Codepedia.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading.Tasks;

namespace Codepedia.Pages
{
    [Authorize]
    public class snippetEditModel : PageModel
    {
        public snippetEditModel(CodepediaContext db) { DB = db; }

        public CodepediaContext DB;

        public WikiCommit commit;
        public WikiCommit compareTo;
        public User SuggestedBy;

        public void OnGet (string slug = null, int? suggestedEdit = null, int? suggestedPost = null)
        {
            if (suggestedEdit is int suggestedEditID)
            {
                var t = DB.WikiSuggestions.Where(s => s.Id == suggestedEditID).Select(s => new
                {
                    Suggestion = s,
                    SuggestedBy = s.SuggestedByNavigation
                }).First();

                WikiSuggestion thisCommit = t.Suggestion;
                SuggestedBy = t.SuggestedBy;

                if (!(HttpContext.UserID() == thisCommit.SuggestedBy || HttpContext.UserRole() == UserRole.Admin)) return;

                var fields = DB.WikiCommits.Where(c => c.Id == thisCommit.CommitId).Select(c => new
                {
                    BaseCommit = c,
                    LastCommit = c.Entry.WikiCommits.OrderByDescending(c => c.TimeCreated).First()
                }).First();

                (WikiCommit baseCommit, WikiCommit lastCommit) = (fields.BaseCommit, fields.LastCommit);

                if (baseCommit.Id != lastCommit.Id)
                    commit = new WikiCommit
                    {
                        Name = thisCommit.Name,
                        Markdown = thisCommit.Markdown,
                        Slug = thisCommit.Markdown
                    };
                else
                    commit = new WikiCommit
                    {
                        Name = (thisCommit.Name == baseCommit.Name) ? lastCommit.Name :
                                   (lastCommit.Name == baseCommit.Name) ? thisCommit.Name :
                                   $"{thisCommit.Name} (This Commit) <<<>>> {lastCommit.Name} (Last Commit)",
                        Markdown = Diff3.CreateDiff(thisCommit.Markdown, baseCommit.Markdown, lastCommit.Markdown).merged,
                        Slug = (thisCommit.Slug == baseCommit.Slug) ? lastCommit.Slug :
                                   (lastCommit.Slug == baseCommit.Slug) ? thisCommit.Slug :
                                   $"{thisCommit.Slug} (This Commit) <<<>>> {lastCommit.Slug} (Last Commit)"
                    };
                compareTo = lastCommit;
            }
            else if (suggestedPost is int suggestedPostID)
            {
                var t =
                    DB.WikiPostSuggestions.Where(p => p.Id == suggestedPostID).Select(p => new
                    {
                        Suggestion = p,
                        SuggestedBy = p.SuggestedByNavigation
                    }).First();
                SuggestedBy = t.SuggestedBy;
                commit = new WikiCommit
                {
                    Name = t.Suggestion.Name,
                    Markdown = t.Suggestion.Markdown,
                    Slug = t.Suggestion.Slug
                };
            }
            else if (slug is string entrySlug)
            {
                int entryID = DB.WikiCommits.Where(c => c.Slug == entrySlug).OrderByDescending(c => c.TimeCreated).First().EntryId;
                commit = DB.WikiCommits.Where(c => c.EntryId == entryID).OrderByDescending(c => c.TimeCreated).First();
            }
        }

        public class SnippetModel
        {
            public int? commitID;
            public string entryName, slug, markdown;
        }


        public async Task<IActionResult> OnPostAsync (int? CommitID, string entryName, string slug, string markdown)
        {
            using MutableDBConnection connection = await MutableDBConnection.Create();

            //int? CommitID = commitID;
            //string entryName = entryName, slug = slug, markdown = markdown;
            using MutableDBTransaction trans = await connection.CreateTransaction();
            if (CommitID is not int commitID)
            {
                // Creating a new post
                if (HttpContext.UserRole() == UserRole.Admin)
                {
                    WikiEntry entry = new();
                    int entryID = new CommandCreator(trans, "INSERT INTO WikiEntries () VALUES ();").RunID();
                    new CommandCreator(
                        trans,
                        "INSERT INTO WikiCommits (Name, Slug, SuggestedBy, ApprovedBy, EntryID, Markdown)" +
                        " VALUES (@name, @slug, @userID, @userID, @entryID, @markdown)"
                    )
                    {
                        ["name"] = entryName,
                        ["slug"] = slug,
                        ["userID"] = HttpContext.UserID(),
                        ["entryID"] = entryID,
                        ["markdown"] = markdown
                    }.Run1();
                    trans.Commit();
                }
                else
                {
                    new CommandCreator(
                        trans,
                        "INSERT INTO WikiPostSuggestions (Name, Slug, SuggestedBy, Markdown) VALUES (@name, @slug, @suggestedBy, @markdown)"
                    )
                    {
                        ["name"] = entryName,
                        ["slug"] = slug,
                        ["suggestedBy"] = HttpContext.UserID(),
                        ["markdown"] = markdown
                    }.Run1();
                    trans.Commit();
                }
            }
            else
            {
                // Editing a post
                if (HttpContext.UserRole() == UserRole.Admin)
                {
                    int entryID = DB.WikiCommits.Single(c => c.Id == commitID).EntryId;
                    if (DB.WikiCommits.Where(e => e.EntryId == entryID).OrderByDescending(c => c.TimeCreated).First().Id != commitID)
                    {
                        // Not the last commit!
                        return Redirect("");
                    }

                    new CommandCreator(
                        trans,
                        "INSERT INTO WikiCommits (Name, Slug, SuggestedBy, ApprovedBy, EntryID, Markdown)" +
                        " VALUES (@name, @slug, @userID, @userID, @entryID, @markdown)"
                    )
                    {
                        ["name"] = entryName,
                        ["slug"] = slug,
                        ["userID"] = HttpContext.UserID(),
                        ["entryID"] = entryID,
                        ["markdown"] = markdown
                    }.Run1();
                    trans.Commit();
                }
                else
                {
                    new CommandCreator(
                        trans,
                        "INSERT INTO WikiSuggestions (CommitID, Slug, Name, Markdown, SuggestedBy)" +
                        " VALUES (@commitID, @slug, @name, @markdown, @suggestedBy)")
                    {
                        ["commitID"] = commitID,
                        ["slug"] = slug,
                        ["name"] = entryName,
                        ["markdown"] = markdown,
                        ["suggestedBy"] = HttpContext.UserID()
                    }.Run1();
                    trans.Commit();
                }
            }
            return Redirect($"/{slug}");
        }
    }
}
