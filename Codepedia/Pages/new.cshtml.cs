using Codepedia.DB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Codepedia.Pages
{
    [Authorize]
    public class snippetEditModel : PageModel
    {
        public snippetEditModel(CodepediaContext db) { DB = db; }

        public CodepediaContext DB;

        // No merging required:
        // - the draft
        // - the suggestion commit the draft is based on
        // - the entry commit the suggestion commit is based on

        // Merging required:
        // - the latest version of the suggestion (maybe not a big worry because only one user can work on a suggestion)
        // - the latest version of the entry

        public struct CommitData
        {
            public int Id;
            public string Name;
            public string Slug;
            public string Markdown;
            public string Message;
            public DateTime TimeCreated;
            public DateTime? LastUpdated;
        }

        // Options:
        // - they are all null
        // - EntryData is not null
        // - EntryData and DraftData are not null
        // - SuggestionData, EntryData, are not null
        // - SuggestionData, EntryData and DraftData are not null

        public CommitData? DraftData;
        public CommitData? SuggestionData;
        public CommitData? EntryData;
        public CommitData? LatestSuggestionData;
        public CommitData? LatestEntryData;
        public CommitData? PreviousCommitData;
        public int? EntryID;

        public CommitData? UpdatedEntryData => EntryData ?? LatestEntryData;

        public CommitData? commit => DraftData ?? SuggestionData ?? EntryData;
        //public Dictionary<string, (CommitData bottom, CommitData top)> dic => new()
        //{
        //    []
        //};
        //public Dictionary<string, CommitData?> ComparisonPoints =>

        //public WikiCommit commit;
        //public WikiCommit compareTo;
        public WikiSuggestion? SuggestionInfo;
        public User? SuggestedBy;
        public User? UserRejected;
        public User? ApprovedBy;
        public EntryCommit? MergingCommit;
        public bool CommitBased;
        public List<(WikiSuggestion, DateTime TimeCreated)>? RelevantSuggestions;

        //public IQueryable<CommitDraft> FindDraftsOfSuggestion (int suggestionID)
        //    => from commitInfo in DB.SuggestionCommits
        //       where commitInfo.Suggestion.Id == suggestionID
        //       let commit = commitInfo.Commit
        //       from draft in DB.CommitDrafts
        //       where draft.BaseCommit == commit.Id && draft.Owner == HttpContext.UserID()
        //       select draft;

        //public IQueryable<CommitDraft> FindDraftsOfEntry (int entryID)
        //    => from commitInfo in DB.EntryCommits
        //       where commitInfo.Entry.Id == entryID
        //       let commit = commitInfo.Commit
        //       from draft in DB.CommitDrafts
        //       where draft.BaseCommit == commit.Id && draft.Owner == HttpContext.UserID()
        //       select draft;

        public static CommitData ToCommitData (CommitDraft draft) => new()
        {
            Id = draft.Id,
            Name = draft.Name,
            Slug = draft.Slug,
            Markdown = draft.Markdown,
            Message = draft.Message,
            TimeCreated = draft.TimeCreated,
            LastUpdated = draft.LastUpdated
        };

        public CommitData ToSuggestionCommitData(WikiCommit commit)
        {
            CommitData result = ToEntryCommitData(commit);
            result.Message = commit.Message;
            return result;
        }

        public CommitData ToEntryCommitData(WikiCommit commit)
        {
            CommitData result = new()
            {
                Id = commit.Id,
                Name = commit.Name,
                Slug = commit.Slug,
                Markdown = commit.Markdown,
                TimeCreated = commit.TimeCreated
            };
            if (CommitBased)
                result.Message = commit.Message;
            return result;
        }

        public IActionResult OnGet (string? slug, int? suggestion, int? commitID, int? draftID)
        {
            IQueryable<CommitDraft> myDrafts = DB.CommitDrafts.Where(d => d.Owner == HttpContext.UserID());
            IQueryable<WikiSuggestion> visibleSuggestions = DB.WikiSuggestions;
            if (HttpContext.UserRole() != UserRole.Admin)
                visibleSuggestions = visibleSuggestions.Where(d => d.SuggestedBy == HttpContext.UserID());
            IQueryable<SuggestionCommit> visibleSuggestionCommits = DB.SuggestionCommits;
            if (HttpContext.UserRole() != UserRole.Admin)
                visibleSuggestionCommits = visibleSuggestionCommits.Where(s => s.Suggestion.SuggestedBy == HttpContext.UserID());

            bool AddDraftData (int draftCommitID)
            {
                var draftInfo = myDrafts.FirstOrDefault(draft => draft.Id == draftCommitID);
                if (draftInfo == null) return false;
                DraftData = ToCommitData(draftInfo);
                if (draftInfo.BaseCommitId is int baseCommit)
                    AddCommitData(baseCommit);
                return true;
            }
            bool AddCommitData (int commitID)
            {
                var commitInfo = (from commit in DB.WikiCommits
                                  where commit.Id == commitID
                                  select new
                                  {
                                      isEntry = commit.EntryCommit != null,
                                      isSuggestion = commit.SuggestionCommit != null
                                  }
                                 ).FirstOrDefault();
                if (commitInfo == null) return false;
                if (commitInfo.isEntry)
                    return AddEntryData(commitID);
                else if (commitInfo.isSuggestion)
                    return AddSuggestionData(commitID);
                else throw new CodepediaException("Commit not part of an entry or a suggestion.");
            }
            bool AddSuggestionData (int suggestionCommitID)
            {
                var suggestionInfo = (from commit in visibleSuggestionCommits
                                      where commit.CommitId == suggestionCommitID
                                      let mergingCommit = commit.Suggestion.MergingCommit
                                      select new
                                      {
                                          commit.Commit,
                                          commit.BaseEntryCommitId,
                                          SuggestionInfo = commit.Suggestion,
                                          SuggestedBy = commit.Suggestion.SuggestedByNavigation,
                                          UserRejected = commit.Suggestion.UserRejectedNavigation,
                                          MergingCommit = mergingCommit,
                                          ApprovedBy = mergingCommit == null ? null : mergingCommit.ApprovedByNavigation,
                                          LatestSuggestionVersion = commit.Suggestion.SuggestionCommits.Select(c => c.Commit).OrderByDescending(c => c.TimeCreated).FirstOrDefault()
                                      }).FirstOrDefault();
                if (suggestionInfo == null) return false;
                SuggestionInfo = suggestionInfo.SuggestionInfo;
                SuggestedBy = suggestionInfo.SuggestedBy;
                UserRejected = suggestionInfo.UserRejected;
                MergingCommit = suggestionInfo.MergingCommit;
                ApprovedBy = suggestionInfo.ApprovedBy;
                SuggestionData = ToSuggestionCommitData(suggestionInfo.Commit);
                if (suggestionInfo.LatestSuggestionVersion.Id != suggestionInfo.Commit.Id)
                    LatestSuggestionData = ToSuggestionCommitData(suggestionInfo.LatestSuggestionVersion);
                if (suggestionInfo.BaseEntryCommitId is int baseEntryCommit)
                    AddEntryData(baseEntryCommit);
                return true;
            }
            bool AddEntryData (int entryCommitID)
            {
                var entryInfo = (from commit in DB.EntryCommits
                                 where commit.CommitId == entryCommitID
                                 select new
                                 {
                                     commit.Commit,
                                     commit.EntryId,
                                     LatestEntryVersion = commit.Entry.EntryCommits.Select(c => c.Commit).OrderByDescending(c => c.TimeCreated).FirstOrDefault()
                                 }).FirstOrDefault();
                if (entryInfo == null) return false;
                EntryID = entryInfo.EntryId;
                EntryData = ToEntryCommitData(entryInfo.Commit);
                if (entryInfo.LatestEntryVersion.Id != entryInfo.Commit.Id)
                    LatestEntryData = ToEntryCommitData(entryInfo.LatestEntryVersion);
                return true;
            }

            if (draftID is int draft)
            {
                // Draft specified in url.
                if (AddDraftData(draft))
                {
                    // Draft found and is valid.
                    return Page();
                }
            }

            if (suggestion is int sid)
            {
                var drafts = (from commitDraft in myDrafts
                              let baseCommitAsSuggestionCommit = commitDraft.BaseCommit.SuggestionCommit
                              where baseCommitAsSuggestionCommit != null && baseCommitAsSuggestionCommit.SuggestionId == sid
                              select (int?)commitDraft.Id
                );
                if (drafts.FirstOrDefault() is int d)
                {
                    AddDraftData(d);
                }
                else
                {
                    if (DB.WikiSuggestions.Where(s => s.Id == sid).Select(
                        s => s.SuggestionCommits.OrderByDescending(c => c.Commit.TimeCreated).FirstOrDefault()
                    ).FirstOrDefault() is SuggestionCommit suggestionCommit)
                    {
                        AddSuggestionData(suggestionCommit.CommitId);
                    }
                }

                // if (HttpContext.UserID() != suggestionInfo.SuggestedBy.Id && HttpContext.UserRole() != UserRole.Admin) return NotFound(); // || suggestion.IsPublic
                // if (suggestionInfo == null) return NotFound();
                // commit = suggestionInfo.Commit;
                // if (suggestionInfo.BaseCommit != null)
                // {
                //     // Get more information about the suggested edit.
                //     WikiCommit lastCommit =
                //         (from commit in DB.EntryCommits
                //          where commit != null && commit.EntryId == suggestionInfo.EntryID
                //          orderby commit.TimeCommited descending
                //          select commit).First().Commit;
                //     if (suggestionInfo.BaseCommit.Id != lastCommit.Id)
                //         commit = new WikiCommit
                //         {
                //             Name = (suggestionInfo.Commit.Name == suggestionInfo.BaseCommit.Name) ? lastCommit.Name :
                //                         (lastCommit.Name == suggestionInfo.BaseCommit.Name) ? suggestionInfo.Commit.Name :
                //                         $"{suggestionInfo.Commit.Name} (This Commit) <<<>>> {lastCommit.Name} (Last Commit)",
                //             Markdown = Diff3.CreateDiff(suggestionInfo.Commit.Markdown, suggestionInfo.BaseCommit.Markdown, lastCommit.Markdown).merged,
                //             Slug = (suggestionInfo.Commit.Slug == suggestionInfo.BaseCommit.Slug) ? lastCommit.Slug :
                //                         (lastCommit.Slug == suggestionInfo.BaseCommit.Slug) ? suggestionInfo.Commit.Slug :
                //                         $"{suggestionInfo.Commit.Slug} (This Commit) <<<>>> {lastCommit.Slug} (Last Commit)"
                //         };
                //     compareTo = lastCommit;
                // }
            }
            else if (commitID is int commitId)
            {
                CommitBased = true;
                if (AddEntryData(commitId))
                {
                    WikiCommit? previousCommit = (from commit in DB.EntryCommits
                                                  where commit.EntryId == EntryID!.Value && commit.TimeCommited < EntryData!.Value.TimeCreated
                                                  orderby commit.TimeCommited descending
                                                  select commit.Commit).FirstOrDefault();
                    if (previousCommit == null) return Page();
                    PreviousCommitData = ToEntryCommitData(previousCommit);
                }
            }
            else if (slug is string entrySlug)
            {
                var commitInfo = (
                    from c in DB.WikiCommits
                    where c.Slug == entrySlug
                    orderby c.TimeCreated descending
                    let entriesCommit = c.EntryCommit
                    where entriesCommit != null
                    let Entry = entriesCommit.Entry
                    let Commit = Entry.EntryCommits.OrderByDescending(c => c.TimeCommited).First().Commit
                    select new { Entry, Commit }
                ).FirstOrDefault();

                if (commitInfo == null) return NotFound("Referenced page not found!");

                RelevantSuggestions = (from s in visibleSuggestions
                                       where s.EntryId == commitInfo.Entry.Id && s.Status == "Unreviewed"
                                       select new
                                       {
                                           Suggestion = s,
                                           TimeCreated = s.SuggestionCommits.OrderBy(s => s.Commit.TimeCreated).First().Commit.TimeCreated
                                       }).AsEnumerable().Select(s => (s.Suggestion, s.TimeCreated)).ToList();

                var drafts = (from commitDraft in myDrafts
                              let baseCommitAsEntryCommit = commitDraft.BaseCommit.EntryCommit
                              where baseCommitAsEntryCommit != null && baseCommitAsEntryCommit.EntryId == commitInfo.Entry.Id
                              select (int?)commitDraft.Id
                );

                if (drafts.FirstOrDefault() is int d)
                {
                    AddDraftData(d);
                }
                else
                {
                    AddEntryData(commitInfo.Commit.Id);
                }
            }
            else
            {
                if (myDrafts.FirstOrDefault(d => d.BaseCommitId == null) is CommitDraft d)
                    AddDraftData(d.Id);
            }
            return Page();
        }

        public class SnippetModel
        {
            public int? commitID;
            public string entryName, slug, markdown;
        }


        public async Task<IActionResult> OnPostAsync (int? baseEntryCommitID, int? baseSuggestionCommitID, int? draft, int? reputationAwarded, bool insertingForReals, string entryName, string slug, string markdown, string? message)
        {
            using MutableDBConnection connection = await MutableDBConnection.Create();
            using MutableDBTransaction trans = await connection.CreateTransaction();

            int? baseCommitID = baseSuggestionCommitID ?? baseEntryCommitID;

            //int? CommitID = commitID;
            //string entryName = entryName, slug = slug, markdown = markdown;

            if (!insertingForReals)
            {
                // This is a draft saving or updating request, not an update to a wiki entry or suggestion.

                if (draft is int draftID)
                {
                    // Editing an existing draft with draft ID {draftID}.

                    if (!DB.CommitDrafts.Any(d => d.Id == draftID && d.Owner == HttpContext.UserID()))
                        return NotFound("Tried to update draft but draft does not exist or could not be accessed by the signed in user." +
                            " This may indicate that the draft was already commited.");

                    new CommandCreator(trans, "UPDATE CommitDrafts SET Name=@name, Slug=@slug, Markdown=@markdown, Message=@message, LastUpdated=CURRENT_TIMESTAMP" +
                        " WHERE ID=@draftID AND Owner=@owner")
                    {
                        ["name"] = entryName,
                        ["slug"] = slug,
                        ["markdown"] = markdown,
                        ["message"] = message,
                        ["draftID"] = draftID,
                        ["owner"] = HttpContext.UserID()
                    }.Run0OR1();
                    trans.Commit();
                    return Content("Draft Saved");
                }
                else
                {
                    // Creating a new draft ({BaseCommitID})
                    // If BaseCommitID is null, the draft is for a new post.

                    // Maybe I should check that the user has access to the commit they are creating a draft for.
                    // Nevertheless, this is not exactly a security risk.

                    int commitDraftID = new CommandCreator(trans, "INSERT INTO CommitDrafts (Owner, BaseCommitID, Name, Slug, Markdown, Message)" +
                        " VALUES(@owner, @baseCommit, @name, @slug, @markdown, @message)")
                    {
                        ["owner"] = HttpContext.UserID(),
                        ["baseCommit"] = baseCommitID,
                        ["name"] = entryName,
                        ["slug"] = slug,
                        ["markdown"] = markdown,
                        ["message"] = message
                    }.RunID();
                    trans.Commit();
                    return Content($"Draft Saved; ID={commitDraftID}");
                }
            }
            else if (draft is int draftID)
            {
                // Deleteing the draft before commiting/suggesting to a wiki entry/suggestion.
                // Thus, if the operation is successful, the draft can safely be deleted.

                int deletedRows = new CommandCreator(trans, "DELETE FROM CommitDrafts WHERE ID=@id AND Owner=@owner")
                {
                    ["id"] = draftID,
                    ["owner"] = HttpContext.UserID()
                }.Run0OR1();
                if (deletedRows == 0) throw new CodepediaException("Tried to delete draft but draft was not found. This may indicate that the draft was already commited.");
            }

            int commitID = new CommandCreator(trans, "INSERT INTO WikiCommits (Slug, Name, Markdown, Message)" +
                " VALUES (@slug, @name, @markdown, @message)")
            {
                ["slug"] = slug,
                ["name"] = entryName,
                ["markdown"] = markdown,
                ["message"] = message
            }.RunID();

            if (baseCommitID is not int baseCommit)
            {
                if (HttpContext.UserRole() == UserRole.Admin)
                {
                    // Creating a new post

                    int entryID = new CommandCreator(trans, "INSERT INTO WikiEntries () VALUES ();").RunID();
                    new CommandCreator(
                        trans,
                        "INSERT INTO EntryCommits (EntryID, CommitID, ApprovedBy)" +
                        " VALUES (@entryID, @commitID, @approvedBy)"
                    )
                    {
                        ["entryID"] = entryID,
                        ["commitID"] = commitID,
                        ["approvedBy"] = HttpContext.UserID()
                    }.Run1();
                    trans.Commit();
                }
                else
                {
                    // Suggesting a new post

                    int suggestionID = new CommandCreator(trans, "INSERT INTO WikiSuggestions (SuggestedBy) VALUES (@suggestedBy);")
                    {
                        ["suggestedBy"] = HttpContext.UserID()
                    }.RunID();

                    new CommandCreator(trans, "INSERT INTO SuggestionCommits (SuggestionID, CommitID) VALUES (@suggestionID, @commitID)")
                    {
                        ["suggestionID"] = suggestionID,
                        ["commitID"] = commitID,
                    }.Run1();

                    trans.Commit();
                    return LocalRedirect($"/suggestions/{suggestionID}");
                }
            }
            else
            {
                // Making or approving a suggestion or editing a post or suggestion

                int entryID;
                EntryCommit? baseEntryCommit = null;
                
                // Find or create the relevant post
                if (baseEntryCommitID is not int entryCommitID)
                {
                    entryID = new CommandCreator(trans, "INSERT INTO WikiEntries () VALUES ();").RunID();
                }
                else
                {
                    if (DB.EntryCommits.FirstOrDefault(e => e.CommitId == entryCommitID) is not EntryCommit ec)
                        return NotFound($"Entry Commit {entryCommitID} not found.");
                    baseEntryCommit = ec;
                    entryID = ec.EntryId;
                }

                WikiSuggestion? suggestion = null;

                // Find the relevant suggestion
                if (baseSuggestionCommitID is int suggestionCommit)
                {
                    suggestion = (from s in DB.SuggestionCommits
                                  where s.CommitId == suggestionCommit
                                  select s.Suggestion
                                 ).FirstOrDefault();
                    if (suggestion == null)
                        return NotFound($"Suggestion Commit {suggestionCommit} not found.");
                }

                if (HttpContext.UserRole() == UserRole.Admin)
                {
                    // Add the commit to the post.

                    try
                    {
                        CommandCreator command = new CommandCreator(trans, "INSERT INTO EntryCommits (EntryID, CommitID, ApprovedBy)" +
                            string.Format(
                                baseEntryCommit != null ?
                                (" SELECT {0} as EntryID, {1} as CommitID, {2} as ApprovedBy FROM EntryCommits" +
                                 // We must check that no more commits were made to the post because this would result in a merge conflict.
                                 " WHERE (EntryID={0} AND TimeCommited>@baseCommitTimeCommited) HAVING COUNT(*)=0"
                                ) :
                                " VALUES ({0}, {1}, {2})", "@entryID", "@commitID", "@approvedBy"
                            )
                        )
                        {
                            ["entryID"] = entryID,
                            ["commitID"] = commitID,
                            ["approvedBy"] = HttpContext.UserID(),
                        };
                        if (baseEntryCommit != null)
                            command["baseCommitTimeCommited"] = baseEntryCommit.TimeCommited;
                        command.Run1();
                    }
                    catch (MySqlException)
                    {
                        // There is a merge conflict. We will redirect the user to the edit page to correct this.
                        return LocalRedirect(Request.Path);
                    }

                    if (suggestion != null)
                    {
                        // Add the details of the user's suggestion.

                        if (reputationAwarded is not int rep)
                            return BadRequest("A suggestor was specified but the amount of reputation to award was not specified!");

                        if (!suggestion.CanEdit())
                            throw new CodepediaException("The suggestion linked to this edit is locked for editing.");

                        // We mark the suggestion as accepted and reward reputation to the suggestor.
                        // TODO: Send the suggestor an email notifying them.

                        int suggestionsUpdated = new CommandCreator(trans,
                            "UPDATE WikiSuggestions SET Status='Accepted', MergingCommitID=@mergingCommit, ReputationAwarded=@reputationAwarded" +
                            " WHERE ID=@suggestionID AND Status='Unreviewed' AND MergingCommitID IS NULL AND ReputationAwarded IS NULL")
                        {
                            ["mergingCommit"] = commitID,
                            ["reputationAwarded"] = rep,
                            ["suggestionID"] = suggestion.Id
                        }.Run0OR1();
                        if (suggestionsUpdated == 0) throw new CodepediaException("Updating suggestion failed.");
                    }

                    trans.Commit();
                }
                else
                {
                    // Creating or editing a suggestion

                    int suggestionID;
                    if (suggestion != null)
                    {
                        // Editing a suggestion

                        if (DB.WikiSuggestions.FirstOrDefault(s => s.Id == suggestion.Id && s.SuggestedBy == HttpContext.UserID()) is not WikiSuggestion suggested)
                            throw new CodepediaException("The suggestion you are trying to edit either does not exist or is not owned by you.");

                        if (!suggested.CanEdit()) throw new CodepediaException("The suggestion you are trying to edit is locked for editing.");

                        suggestionID = suggested.Id;
                    }
                    else if (baseEntryCommit != null)
                    {
                        // Creating a new suggestion

                        suggestionID = new CommandCreator(trans, "INSERT INTO WikiSuggestions (EntryID, SuggestedBy) VALUES (@entryID, @suggestedBy);")
                        {
                            ["entryID"] = baseEntryCommit.EntryId,
                            ["suggestedBy"] = HttpContext.UserID()
                        }.RunID();
                    }
                    else
                        throw new CodepediaException("No suggestion and no base entry commit!");
                    new CommandCreator(trans, "INSERT INTO SuggestionCommits (SuggestionID, CommitID, BaseEntryCommitID)" +
                        " VALUES (@suggestionID, @commitID, @baseEntryCommitID)"
                    )
                    {
                        ["suggestionID"] = suggestionID,
                        ["commitID"] = commitID,
                        ["baseEntryCommitID"] = baseEntryCommit?.CommitId
                    }.Run1();
                    trans.Commit();
                    return LocalRedirect($"/suggestions/{suggestionID}");
                }
            }
            return LocalRedirect($"/{slug}");
        }
    }
}
