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
            // Dacă utilizatorul este deja autentificat, redirecționează la dashboard
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

            var user = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Activ);

            if (user == null || !AuthHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Username sau parola incorecte.");
                return View(model);
            }

            // Setează sesiunea
            AuthHelper.SetCurrentUser(HttpContext.Session, user);

            // Log activitate
            await _logService.LogActivityAsync("Login", "Utilizator autentificat cu succes", user.DepozitId);

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                // Log înainte de a șterge sesiunea
                await _logService.LogActivityAsync("Logout", "Utilizator deconectat");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging logout: {ex.Message}");
            }

            // Șterge sesiunea
            AuthHelper.Logout(HttpContext.Session);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}