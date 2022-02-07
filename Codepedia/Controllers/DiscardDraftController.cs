using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Codepedia.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    public class DiscardDraftController : Controller
    {
        public async Task<IActionResult> PostAsync (int draftID)
        {
            using MutableDBConnection conn = await MutableDBConnection.Create();
            using MutableDBTransaction trans = await conn.CreateTransaction();
            int deleted = new CommandCreator(trans, "DELETE FROM CommitDrafts WHERE ID=@draftID AND Owner=@owner")
            {
                ["draftID"] = draftID,
                ["owner"] = HttpContext.UserID()
            }.Run0OR1();
            if (deleted == 1)
            {
                trans.Commit();
                return Content("Draft deleted");
            }
            else
                return NotFound("Draft not deleted");
        }
    }
}
