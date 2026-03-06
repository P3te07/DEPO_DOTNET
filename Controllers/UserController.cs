using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);
            var currentUserId = AuthHelper.GetCurrentUserId(HttpContext.Session);

            IQueryable<User> query = _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit);

            // Filtrare pe bază de rol
            if (currentUserRole == UserRole.DirectorCompanie)
            {
                // Directorul vede doar utilizatorii din compania sa
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                query = query.Where(u => u.CompanieId == companieId);
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                // Responsabilul vede doar muncitorii din depozitul sau
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var depozitId = userData["DepozitId"].GetInt32();

                query = query.Where(u => u.DepozitId == depozitId && u.Rol == UserRole.Muncitor);
            }

            var users = await query.OrderBy(u => u.NumeComplet).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (currentUserRole == UserRole.SuperAdmin)
            {
                ViewBag.Companii = new SelectList(await _context.Companii.Where(c => c.Active).ToListAsync(), "Id", "Nume");
            }
            else if (currentUserRole == UserRole.DirectorCompanie)
            {
                // Director poate crea doar utilizatori din compania sa
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                var companie = await _context.Companii.FindAsync(companieId);
                ViewBag.Companii = new SelectList(new[] { companie }, "Id", "Nume");
                ViewBag.CompanieBlocata = true;
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                // Responsabil poate crea doar muncitori in depozitul sau
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();
                var depozitId = userData["DepozitId"].GetInt32();

                var companie = await _context.Companii.FindAsync(companieId);
                var depozit = await _context.Depozite.FindAsync(depozitId);

                ViewBag.Companii = new SelectList(new[] { companie }, "Id", "Nume");
                ViewBag.Depozite = new SelectList(new[] { depozit }, "Id", "Nume");
                ViewBag.CompanieBlocata = true;
                ViewBag.DepozitBlocat = true;
                ViewBag.RolBlocat = true; // Poate crea doar muncitori
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User model, string password)
        {
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            // Validări
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ModelState.AddModelError("", "Parola trebuie sa aiba minim 6 caractere.");
            }

            // Verifică username unic
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username-ul este deja folosit.");
            }

            // Validări specifice pe rol
            if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                if (model.CompanieId != companieId)
                {
                    ModelState.AddModelError("", "Nu puteti crea utilizatori in alte companii.");
                }

                if (model.Rol == UserRole.SuperAdmin)
                {
                    ModelState.AddModelError("", "Nu puteti crea SuperAdmin.");
                }
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                if (model.Rol != UserRole.Muncitor)
                {
                    ModelState.AddModelError("", "Puteti crea doar muncitori.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PrepareViewBagForCreate(currentUserRole);
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

            var currentUserId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            await _logService.LogActivityAsync(currentUserId.Value, "Adaugare Utilizator",
                $"Utilizator nou: {user.NumeComplet} ({user.Username})");

            TempData["Success"] = "Utilizatorul a fost adaugat cu succes!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                TempData["Error"] = "Utilizatorul nu a fost gasit.";
                return RedirectToAction("Index");
            }

            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            // Verificare permisiuni
            if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                if (user.CompanieId != companieId)
                {
                    TempData["Error"] = "Nu aveti permisiunea sa editati acest utilizator.";
                    return RedirectToAction("Index");
                }
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var depozitId = userData["DepozitId"].GetInt32();

                if (user.DepozitId != depozitId || user.Rol != UserRole.Muncitor)
                {
                    TempData["Error"] = "Nu aveti permisiunea sa editati acest utilizator.";
                    return RedirectToAction("Index");
                }
            }

            await PrepareViewBagForEdit(currentUserRole, user);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User model)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                TempData["Error"] = "Utilizatorul nu a fost gasit.";
                return RedirectToAction("Index");
            }

            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            // Verificare permisiuni
            if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                if (user.CompanieId != companieId)
                {
                    TempData["Error"] = "Nu aveti permisiunea sa editati acest utilizator.";
                    return RedirectToAction("Index");
                }
            }

            // IMPORTANT: Username și Parola NU se pot edita aici
            // Se editează doar prin profilul propriu
            user.NumeComplet = model.NumeComplet;
            user.Email = model.Email;
            user.Activ = model.Activ;

            // SuperAdmin poate modifica rol și asignare
            if (currentUserRole == UserRole.SuperAdmin)
            {
                user.Rol = model.Rol;
                user.CompanieId = model.CompanieId;
                user.DepozitId = model.DepozitId;
            }
            // Director poate modifica rol (dar nu SuperAdmin) și depozit
            else if (currentUserRole == UserRole.DirectorCompanie)
            {
                if (model.Rol != UserRole.SuperAdmin)
                {
                    user.Rol = model.Rol;
                }
                user.DepozitId = model.DepozitId;
            }
            // Responsabil nu poate modifica rol sau depozit

            await _context.SaveChangesAsync();

            var currentUserId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            await _logService.LogActivityAsync(currentUserId.Value, "Editare Utilizator",
                $"Utilizator editat: {user.NumeComplet}");

            TempData["Success"] = "Utilizatorul a fost actualizat cu succes!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .Include(u => u.Sarcini)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                TempData["Error"] = "Utilizatorul nu a fost gasit.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        [HttpGet]
        public async Task<JsonResult> GetDepoziteByCompanie(int companieId)
        {
            var depozite = await _context.Depozite
                .Where(d => d.CompanieId == companieId && d.Active)
                .Select(d => new { id = d.Id, nume = d.Nume })
                .ToListAsync();

            return Json(depozite);
        }

        private async Task PrepareViewBagForCreate(UserRole? currentUserRole)
        {
            if (currentUserRole == UserRole.SuperAdmin)
            {
                ViewBag.Companii = new SelectList(await _context.Companii.Where(c => c.Active).ToListAsync(), "Id", "Nume");
            }
            else if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                var companie = await _context.Companii.FindAsync(companieId);
                ViewBag.Companii = new SelectList(new[] { companie }, "Id", "Nume");
                ViewBag.CompanieBlocata = true;
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();
                var depozitId = userData["DepozitId"].GetInt32();

                var companie = await _context.Companii.FindAsync(companieId);
                var depozit = await _context.Depozite.FindAsync(depozitId);

                ViewBag.Companii = new SelectList(new[] { companie }, "Id", "Nume");
                ViewBag.Depozite = new SelectList(new[] { depozit }, "Id", "Nume");
                ViewBag.CompanieBlocata = true;
                ViewBag.DepozitBlocat = true;
                ViewBag.RolBlocat = true;
            }
        }

        private async Task PrepareViewBagForEdit(UserRole? currentUserRole, User user)
        {
            if (currentUserRole == UserRole.SuperAdmin)
            {
                ViewBag.Companii = new SelectList(await _context.Companii.Where(c => c.Active).ToListAsync(), "Id", "Nume", user.CompanieId);

                if (user.CompanieId.HasValue)
                {
                    ViewBag.Depozite = new SelectList(
                        await _context.Depozite.Where(d => d.CompanieId == user.CompanieId && d.Active).ToListAsync(),
                        "Id", "Nume", user.DepozitId);
                }
            }
            else if (currentUserRole == UserRole.DirectorCompanie)
            {
                var companie = await _context.Companii.FindAsync(user.CompanieId);
                ViewBag.Companii = new SelectList(new[] { companie }, "Id", "Nume", user.CompanieId);
                ViewBag.CompanieBlocata = true;

                if (user.CompanieId.HasValue)
                {
                    ViewBag.Depozite = new SelectList(
                        await _context.Depozite.Where(d => d.CompanieId == user.CompanieId && d.Active).ToListAsync(),
                        "Id", "Nume", user.DepozitId);
                }
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var companie = await _context.Companii.FindAsync(user.CompanieId);
                var depozit = await _context.Depozite.FindAsync(user.DepozitId);

                ViewBag.Companii = new SelectList(new[] { companie }, "Id", "Nume", user.CompanieId);
                ViewBag.Depozite = new SelectList(new[] { depozit }, "Id", "Nume", user.DepozitId);
                ViewBag.CompanieBlocata = true;
                ViewBag.DepozitBlocat = true;
                ViewBag.RolBlocat = true;
            }
        }
    }
}