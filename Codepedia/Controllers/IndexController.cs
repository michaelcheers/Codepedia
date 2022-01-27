using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Codepedia.Controllers
{
    public class IndexController : Controller
    {
        [Route("/google-sign-in")]
        public async Task<IActionResult> GoogleSignIn(string id_token, string redirectTo = null)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(id_token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "129144355612-kd63kqitjapt8eudcs4dajp54vjp8plg.apps.googleusercontent.com" }
            });
            using MutableDBConnection connection = await MutableDBConnection.Create();
            using MutableDBTransaction trans = await connection.CreateTransaction();
            new CommandCreator(trans, "INSERT INTO Users (Username, GoogleUserID) VALUES (@username, @googleUserID)")
            {
                ["username"] = "",
                ["googleUserID"] = payload.JwtId
            }.Run1();
            return Redirect(string.IsNullOrEmpty(redirectTo) ? "/" : redirectTo);
        }
    }
}
