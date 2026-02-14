using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.ViewModels;

namespace Proiect_ASPDOTNET.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public AuthController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (AuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Activ);

            if (user == null || !AuthHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Username sau parola incorecte");
                return View(model);
            }

            AuthHelper.SetCurrentUser(HttpContext.Session, user);

            // Log login
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.LogActivityAsync(user.Id, "Login", "Utilizator autentificat", user.DepozitId, ipAddress);

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            if (userId.HasValue)
            {
                await _logService.LogActivityAsync(userId.Value, "Logout", "Utilizator deconectat");
            }

            AuthHelper.Logout(HttpContext.Session);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}