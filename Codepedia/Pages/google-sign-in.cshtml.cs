using Codepedia.DB;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codepedia.Pages
{
    public class google_sign_inModel : PageModel
    {
        public google_sign_inModel(CodepediaContext db) => DB = db;

        public readonly CodepediaContext DB;

        public string IdToken;

        public async Task<IActionResult> OnGetAsync (string id_token, string next = null)
        {
            next = next is "" or null ? "/" : next;
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(id_token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "129144355612-kd63kqitjapt8eudcs4dajp54vjp8plg.apps.googleusercontent.com" }
            });
            User user = DB.Users.FirstOrDefault(u => u.GoogleUserId == payload.Subject);
            if (user != null) return await this.SignUserInAsync(user, next, authType: "Google");
            IdToken = id_token;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id_token, string username = null, string next = null)
        {
            if (string.IsNullOrEmpty(id_token) || string.IsNullOrEmpty(username)) return BadRequest();
            if (!AccountModel.userNameRegex.IsMatch(username)) return BadRequest(AccountModel.badUsernameMessage);
            next = next is "" or null ? "/" : next;
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(id_token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { "129144355612-kd63kqitjapt8eudcs4dajp54vjp8plg.apps.googleusercontent.com" }
            });
            User user = DB.Users.FirstOrDefault(u => u.GoogleUserId == payload.Subject);
            if (user != null) return await this.SignUserInAsync(user, next, authType: "Google");
            using MutableDBConnection db = await MutableDBConnection.Create();
            using MutableDBTransaction trans = await db.CreateTransaction();
            try
            {
                new CommandCreator(trans, "INSERT INTO Users (Username, Email, GoogleUserID) VALUES (@username, @email, @googleUserID)")
                {
                    ["username"] = username,
                    ["email"] = payload.Email,
                    ["googleUserID"] = payload.Subject
                }.Run1();
            }
            catch (MySqlException)
            {
                return LocalRedirect(
                    "/google-sign-in" +
                    "?username-already-taken" +
                    "&id_token=" + Uri.EscapeUriString(id_token) +
                    (next != null ? ("&next=" + Uri.EscapeUriString(next)) : "")
                );
            }
            trans.Commit();
            return await this.OnGetAsync(id_token, next);
        }
    }
}
