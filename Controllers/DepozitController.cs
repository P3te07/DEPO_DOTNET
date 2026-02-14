using Proiect_ASPDOTNET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Models.ViewModels;
using Proiect_ASPDOTNET.Services;
using System.Text.Json;

namespace Proiect_ASPDOTNET.Controllers
{
    public class DepozitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public DepozitController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie)]
        public async Task<IActionResult> Index()
        {
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);

            IQueryable<Depozit> query = _context.Depozite.Include(d => d.Companie);

            if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                query = query.Where(d => d.CompanieId == companieId);
            }

            var depozite = await query.OrderBy(d => d.Nume).ToListAsync();
            return View(depozite);
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie)]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);

            if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                ViewBag.Companii = new SelectList(
                    await _context.Companii.Where(c => c.Id == companieId && c.Active).ToListAsync(),
                    "Id", "Nume");
            }
            else
            {
                ViewBag.Companii = new SelectList(
                    await _context.Companii.Where(c => c.Active).ToListAsync(),
                    "Id", "Nume");
            }

            return View();
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepozitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCompaniiSelectList(model.CompanieId);
                return View(model);
            }

            var depozit = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = model.Nume,
                CompanieId = model.CompanieId,
                Adresa = model.Adresa,
                Latitudine = model.Latitudine,
                Longitudine = model.Longitudine,
                CapacitateMaxima = model.CapacitateMaxima,
                DataDeschidere = DateTime.Now,
                Active = true
            };

            _context.Depozite.Add(depozit);
            await _context.SaveChangesAsync();

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(userId, "Creare Depozit",
                $"Depozit nou creat: {depozit.Nume} (ID: {depozit.DepozitId})", depozit.Id);

            TempData["Success"] = "Depozit adaugat cu succes!";
            return RedirectToAction(nameof(Index));
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var depozit = await _context.Depozite.FindAsync(id);
            if (depozit == null)
            {
                return NotFound();
            }

            // Check authorization for DirectorCompanie
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);
            if (userRole == UserRole.DirectorCompanie)
            {
                var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                if (depozit.CompanieId != companieId)
                {
                    return Forbid();
                }
            }

            var model = new DepozitViewModel
            {
                Id = depozit.Id,
                Nume = depozit.Nume,
                CompanieId = depozit.CompanieId,
                Adresa = depozit.Adresa,
                Latitudine = depozit.Latitudine,
                Longitudine = depozit.Longitudine,
                CapacitateMaxima = depozit.CapacitateMaxima
            };

            await LoadCompaniiSelectList(model.CompanieId);
            return View(model);
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepozitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCompaniiSelectList(model.CompanieId);
                return View(model);
            }

            var depozit = await _context.Depozite.FindAsync(model.Id);
            if (depozit == null)
            {
                return NotFound();
            }

            depozit.Nume = model.Nume;
            depozit.Adresa = model.Adresa;
            depozit.Latitudine = model.Latitudine;
            depozit.Longitudine = model.Longitudine;
            depozit.CapacitateMaxima = model.CapacitateMaxima;

            await _context.SaveChangesAsync();

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            await _logService.LogActivityAsync(userId, "Editare Depozit",
                $"Depozit editat: {depozit.Nume} (ID: {depozit.DepozitId})", depozit.Id);

            TempData["Success"] = "Depozit actualizat cu succes!";
            return RedirectToAction(nameof(Index));
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie, UserRole.ResponsabilDepozit)]
        public async Task<IActionResult> Details(int id)
        {
            var depozit = await _context.Depozite
                .Include(d => d.Companie)
                .Include(d => d.Marfuri)
                .Include(d => d.Utilizatori)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (depozit == null)
            {
                return NotFound();
            }

            return View(depozit);
        }

        private async Task LoadCompaniiSelectList(int selectedId = 0)
        {
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);

            if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                ViewBag.Companii = new SelectList(
                    await _context.Companii.Where(c => c.Id == companieId && c.Active).ToListAsync(),
                    "Id", "Nume", selectedId);
            }
            else
            {
                ViewBag.Companii = new SelectList(
                    await _context.Companii.Where(c => c.Active).ToListAsync(),
                    "Id", "Nume", selectedId);
            }
        }
    }
}