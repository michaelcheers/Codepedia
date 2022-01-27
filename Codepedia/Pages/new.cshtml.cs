using Codepedia.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading.Tasks;

namespace Codepedia.Pages
{
    public class snippetEditModel : PageModel
    {
        public snippetEditModel(CodepediaContext db) { DB = db; }

        public CodepediaContext DB;

        public WikiCommit commit;

        public void OnGet(string? slug)
        {
            if (slug is string entrySlug)
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

            const int userID = 1;
            //int? CommitID = commitID;
            //string entryName = entryName, slug = slug, markdown = markdown;
            using MutableDBTransaction trans = await connection.CreateTransaction();
            if (CommitID is not int commitID)
            {
                // Creating a new post
                if (/*User is Admin*/true)
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
                        ["userID"] = userID,
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
                        ["suggestedBy"] = userID,
                        ["markdown"] = markdown
                    }.Run1();
                    trans.Commit();
                }
            }
            else
            {
                // Editing a post
                if (/*User is Admin*/true)
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
                        ["userID"] = userID,
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
                        ["suggestedBy"] = userID
                    }.Run1();
                    trans.Commit();
                }
            }
            return Redirect($"/{slug}");
        }
    }
}
