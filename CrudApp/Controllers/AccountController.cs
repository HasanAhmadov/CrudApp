using System.Data;
using Dapper;
using CrudApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrudApp.Controllers
{
    public class AccountController : Controller
    {
        public List<User> users = null;

        private readonly IDbConnection _db;

        public AccountController(IDbConnection db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login(string ReturnUrl = "/")
        {
            LoginModel loginModel = new LoginModel();
            loginModel.ReturnUrl = ReturnUrl;
            return View(loginModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            string sql = @"SELECT u.Id, u.Username, u.Password, r.Name as Role
                           FROM Users u
                           JOIN Roles r on u.Role = r.Id
                           WHERE Username = @Username AND Password = @Password";

            var user = _db.QueryFirstOrDefault<User>(sql, new
            {
                Username = loginModel.Username,
                Password = loginModel.Password
            });

            if (user != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("MyCookieAuth", "Code")
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = loginModel.RememberLogin
                });

                if (string.IsNullOrEmpty(loginModel.ReturnUrl))
                    return RedirectToAction("Index", "Home");

                return LocalRedirect(loginModel.ReturnUrl);
            }
            else
            {
                ViewBag.Message = "Invalid credentials";
                return View(loginModel);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }
    }
}
