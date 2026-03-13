using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Models.ViewModels;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            await _logService.LogActivityAsync(userId.Value, "Adaugare Companie",
                $"Companie noua: {companie.Nume}");

            TempData["Success"] = "Compania a fost adaugata cu succes!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var companie = await _context.Companii
                .Include(c => c.Depozite)
                .Include(c => c.Utilizatori)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (companie == null)
            {
                TempData["Error"] = "Compania nu a fost gasita.";
                return RedirectToAction("Index");
            }

            return View(companie);
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
                return Json(new { success = false, message = "Compania nu a fost gasita." });
            }

            if (companie.Depozite.Any(d => d.Activ) || companie.Utilizatori.Any(u => u.Activ))
            {
                return Json(new { success = false, message = "Nu poti sterge o companie cu depozite sau utilizatori activi." });
            }

            companie.Activa = false;
            await _context.SaveChangesAsync();

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);
            await _logService.LogActivityAsync(userId.Value, "Stergere Companie",
                $"Companie stearsa: {companie.Nume}");

            return Json(new { success = true, message = "Compania a fost stearsa cu succes!" });
        }
    }
}