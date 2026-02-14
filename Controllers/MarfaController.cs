using Proiect_ASPDOTNET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Models.ViewModels;
using Proiect_ASPDOTNET.Services;
using System.Text.Json;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.ResponsabilDepozit, UserRole.DirectorCompanie, UserRole.SuperAdmin)]
    public class MarfaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public MarfaController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            IQueryable<Marfa> query = _context.Marfuri.Include(m => m.Depozit);

            if (userRole == UserRole.ResponsabilDepozit)
            {
                var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
                query = query.Where(m => m.DepozitId == depozitId);
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                query = query.Where(m => m.Depozit.CompanieId == companieId);
            }

            var marfuri = await query.OrderBy(m => m.Zona).ThenBy(m => m.Etaj).ToListAsync();
            return View(marfuri);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (userRole == UserRole.ResponsabilDepozit)
            {
                var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
                ViewBag.DepozitId = depozitId;
                ViewBag.DepozitNume = (await _context.Depozite.FindAsync(depozitId))?.Nume;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarfaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (userRole == UserRole.ResponsabilDepozit)
            {
                model.DepozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
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
                DataAdaugare = DateTime.Now
            };

            _context.Marfuri.Add(marfa);
            await _context.SaveChangesAsync();

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(userId, "Adaugare Marfa",
                $"Marfa noua: {marfa.Name} (SKU: {marfa.SKU}, Cantitate: {marfa.CapacitateCurenta})",
                marfa.DepozitId);

            TempData["Success"] = "Marfa adaugata cu succes!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var marfa = await _context.Marfuri.FindAsync(id);
            if (marfa == null)
            {
                return NotFound();
            }

            var model = new MarfaViewModel
            {
                Id = marfa.Id,
                Nume = marfa.Name,
                Descriere = marfa.Descriere,
                SKU = marfa.SKU,
                CantitateCurenta = marfa.CapacitateCurenta,
                UnitateMasura = marfa.UnitateMasura,
                PretUnitar = marfa.PretUnitar,
                Zona = marfa.Zona,
                Etaj = marfa.Etaj,
                Raft = marfa.Raft,
                Pozitie = marfa.Pozitie,
                DepozitId = marfa.DepozitId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MarfaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var marfa = await _context.Marfuri.FindAsync(model.Id);
            if (marfa == null)
            {
                return NotFound();
            }

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

            await _context.SaveChangesAsync();

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(userId, "Editare Marfa",
                $"Marfa editata: {marfa.Name} (SKU: {marfa.SKU})", marfa.DepozitId);

            TempData["Success"] = "Marfa actualizata cu succes!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var marfa = await _context.Marfuri
                .Include(m => m.Depozit)
                .ThenInclude(d => d.Companie)
                .Include(m => m.Tranzactii.OrderByDescending(t => t.DataTranzactie).Take(10))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (marfa == null)
            {
                return NotFound();
            }

            return View(marfa);
        }
    }
}