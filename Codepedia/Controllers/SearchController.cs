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
                    "SELECT a.* FROM WikiCommits a INNER JOIN (" +
                      "SELECT EntryID, MAX(TimeCreated) TimeCreated FROM WikiCommits" +
                      " WHERE MATCH(Name, Markdown, Words) AGAINST(@q IN NATURAL LANGUAGE MODE)" +
                      " GROUP BY EntryID" +
                    ") b" +
                    " ON a.EntryID = b.EntryID AND a.TimeCreated = b.TimeCreated"
                ) { ["q"] = q }.Run()
                // Search latest commits for specified text
                // Two possible methods:
                // - Find commits that match the search -> check if they are latest after either during a query or in a separate query
                // - Check latest commits for ones that match the search

                // Method 1 -> SELECT EntryID, TimeCreated FROM WikiCommits
                //  WHERE MATCH(Markdown, Words) AGAINST (@search IN NATURAL LANGUAGE MODE)
                //  AND 
            )
            {
                searchResults.Add(new SearchResult
                {
                    Id = reader.GetInt32("EntryID"),
                    Name = reader.GetString("Name"),
                    Markdown = reader.GetString("Markdown"),
                    TimeModified = reader.GetUTCDateTime("TimeCreated")
                });
                // TODO: Add time created.
            }
            return searchResults;
        }
    }
}
