using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.ViewModels;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            // Dacă utilizatorul este deja autentificat, redirectează la dashboard
            if (AuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verifică credențialele
            var user = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Activ);

            if (user == null || !AuthHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Username sau parola incorecte.");
                return View(model);
            }

            // Setează sesiunea - CORECTARE: ordinea corectă a parametrilor
            AuthHelper.SetCurrentUser(
                HttpContext.Session,
                user.Id,              // int id
                user.UserId,          // string userId
                user.Username,        // string username
                user.Email,           // string email
                user.NumeComplet,     // string numeComplet
                user.Rol,             // UserRole rol
                user.CompanieId,      // int? companieId
                user.DepozitId        // int? depozitId
            );

            // Log activitate - CORECTARE: toate parametrii necesari
            await _logService.LogActivityAsync(
                user.Id,                                             // int userId
                "Login",                                             // string actiune
                "Utilizator autentificat cu succes",                 // string detalii
                HttpContext.Connection.RemoteIpAddress?.ToString(),  // string? adresaIP
                user.DepozitId                                       // int? depozitId
            );

            TempData["Success"] = $"Bun venit, {user.NumeComplet}!";
            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);

            if (userId.HasValue)
            {
                // Log activitate înainte de logout
                await _logService.LogActivityAsync(
                    userId.Value,
                    "Logout",
                    "Utilizator deconectat",
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
            }

            AuthHelper.Logout(HttpContext.Session);
            TempData["Success"] = "Te-ai deconectat cu succes.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}