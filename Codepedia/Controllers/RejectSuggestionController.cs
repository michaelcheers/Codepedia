using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Codepedia.Controllers
{
    public class RejectSuggestionController : Controller
    {
        [Route("/rejectSuggestion/{suggestionID:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index (int suggestionID)
        {
            using MutableDBConnection db = await MutableDBConnection.Create();
            using MutableDBTransaction trans = await db.CreateTransaction();
            new CommandCreator(trans, "UPDATE WikiSuggestions SET Status='Rejected' WHERE Status='Unreviewed' AND ID=@id")
            {
                ["id"] = suggestionID
            }.Run1();
            trans.Commit();
            return Redirect("/admin/suggestions");
        }
    }
}
