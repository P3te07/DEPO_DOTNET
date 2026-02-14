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
    [AuthorizeRole(UserRole.ResponsabilDepozit, UserRole.Muncitor, UserRole.DirectorCompanie, UserRole.SuperAdmin)]
    public class TranzactiiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public TranzactiiController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index(DateTime? dataStart, DateTime? dataEnd, TipTranzactie? tip)
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            IQueryable<Tranzactii> query = _context.Tranzactii
                .Include(t => t.Marfa)
                .Include(t => t.DepozitSursa)
                .Include(t => t.DepozitDestinatie)
                .Include(t => t.User);

            // Filter by user role
            if (userRole == UserRole.ResponsabilDepozit || userRole == UserRole.Muncitor)
            {
                var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
                query = query.Where(t => t.DepozitSursaId == depozitId || t.DepozitDestinatieId == depozitId);
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                query = query.Where(t =>
                    (t.DepozitSursa != null && t.DepozitSursa.CompanieId == companieId) ||
                    (t.DepozitDestinatie != null && t.DepozitDestinatie.CompanieId == companieId));
            }

            // Apply filters
            if (dataStart.HasValue)
                query = query.Where(t => t.DataTranzactie >= dataStart.Value);

            if (dataEnd.HasValue)
                query = query.Where(t => t.DataTranzactie <= dataEnd.Value);

            if (tip.HasValue)
                query = query.Where(t => t.Tip == tip.Value);

            var tranzactii = await query
                .OrderByDescending(t => t.DataTranzactie)
                .ToListAsync();

            ViewBag.DataStart = dataStart;
            ViewBag.DataEnd = dataEnd;
            ViewBag.TipSelectat = tip;

            return View(tranzactii);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (userRole == UserRole.ResponsabilDepozit || userRole == UserRole.Muncitor)
            {
                var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();

                ViewBag.Marfuri = new SelectList(
                    await _context.Marfuri
                        .Where(m => m.DepozitId == depozitId)
                        .ToListAsync(),
                    "Id", "Nume");

                ViewBag.DepozitCurent = depozitId;
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();

                ViewBag.Marfuri = new SelectList(
                    await _context.Marfuri
                        .Include(m => m.Depozit)
                        .Where(m => m.Depozit.CompanieId == companieId)
                        .ToListAsync(),
                    "Id", "Nume");

                ViewBag.Depozite = new SelectList(
                    await _context.Depozite
                        .Where(d => d.CompanieId == companieId && d.Active)
                        .ToListAsync(),
                    "Id", "Nume");
            }
            else // SuperAdmin
            {
                ViewBag.Marfuri = new SelectList(await _context.Marfuri.ToListAsync(), "Id", "Nume");
                ViewBag.Depozite = new SelectList(await _context.Depozite.Where(d => d.Active).ToListAsync(), "Id", "Nume");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TranzactieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectLists(model.MarfaId);
                return View(model);
            }

            var marfa = await _context.Marfuri.FindAsync(model.MarfaId);
            if (marfa == null)
            {
                ModelState.AddModelError("", "Marfa negasita");
                await LoadSelectLists(model.MarfaId);
                return View(model);
            }

            // Validate transaction based on type
            if (model.Tip == TipTranzactie.Intrare)
            {
                if (!model.DepozitDestinatieId.HasValue)
                {
                    ModelState.AddModelError("DepozitDestinatieId", "Depozitul destinatie este obligatoriu pentru intrari");
                    await LoadSelectLists(model.MarfaId);
                    return View(model);
                }
            }
            else if (model.Tip == TipTranzactie.Iesire)
            {
                if (!model.DepozitSursaId.HasValue)
                {
                    ModelState.AddModelError("DepozitSursaId", "Depozitul sursa este obligatoriu pentru iesiri");
                    await LoadSelectLists(model.MarfaId);
                    return View(model);
                }

                if (marfa.CapacitateCurenta < model.Cantitate)
                {
                    ModelState.AddModelError("Cantitate", $"Stoc insuficient. Disponibil: {marfa.CapacitateCurenta}");
                    await LoadSelectLists(model.MarfaId);
                    return View(model);
                }
            }
            else if (model.Tip == TipTranzactie.Transfer)
            {
                if (!model.DepozitSursaId.HasValue || !model.DepozitDestinatieId.HasValue)
                {
                    ModelState.AddModelError("", "Ambele depozite sunt obligatorii pentru transferuri");
                    await LoadSelectLists(model.MarfaId);
                    return View(model);
                }

                if (model.DepozitSursaId == model.DepozitDestinatieId)
                {
                    ModelState.AddModelError("", "Depozitul sursa si destinatie nu pot fi identice");
                    await LoadSelectLists(model.MarfaId);
                    return View(model);
                }

                if (marfa.CapacitateCurenta < model.Cantitate)
                {
                    ModelState.AddModelError("Cantitate", $"Stoc insuficient. Disponibil: {marfa.CapacitateCurenta}");
                    await LoadSelectLists(model.MarfaId);
                    return View(model);
                }
            }

            var tranzactie = new Tranzactii
            {
                TranzactieId = IdGenerator.GenerateTranzactieId(),
                Tip = model.Tip,
                MarfaId = model.MarfaId,
                Cantitate = model.Cantitate,
                DepozitSursaId = model.DepozitSursaId,
                DepozitDestinatieId = model.DepozitDestinatieId,
                UserId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value,
                DataTranzactie = DateTime.Now,
                Observatii = model.Observatii,
                ValoareTotala = model.Cantitate * marfa.PretUnitar
            };

            // Update stock based on transaction type
            if (model.Tip == TipTranzactie.Intrare)
            {
                marfa.CapacitateCurenta += model.Cantitate;
            }
            else if (model.Tip == TipTranzactie.Iesire)
            {
                marfa.CapacitateCurenta -= model.Cantitate;
            }
            else if (model.Tip == TipTranzactie.Transfer)
            {
                // Decrease from source
                marfa.CapacitateCurenta -= model.Cantitate;

                // Create or update in destination warehouse
                var marfaDestinatie = await _context.Marfuri
                    .FirstOrDefaultAsync(m => m.SKU == marfa.SKU && m.DepozitId == model.DepozitDestinatieId);

                if (marfaDestinatie != null)
                {
                    marfaDestinatie.CapacitateCurenta += model.Cantitate;
                }
                else
                {
                    // Create new entry in destination warehouse
                    marfaDestinatie = new Marfa
                    {
                        MarfaId = IdGenerator.GenerateMarfaId(),
                        Name = marfa.Name,
                        Descriere = marfa.Descriere,
                        SKU = marfa.SKU,
                        DepozitId = model.DepozitDestinatieId.Value,
                        CapacitateCurenta = model.Cantitate,
                        UnitateMasura = marfa.UnitateMasura,
                        PretUnitar = marfa.PretUnitar,
                        Zona = "A", // Default zone
                        Etaj = 1,
                        Raft = "1",
                        Pozitie = "1",
                        DataAdaugare = DateTime.Now
                    };
                    _context.Marfuri.Add(marfaDestinatie);
                }
            }

            marfa.DataUltimaModificare = DateTime.Now;

            _context.Tranzactii.Add(tranzactie);
            await _context.SaveChangesAsync();

            // Log the transaction
            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;
            var logDetalii = $"Tranzactie {model.Tip}: {marfa.Name} (SKU: {marfa.SKU}), Cantitate: {model.Cantitate}";

            if (model.DepozitSursaId.HasValue)
            {
                var depozitSursa = await _context.Depozite.FindAsync(model.DepozitSursaId.Value);
                logDetalii += $", Sursa: {depozitSursa?.Nume}";
            }

            if (model.DepozitDestinatieId.HasValue)
            {
                var depozitDest = await _context.Depozite.FindAsync(model.DepozitDestinatieId.Value);
                logDetalii += $", Destinatie: {depozitDest?.Nume}";
            }

            await _logService.LogActivityAsync(userId, "Tranzactie Noua", logDetalii, marfa.DepozitId);

            TempData["Success"] = "Tranzactie inregistrata cu succes!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var tranzactie = await _context.Tranzactii
                .Include(t => t.Marfa)
                .Include(t => t.DepozitSursa)
                .Include(t => t.DepozitDestinatie)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tranzactie == null)
            {
                return NotFound();
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
                depozitId = marfa.DepozitId,
                depozitNume = marfa.Depozit?.Nume
            });
        }

        private async Task LoadSelectLists(int? marfaId = null)
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            if (userRole == UserRole.ResponsabilDepozit || userRole == UserRole.Muncitor)
            {
                var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();

                ViewBag.Marfuri = new SelectList(
                    await _context.Marfuri
                        .Where(m => m.DepozitId == depozitId)
                        .ToListAsync(),
                    "Id", "Nume", marfaId);

                ViewBag.DepozitCurent = depozitId;
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();

                ViewBag.Marfuri = new SelectList(
                    await _context.Marfuri
                        .Include(m => m.Depozit)
                        .Where(m => m.Depozit.CompanieId == companieId)
                        .ToListAsync(),
                    "Id", "Nume", marfaId);

                ViewBag.Depozite = new SelectList(
                    await _context.Depozite
                        .Where(d => d.CompanieId == companieId && d.Active)
                        .ToListAsync(),
                    "Id", "Nume");
            }
            else
            {
                ViewBag.Marfuri = new SelectList(await _context.Marfuri.ToListAsync(), "Id", "Nume", marfaId);
                ViewBag.Depozite = new SelectList(await _context.Depozite.Where(d => d.Active).ToListAsync(), "Id", "Nume");
            }
        }
    }
}