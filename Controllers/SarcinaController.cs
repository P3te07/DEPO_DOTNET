using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.ResponsabilDepozit, UserRole.DirectorCompanie)]
    public class SarcinaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public SarcinaController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var sarcini = await _context.Sarcini
                .Include(s => s.User)
                .OrderByDescending(s => s.DataLimita)
                .ToListAsync();

            return View(sarcini);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);
            int? depozitId = AuthHelper.GetCurrentUser(HttpContext.Session)?.DepozitId;

            IQueryable<User> muncitoriQuery = _context.Users.Where(u => u.Rol == UserRole.Muncitor && u.Activ);

            if (userRole == UserRole.ResponsabilDepozit && depozitId.HasValue)
            {
                muncitoriQuery = muncitoriQuery.Where(u => u.DepozitId == depozitId.Value);
            }

            var muncitori = await muncitoriQuery.ToListAsync();

            ViewBag.Muncitori = new SelectList(muncitori, "Id", "NumeComplet");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sarcina sarcina)
        {
            if (ModelState.IsValid)
            {
                sarcina.Finalizata = false;
                sarcina.DataFinalizare = null;

                _context.Sarcini.Add(sarcina);
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(sarcina.UserId);
                await _logService.LogActivityAsync(
                    "Creare Sarcina",
                    $"Sarcina '{sarcina.Titlu}' atribuita utilizatorului {user?.NumeComplet}",
                    user?.DepozitId
                );

                TempData["Success"] = "Sarcina a fost creata cu succes!";
                return RedirectToAction(nameof(Index));
            }

            var muncitori = await _context.Users
                .Where(u => u.Rol == UserRole.Muncitor && u.Activ)
                .ToListAsync();
            ViewBag.Muncitori = new SelectList(muncitori, "Id", "NumeComplet");

            return View(sarcina);
        }

        [HttpPost]
        public async Task<IActionResult> Finalizeaza(int id)
        {
            var sarcina = await _context.Sarcini
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sarcina == null)
            {
                return NotFound();
            }

            sarcina.Finalizata = true;
            sarcina.DataFinalizare = DateTime.Now;

            // Adaugă puncte reward
            var user = sarcina.User;
            user.PuncteReward += sarcina.PuncteReward;

            await _context.SaveChangesAsync();

            await _logService.LogActivityAsync(
                "Finalizare Sarcina",
                $"Sarcina '{sarcina.Titlu}' finalizata. +{sarcina.PuncteReward} puncte reward",
                user.DepozitId
            );

            TempData["Success"] = $"Sarcina finalizata! +{sarcina.PuncteReward} puncte reward!";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var sarcina = await _context.Sarcini.FindAsync(id);
            if (sarcina == null)
            {
                return Json(new { success = false, message = "Sarcina nu a fost gasita." });
            }

            _context.Sarcini.Remove(sarcina);
            await _context.SaveChangesAsync();

            await _logService.LogActivityAsync("Stergere Sarcina", $"Sarcina '{sarcina.Titlu}' a fost stearsa");

            return Json(new { success = true, message = "Sarcina a fost stearsa cu succes!" });
        }
    }
}