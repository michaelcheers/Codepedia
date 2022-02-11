using Codepedia.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace Codepedia.Pages
{
    public class blogModel : PageModel
    {
        public blogModel(CodepediaContext db) { DB = db; }

        public CodepediaContext DB;
        public List<BlogPost> BlogPosts;

        public void OnGet()
        {
            BlogPosts = DB.BlogPosts.OrderByDescending(b => b.TimeCreated).ToList();
        }
    }
}
