using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Models.ViewModels;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie, UserRole.ResponsabilDepozit)]
    public class MarfaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public MarfaController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index(string search, string zona)
        {
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            var query = _context.Marfuri
                .Include(m => m.Depozit)
                    .ThenInclude(d => d.Companie)
                .AsQueryable();

            // Filtrare pe bază de rol
            if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                query = query.Where(m => m.Depozit.CompanieId == companieId);
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var depozitId = userData["DepozitId"].GetInt32();

                query = query.Where(m => m.DepozitId == depozitId);
            }

            // Filtrare pe search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Name.Contains(search) || m.SKU.Contains(search) || m.Descriere.Contains(search));
                ViewBag.Search = search;
            }

            // Filtrare pe zonă
            if (!string.IsNullOrWhiteSpace(zona))
            {
                query = query.Where(m => m.Zona == zona);
                ViewBag.ZonaSelectata = zona;
            }

            var marfuri = await query
                .OrderBy(m => m.Zona)
                .ThenBy(m => m.Etaj)
                .ThenBy(m => m.Raft)
                .ToListAsync();

            return View(marfuri);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (currentUserRole == UserRole.SuperAdmin)
            {
                ViewBag.Depozite = new SelectList(
                    await _context.Depozite.Where(d => d.Active).Include(d => d.Companie).ToListAsync(),
                    "Id",
                    "Nume");
            }
            else if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                ViewBag.Depozite = new SelectList(
                    await _context.Depozite.Where(d => d.Active && d.CompanieId == companieId).ToListAsync(),
                    "Id",
                    "Nume");
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var depozitId = userData["DepozitId"].GetInt32();

                var depozit = await _context.Depozite.FindAsync(depozitId);
                ViewBag.Depozite = new SelectList(new[] { depozit }, "Id", "Nume");
                ViewBag.DepozitId = depozitId;
                ViewBag.DepozitNume = depozit?.Nume;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarfaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewBagForCreate();
                return View(model);
            }

            // Verifică dacă SKU-ul există deja
            if (await _context.Marfuri.AnyAsync(m => m.SKU == model.SKU))
            {
                ModelState.AddModelError("SKU", "SKU-ul este deja folosit.");
                await PrepareViewBagForCreate();
                return View(model);
            }

            var marfa = new Marfa
            {
                MarfaId = IdGenerator.GenerateMarfaId(),
                Name = model.Nume,
                Descriere = model.Descriere,
                SKU = model.SKU,
                DepozitId = model.DepozitId,
                CapacitateCurenta = model.CantitateCurenta,
                UnitateMasura = model.UnitateMasura,
                PretUnitar = model.PretUnitar,
                Zona = model.Zona,
                Etaj = model.Etaj,
                Raft = model.Raft,
                Pozitie = model.Pozitie,
                DataAdaugare = DateTime.Now,
                DataUltimaModificare = null
            };

            _context.Marfuri.Add(marfa);

            try
            {
                await _context.SaveChangesAsync();

                var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
                if (userId.HasValue)
                {
                    await _logService.LogActivityAsync(
                        userId.Value,
                        "Adaugare Marfa",
                        $"Marfa noua: {marfa.Name} (SKU: {marfa.SKU})",
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        marfa.DepozitId);
                }

                TempData["Success"] = "Marfa a fost adaugata cu succes!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "A aparut o eroare la salvarea marfii: " + ex.Message;
                await PrepareViewBagForCreate();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var marfa = await _context.Marfuri
                .Include(m => m.Depozit)
                    .ThenInclude(d => d.Companie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (marfa == null)
            {
                TempData["Error"] = "Marfa nu a fost gasita.";
                return RedirectToAction("Index");
            }

            // Verificare permisiuni
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                if (marfa.Depozit.CompanieId != companieId)
                {
                    TempData["Error"] = "Nu aveti permisiunea sa editati aceasta marfa.";
                    return RedirectToAction("Index");
                }
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var depozitId = userData["DepozitId"].GetInt32();

                if (marfa.DepozitId != depozitId)
                {
                    TempData["Error"] = "Nu aveti permisiunea sa editati aceasta marfa.";
                    return RedirectToAction("Index");
                }
            }

            var model = new MarfaViewModel
            {
                Id = marfa.Id,
                Nume = marfa.Name,
                Descriere = marfa.Descriere,
                SKU = marfa.SKU,
                DepozitId = marfa.DepozitId,
                CantitateCurenta = marfa.CapacitateCurenta,
                UnitateMasura = marfa.UnitateMasura,
                PretUnitar = marfa.PretUnitar,
                Zona = marfa.Zona,
                Etaj = marfa.Etaj,
                Raft = marfa.Raft,
                Pozitie = marfa.Pozitie
            };

            await PrepareViewBagForEdit(marfa.DepozitId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MarfaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewBagForEdit(model.DepozitId);
                return View(model);
            }

            var marfa = await _context.Marfuri.FindAsync(model.Id);

            if (marfa == null)
            {
                TempData["Error"] = "Marfa nu a fost gasita.";
                return RedirectToAction("Index");
            }

            // Verifică SKU unic (dacă s-a schimbat)
            if (marfa.SKU != model.SKU)
            {
                if (await _context.Marfuri.AnyAsync(m => m.SKU == model.SKU && m.Id != model.Id))
                {
                    ModelState.AddModelError("SKU", "SKU-ul este deja folosit.");
                    await PrepareViewBagForEdit(model.DepozitId);
                    return View(model);
                }
            }

            // Actualizează doar informațiile editabile
            // NOTA: Cantitatea se modifică doar prin tranzacții
            marfa.Name = model.Nume;
            marfa.Descriere = model.Descriere;
            marfa.SKU = model.SKU;
            marfa.UnitateMasura = model.UnitateMasura;
            marfa.PretUnitar = model.PretUnitar;
            marfa.Zona = model.Zona;
            marfa.Etaj = model.Etaj;
            marfa.Raft = model.Raft;
            marfa.Pozitie = model.Pozitie;
            marfa.DataUltimaModificare = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
                if (userId.HasValue)
                {
                    await _logService.LogActivityAsync(
                        userId.Value,
                        "Editare Marfa",
                        $"Marfa editata: {marfa.Name} (SKU: {marfa.SKU})",
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        marfa.DepozitId);
                }

                TempData["Success"] = "Marfa a fost actualizata cu succes!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "A aparut o eroare la actualizarea marfii: " + ex.Message;
                await PrepareViewBagForEdit(model.DepozitId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var marfa = await _context.Marfuri
                .Include(m => m.Depozit)
                    .ThenInclude(d => d.Companie)
                .Include(m => m.Tranzactii)
                    .ThenInclude(t => t.User)
                .Include(m => m.Tranzactii)
                    .ThenInclude(t => t.DepozitSursa)
                .Include(m => m.Tranzactii)
                    .ThenInclude(t => t.DepozitDestinatie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (marfa == null)
            {
                TempData["Error"] = "Marfa nu a fost gasita.";
                return RedirectToAction("Index");
            }

            // Verificare permisiuni
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                if (marfa.Depozit.CompanieId != companieId)
                {
                    TempData["Error"] = "Nu aveti permisiunea sa vizualizati aceasta marfa.";
                    return RedirectToAction("Index");
                }
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var depozitId = userData["DepozitId"].GetInt32();

                if (marfa.DepozitId != depozitId)
                {
                    TempData["Error"] = "Nu aveti permisiunea sa vizualizati aceasta marfa.";
                    return RedirectToAction("Index");
                }
            }

            return View(marfa);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var marfa = await _context.Marfuri
                .Include(m => m.Tranzactii)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (marfa == null)
            {
                return Json(new { success = false, message = "Marfa nu a fost gasita." });
            }

            // Nu permite ștergerea dacă există tranzacții
            if (marfa.Tranzactii.Any())
            {
                return Json(new { success = false, message = "Nu puteti sterge o marfa care are tranzactii asociate." });
            }

            // Nu permite ștergerea dacă există stoc
            if (marfa.CapacitateCurenta > 0)
            {
                return Json(new { success = false, message = "Nu puteti sterge o marfa cu stoc disponibil. Reduceti mai intai stocul la 0." });
            }

            try
            {
                _context.Marfuri.Remove(marfa);
                await _context.SaveChangesAsync();

                var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
                if (userId.HasValue)
                {
                    await _logService.LogActivityAsync(
                        userId.Value,
                        "Stergere Marfa",
                        $"Marfa stearsa: {marfa.Name} (SKU: {marfa.SKU})",
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        marfa.DepozitId);
                }

                return Json(new { success = true, message = "Marfa a fost stearsa cu succes!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "A aparut o eroare la stergerea marfii: " + ex.Message });
            }
        }

        // Helper methods
        private async Task PrepareViewBagForCreate()
        {
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (currentUserRole == UserRole.SuperAdmin)
            {
                ViewBag.Depozite = new SelectList(
                    await _context.Depozite.Where(d => d.Active).Include(d => d.Companie).ToListAsync(),
                    "Id",
                    "Nume");
            }
            else if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                ViewBag.Depozite = new SelectList(
                    await _context.Depozite.Where(d => d.Active && d.CompanieId == companieId).ToListAsync(),
                    "Id",
                    "Nume");
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var depozitId = userData["DepozitId"].GetInt32();

                var depozit = await _context.Depozite.FindAsync(depozitId);
                ViewBag.Depozite = new SelectList(new[] { depozit }, "Id", "Nume");
                ViewBag.DepozitId = depozitId;
                ViewBag.DepozitNume = depozit?.Nume;
            }
        }

        private async Task PrepareViewBagForEdit(int depozitId)
        {
            var currentUserRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (currentUserRole == UserRole.SuperAdmin)
            {
                ViewBag.Depozite = new SelectList(
                    await _context.Depozite.Where(d => d.Active ).ToListAsync(),
                    "Id",
                    "Nume",
                    depozitId);
            }
            else if (currentUserRole == UserRole.DirectorCompanie)
            {
                var userDataJson = HttpContext.Session.GetString("_CurrentUser");
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userDataJson);
                var companieId = userData["CompanieId"].GetInt32();

                ViewBag.Depozite = new SelectList(
                    await _context.Depozite.Where(d => d.Active && d.CompanieId == companieId).ToListAsync(),
                    "Id",
                    "Nume",
                    depozitId);
            }
            else if (currentUserRole == UserRole.ResponsabilDepozit)
            {
                var depozit = await _context.Depozite.FindAsync(depozitId);
                ViewBag.Depozite = new SelectList(new[] { depozit }, "Id", "Nume", depozitId);
                ViewBag.DepozitBlocat = true;
            }
        }
    }
}