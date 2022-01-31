using Codepedia.DB;
using CryptSharp;
using CryptSharp.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Codepedia.Pages
{
    public class sign_inModel : PageModel
    {
        public sign_inModel(CodepediaContext db) => DB = db;

        public readonly CodepediaContext DB;

        public async Task<IActionResult> OnPostAsync (string username, string password, string email = null, string next = null)
        {
            next = next is "" or null ? "/" : next;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return BadRequest();
            if (!string.IsNullOrEmpty(email))
            {
                using MutableDBConnection conn = await MutableDBConnection.Create();
                using MutableDBTransaction trans = await conn.CreateTransaction();
                new CommandCreator(trans, "INSERT INTO Users (Username, Email, Password) VALUES (@username, @email, @password)")
                {
                    ["username"] = username,
                    ["password"] = Crypter.Sha512.Crypt(password),
                    ["email"] = email
                }.Run1();
                trans.Commit();
                return await OnPostAsync(username, password);
            }
            User user = DB.Users.SingleOrDefault(u => u.Username == username);
            if (user == null) return Redirect("/sign-in?incorrect-username");

            if (!Crypter.CheckPassword(password, user.Password)) return Redirect("/sign-in?incorrect-password");

            return await this.SignUserInAsync(user, next);
        }
    }
}
