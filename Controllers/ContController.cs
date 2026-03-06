using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Controllers
{
    public class ContController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public ContController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Profil()
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            var user = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult SchimbareParola()
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SchimbareParola(string parolaVeche, string parolaNoua, string confirmaParola)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Validare
            if (string.IsNullOrWhiteSpace(parolaVeche) || string.IsNullOrWhiteSpace(parolaNoua))
            {
                TempData["Error"] = "Toate campurile sunt obligatorii.";
                return View();
            }

            if (parolaNoua.Length < 6)
            {
                TempData["Error"] = "Parola noua trebuie sa aiba minim 6 caractere.";
                return View();
            }

            if (parolaNoua != confirmaParola)
            {
                TempData["Error"] = "Parola noua si confirmarea nu se potrivesc.";
                return View();
            }

            // Verifică parola veche
            if (!AuthHelper.VerifyPassword(parolaVeche, user.PasswordHash))
            {
                TempData["Error"] = "Parola veche este incorecta.";
                return View();
            }

            // Schimbă parola
            user.PasswordHash = AuthHelper.HashPassword(parolaNoua);
            await _context.SaveChangesAsync();

            // LINIA 99 - Log activitate CORECTAT
            await _logService.LogActivityAsync(
                user.Id,                                             // userId
                "Schimbare Parola",                                  // actiune
                "Utilizatorul si-a schimbat parola",                 // detalii
                HttpContext.Connection.RemoteIpAddress?.ToString(),  // adresaIP (opțional)
                user.DepozitId                                       // depozitId (opțional)
            );

            TempData["Success"] = "Parola a fost schimbata cu succes!";
            return RedirectToAction("Profil");
        }

        [HttpGet]
        public async Task<IActionResult> EditareProfil()
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditareProfil(string username, string email, string numeComplet)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Validare
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(numeComplet))
            {
                TempData["Error"] = "Toate campurile sunt obligatorii.";
                return View(user);
            }

            // Verifică dacă username-ul este disponibil
            if (username != user.Username)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Id != userId);
                if (existingUser != null)
                {
                    TempData["Error"] = "Username-ul este deja folosit de alt utilizator.";
                    return View(user);
                }

                user.Username = username;
            }

            user.Email = email;
            user.NumeComplet = numeComplet;

            await _context.SaveChangesAsync();

            // Actualizează sesiunea cu noile date
            AuthHelper.SetCurrentUser(HttpContext.Session, user.Id, user.UserId, user.Username,
                user.Email, user.NumeComplet, user.Rol, user.CompanieId, user.DepozitId);

            await _logService.LogActivityAsync(user.Id, "Editare Profil",
                "Utilizatorul si-a actualizat profilul",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Profilul a fost actualizat cu succes!";
            return RedirectToAction("Profil");
        }
    }
}