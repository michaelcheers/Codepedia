using Codepedia.DB;
using CryptSharp;
using CryptSharp.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codepedia.Pages
{
    public static class AccountModel
    {
        public static readonly Regex userNameRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9]{5,19}$");
        public static readonly Regex passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$");
        public static readonly Regex emailRegex = new Regex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
        public const string badUsernameMessage = "Username must be between 6 and 20 characters, must start with a letter and must consist only of letters and numbers.";
        public const string badPasswordMessage = "Password be at least 8 characters in length and must contain at least one uppercase letter, one lowercase letter and one number.";
        public const string badEmailMessage = "Email address not compliant with email address standards.";
    }
    public class sign_inModel : PageModel
    {
        public sign_inModel(CodepediaContext db) => DB = db;

        public readonly CodepediaContext DB;

        public async Task<IActionResult> OnPostAsync (string username, string password, string email = null, string next = null)
        {
            next = next is "" or null ? "/" : next;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return BadRequest();
            if (!AccountModel.userNameRegex.IsMatch(username)) return BadRequest(AccountModel.badUsernameMessage);
            if (!AccountModel.passwordRegex.IsMatch(password)) return BadRequest(AccountModel.badPasswordMessage);
            if (!string.IsNullOrEmpty(email))
            {
                if (!AccountModel.emailRegex.IsMatch(email)) return BadRequest(AccountModel.badEmailMessage);

                using MutableDBConnection conn = await MutableDBConnection.Create();
                using MutableDBTransaction trans = await conn.CreateTransaction();
                try
                {
                    new CommandCreator(trans, "INSERT INTO Users (Username, Email, Password) VALUES (@username, @email, @password)")
                    {
                        ["username"] = username,
                        ["password"] = Crypter.Sha512.Crypt(password),
                        ["email"] = email
                    }.Run1();
                }
                catch (MySqlException)
                {
                    return LocalRedirect(
                        "/sign-in?username-already-taken" +
                        (next != null ? ("&next=" + Uri.EscapeUriString(next)) : "")
                    );
                }
                trans.Commit();
                return await OnPostAsync(username, password);
            }
            User user = DB.Users.SingleOrDefault(u => u.Username == username);
            if (user == null) return LocalRedirect("/sign-in?incorrect-username");

            if (!Crypter.CheckPassword(password, user.Password)) return LocalRedirect("/sign-in?incorrect-password");

            return await this.SignUserInAsync(user, next);
        }
    }
}
