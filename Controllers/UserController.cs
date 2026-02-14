using Proiect_ASPDOTNET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Services;
using System.Text.Json;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie, UserRole.ResponsabilDepozit)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public UserController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            IQueryable<User> query = _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit);

            if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                query = query.Where(u => u.CompanieId == companieId);
            }
            else if (userRole == UserRole.ResponsabilDepozit)
            {
                var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
                query = query.Where(u => u.DepozitId == depozitId && u.Rol == UserRole.Muncitor);
            }

            var users = await query.OrderBy(u => u.NumeComplet).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User model, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ModelState.AddModelError("password", "Parola trebuie sa aiba minim 6 caractere");
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateSelectLists();
                return View(model);
            }

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username-ul este deja folosit");
                await LoadCreateSelectLists();
                return View(model);
            }

            var user = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = model.Username,
                PasswordHash = AuthHelper.HashPassword(password),
                Email = model.Email,
                NumeComplet = model.NumeComplet,
                Rol = model.Rol,
                CompanieId = model.CompanieId,
                DepozitId = model.DepozitId,
                DataCreare = DateTime.Now,
                Activ = true,
                PuncteReward = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var currentUserId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(currentUserId, "Creare Utilizator",
                $"Utilizator nou: {user.NumeComplet} ({user.Username}), Rol: {user.Rol}");

            TempData["Success"] = "Utilizator creat cu succes!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await LoadEditSelectLists(user.CompanieId, user.DepozitId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model, string newPassword)
        {
            if (!ModelState.IsValid)
            {
                await LoadEditSelectLists(model.CompanieId, model.DepozitId);
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.NumeComplet = model.NumeComplet;
            user.Email = model.Email;
            user.Rol = model.Rol;
            user.CompanieId = model.CompanieId;
            user.DepozitId = model.DepozitId;
            user.Activ = model.Activ;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword.Length < 6)
                {
                    ModelState.AddModelError("newPassword", "Parola trebuie sa aiba minim 6 caractere");
                    await LoadEditSelectLists(model.CompanieId, model.DepozitId);
                    return View(model);
                }
                user.PasswordHash = AuthHelper.HashPassword(newPassword);
            }

            await _context.SaveChangesAsync();

            var currentUserId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(currentUserId, "Editare Utilizator",
                $"Utilizator editat: {user.NumeComplet} ({user.Username})");

            TempData["Success"] = "Utilizator actualizat cu succes!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .Include(u => u.Sarcini)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public async Task<JsonResult> GetDepoziteByCompanie(int companieId)
        {
            var depozite = await _context.Depozite
                .Where(d => d.CompanieId == companieId && d.Active)
                .Select(d => new { d.Id, d.Nume })
                .ToListAsync();

            return Json(depozite);
        }

        private async Task LoadCreateSelectLists()
        {
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);

            if (userRole == UserRole.SuperAdmin)
            {
                ViewBag.Companii = new SelectList(
                    await _context.Companii.Where(c => c.Active).ToListAsync(),
                    "Id", "Nume");
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                ViewBag.Companii = new SelectList(
                    await _context.Companii.Where(c => c.Id == companieId).ToListAsync(),
                    "Id", "Nume");

                ViewBag.Depozite = new SelectList(
                    await _context.Depozite.Where(d => d.CompanieId == companieId && d.Active).ToListAsync(),
                    "Id", "Nume");
            }
            else if (userRole == UserRole.ResponsabilDepozit)
            {
                var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
                var depozit = await _context.Depozite.Include(d => d.Companie).FirstOrDefaultAsync(d => d.Id == depozitId);

                ViewBag.Companii = new SelectList(new[] { depozit.Companie }, "Id", "Nume");
                ViewBag.Depozite = new SelectList(new[] { depozit }, "Id", "Nume");
            }
        }

        private async Task LoadEditSelectLists(int? companieId, int? depozitId)
        {
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (userRole == UserRole.SuperAdmin)
            {
                ViewBag.Companii = new SelectList(
                    await _context.Companii.Where(c => c.Active).ToListAsync(),
                    "Id", "Nume", companieId);

                if (companieId.HasValue)
                {
                    ViewBag.Depozite = new SelectList(
                        await _context.Depozite.Where(d => d.CompanieId == companieId && d.Active).ToListAsync(),
                        "Id", "Nume", depozitId);
                }
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
                var userCompanieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();

                ViewBag.Companii = new SelectList(
                    await _context.Companii.Where(c => c.Id == userCompanieId).ToListAsync(),
                    "Id", "Nume", companieId);

                ViewBag.Depozite = new SelectList(
                    await _context.Depozite.Where(d => d.CompanieId == userCompanieId && d.Active).ToListAsync(),
                    "Id", "Nume", depozitId);
            }
        }
    }
}