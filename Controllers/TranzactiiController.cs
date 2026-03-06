using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Models.ViewModels;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.DirectorCompanie, UserRole.ResponsabilDepozit, UserRole.Muncitor)]
    public class TranzactieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public TranzactieController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index(DateTime? dataStart, DateTime? dataEnd, TipTranzactie? tip)
        {
            var query = _context.Tranzactii
                .Include(t => t.Marfa)
                .Include(t => t.User)
                .Include(t => t.DepozitSursa)
                .Include(t => t.DepozitDestinatie)
                .AsQueryable();

            if (dataStart.HasValue)
            {
                query = query.Where(t => t.DataTranzactie >= dataStart.Value);
                ViewBag.DataStart = dataStart;
            }

            if (dataEnd.HasValue)
            {
                query = query.Where(t => t.DataTranzactie <= dataEnd.Value);
                ViewBag.DataEnd = dataEnd;
            }

            if (tip.HasValue)
            {
                query = query.Where(t => t.Tip == tip.Value);
                ViewBag.TipSelectat = tip;
            }

            var tranzactii = await query.OrderByDescending(t => t.DataTranzactie).ToListAsync();
            return View(tranzactii);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Marfuri = new SelectList(await _context.Marfuri.ToListAsync(), "Id", "Nume");
            ViewBag.Depozite = new SelectList(await _context.Depozite.Where(d => d.Active).ToListAsync(), "Id", "Nume");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TranzactieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Marfuri = new SelectList(await _context.Marfuri.ToListAsync(), "Id", "Nume");
                ViewBag.Depozite = new SelectList(await _context.Depozite.ToListAsync(), "Id", "Nume");
                return View(model);
            }

            var marfa = await _context.Marfuri.FindAsync(model.MarfaId);
            if (marfa == null)
            {
                TempData["Error"] = "Marfa nu a fost gasita.";
                return RedirectToAction("Index");
            }

            // Validări specifice pe tip
            if (model.Tip == TipTranzactie.Iesire || model.Tip == TipTranzactie.Transfer)
            {
                if (marfa.CapacitateCurenta < model.Cantitate)
                {
                    TempData["Error"] = $"Stoc insuficient! Disponibil: {marfa.CapacitateCurenta} {marfa.UnitateMasura}";
                    ViewBag.Marfuri = new SelectList(await _context.Marfuri.ToListAsync(), "Id", "Nume");
                    ViewBag.Depozite = new SelectList(await _context.Depozite.ToListAsync(), "Id", "Nume");
                    return View(model);
                }
            }

            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;

            var tranzactie = new Tranzactii
            {
                TranzactieId = IdGenerator.GenerateTranzactieId(),
                Tip = model.Tip,
                MarfaId = model.MarfaId,
                Cantitate = model.Cantitate,
                DepozitSursaId = model.DepozitSursaId,
                DepozitDestinatieId = model.DepozitDestinatieId,
                UserId = userId,
                DataTranzactie = DateTime.Now,
                Observatii = model.Observatii,
                ValoareTotala = model.Cantitate * marfa.PretUnitar
            };

            // Actualizează stocul
            switch (model.Tip)
            {
                case TipTranzactie.Intrare:
                    marfa.CapacitateCurenta += model.Cantitate;
                    break;

                case TipTranzactie.Iesire:
                    marfa.CapacitateCurenta -= model.Cantitate;
                    break;

                case TipTranzactie.Transfer:
                    marfa.CapacitateCurenta -= model.Cantitate;
                    // Aici ar trebui să creezi sau actualizezi marfa în depozitul destinație
                    break;
            }

            marfa.DataUltimaModificare = DateTime.Now;

            _context.Tranzactii.Add(tranzactie);
            await _context.SaveChangesAsync();

            await _logService.LogActivityAsync(userId, $"Tranzactie {model.Tip}",
                $"Marfa: {marfa.Name}, Cantitate: {model.Cantitate}",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                model.DepozitSursaId ?? model.DepozitDestinatieId);

            TempData["Success"] = "Tranzactia a fost inregistrata cu succes!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var tranzactie = await _context.Tranzactii
                .Include(t => t.Marfa)
                .Include(t => t.User)
                .Include(t => t.DepozitSursa)
                .Include(t => t.DepozitDestinatie)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tranzactie == null)
            {
                TempData["Error"] = "Tranzactia nu a fost gasita.";
                return RedirectToAction("Index");
            }

            return View(tranzactie);
        }

        [HttpGet]
        public async Task<IActionResult> GetMarfaDetails(int marfaId)
        {
            var marfa = await _context.Marfuri
                .Include(m => m.Depozit)
                .FirstOrDefaultAsync(m => m.Id == marfaId);

            if (marfa == null)
            {
                return Json(new { success = false });
            }

            return Json(new
            {
                success = true,
                nume = marfa.Name,
                sku = marfa.SKU,
                stocDisponibil = marfa.CapacitateCurenta,
                unitateMasura = marfa.UnitateMasura,
                pretUnitar = marfa.PretUnitar,
                depozitNume = marfa.Depozit?.Nume
            });
        }
    }
}