using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Codepedia.Pages
{
    public class sign_outModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync (string next = null)
        {
            if (HttpContext.User.HasClaim(ClaimTypes.AuthenticationMethod, "Google")) return Page();
            return await OnPostAsync(next);
        }
        public async Task<IActionResult> OnPostAsync (string next = null)
        {
            next = next is "" or null ? "/" : next;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect(next);
        }
    }
}
