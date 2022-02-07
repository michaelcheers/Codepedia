using Codepedia.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace Codepedia.Pages
{
    public class viewBlogPostModel : PageModel
    {
        public readonly CodepediaContext DB;
        public viewBlogPostModel (CodepediaContext db) => DB = db;
        public BlogPost Post;
        public void OnGet (int id)
        {
            Post = DB.BlogPosts.FirstOrDefault(post => post.Id == id);
        }
    }
}
