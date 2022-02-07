using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codepedia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        public async Task<List<SearchResult>> Get (string q)
        {
            q = SearchRegex.Replace(q);

            List<SearchResult> searchResults = new();

            MutableDBConnection conn = await MutableDBConnection.Create();
            foreach (MySqlDataReader reader in
                new CommandCreator(conn,
                    "SELECT a.*, a_.* FROM WikiCommits a" +
                    " JOIN EntryCommits a_ ON a_.CommitID=a.ID" +
                    " INNER JOIN (" +
                      "SELECT EntryID, MAX(TimeCreated) TimeCreated FROM WikiCommits" +
                      " JOIN EntryCommits ON EntryCommits.CommitID=WikiCommits.ID" +
                      " WHERE MATCH(Name, Markdown, Words) AGAINST(@q IN NATURAL LANGUAGE MODE)" +
                      " GROUP BY EntryID" +
                    ") b" +
                    " ON a_.EntryID = b.EntryID AND a.TimeCreated = b.TimeCreated"
                ) { ["q"] = q }.Run().Take(20)
                // Checks only commits that are the latest version of an entry and gives results from those
            )
            {
                string markdown = reader.GetString("Markdown");
                string preview = SimpleMarkdownInterpreter.ToHTMLSummary(markdown);
                searchResults.Add(new SearchResult
                {
                    Id = reader.GetInt32("EntryID"),
                    Slug = reader.GetString("Slug"),
                    Name = reader.GetString("Name"),
                    Markdown = markdown,
                    Preview = preview,
                    TimeModified = reader.GetUTCDateTime("TimeCreated")
                });
                // TODO: Add time created.
            }
            return searchResults;
        }
    }
}
