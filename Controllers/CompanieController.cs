using Proiect_ASPDOTNET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Models.ViewModels;
using Proiect_ASPDOTNET.Services;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.SuperAdmin)]
    public class CompanieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public CompanieController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var companii = await _context.Companii
                .Include(c => c.Depozite)
                .OrderByDescending(c => c.DataInregistrare)
                .ToListAsync();
            return View(companii);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var companie = new Companie
            {
                CompanieId = IdGenerator.GenerateCompanieId(),
                Nume = model.Nume,
                CUI = model.CUI,
                Adresa = model.Adresa,
                Telefon = model.Telefon,
                Email = model.Email,
                DataInregistrare = DateTime.Now,
                Activa = true
            };

            _context.Companii.Add(companie);
            await _context.SaveChangesAsync();

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(userId, "Creare Companie",
                $"Companie noua creata: {companie.Nume} (ID: {companie.CompanieId})");

            TempData["Success"] = "Companie adaugata cu succes!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var companie = await _context.Companii.FindAsync(id);
            if (companie == null)
            {
                return NotFound();
            }

            var model = new CompanieViewModel
            {
                Id = companie.Id,
                Nume = companie.Nume,
                CUI = companie.CUI,
                Adresa = companie.Adresa,
                Telefon = companie.Telefon,
                Email = companie.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var companie = await _context.Companii.FindAsync(model.Id);
            if (companie == null)
            {
                return NotFound();
            }

            companie.Nume = model.Nume;
            companie.CUI = model.CUI;
            companie.Adresa = model.Adresa;
            companie.Telefon = model.Telefon;
            companie.Email = model.Email;

            await _context.SaveChangesAsync();

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(userId, "Editare Companie",
                $"Companie editata: {companie.Nume} (ID: {companie.CompanieId})");

            TempData["Success"] = "Companie actualizata cu succes!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var companie = await _context.Companii
                .Include(c => c.Depozite)
                .Include(c => c.Utilizatori)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (companie == null)
            {
                return Json(new { success = false, message = "Companie negasita" });
            }

            // Check if has active warehouses or users
            if (companie.Depozite.Any(d => d.Active) || companie.Utilizatori.Any(u => u.Activ))
            {
                return Json(new { success = false, message = "Nu poti sterge o companie cu depozite sau utilizatori activi" });
            }

            companie.Active = false;
            await _context.SaveChangesAsync();

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(userId, "Stergere Companie",
                $"Companie stearsa: {companie.Nume} (ID: {companie.CompanieId})");

            return Json(new { success = true, message = "Companie stearsa cu succes" });
        }

        public async Task<IActionResult> Details(int id)
        {
            var companie = await _context.Companii
                .Include(c => c.Depozite)
                .Include(c => c.Utilizatori)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (companie == null)
            {
                return NotFound();
            }

            return View(companie);
        }
    }
}