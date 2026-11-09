using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using AnimalCareVeterinaryClinicDBFirst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AnimalCareVeterinaryClinicDBFirst.Controllers
{
    public class AuthController : Controller
    {
        private readonly AnimalCareContext _context;

        public AuthController(AnimalCareContext context)
        {
            _context = context;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            // PG-MIGRATION (7a): PostgreSQL string comparison is case-sensitive by default.
            // SQL Server's default collation was case-insensitive. Compare lower-cased values
            // so existing users can still log in regardless of the case they type.
            var loginLower = model.Username.Trim().ToLower();

            var user = await _context.UserAccounts
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.UserLogin.ToLower() == loginLower &&
                    u.UserPassword == model.Password);

            if (user == null)
            {
                model.ErrorMessage = "Invalid Username or Password";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserLogin),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET: Auth/AccessDenied
        [Authorize]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // POST: Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}