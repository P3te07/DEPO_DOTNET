using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie, UserRole.ResponsabilDepozit)]
    public class RapoarteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RapoarteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Inventar(int? depozitId, DateTime? data)
        {
            var query = _context.Marfuri
                .Include(m => m.Depozit)
                    .ThenInclude(d => d.Companie)
                .AsQueryable();

            if (depozitId.HasValue)
            {
                query = query.Where(m => m.DepozitId == depozitId.Value);
            }

            var marfuri = await query.ToListAsync();

            ViewBag.Depozite = await _context.Depozite.ToListAsync();
            ViewBag.DepozitSelectat = depozitId;
            ViewBag.DataRaport = data ?? DateTime.Now;
            ViewBag.ValoareTotala = marfuri.Sum(m => m.CapacitateCurenta * m.PretUnitar);

            return View(marfuri);
        }

        public async Task<IActionResult> Tranzactii(DateTime? dataStart, DateTime? dataEnd, int? depozitId, TipTranzactie? tip)
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
            }

            if (dataEnd.HasValue)
            {
                query = query.Where(t => t.DataTranzactie <= dataEnd.Value);
            }

            if (depozitId.HasValue)
            {
                query = query.Where(t => t.DepozitSursaId == depozitId || t.DepozitDestinatieId == depozitId);
            }

            if (tip.HasValue)
            {
                query = query.Where(t => t.Tip == tip.Value);
            }

            var tranzactii = await query.OrderByDescending(t => t.DataTranzactie).ToListAsync();

            ViewBag.Depozite = await _context.Depozite.ToListAsync();
            ViewBag.DataStart = dataStart ?? DateTime.Now.AddMonths(-1);
            ViewBag.DataEnd = dataEnd ?? DateTime.Now;
            ViewBag.DepozitSelectat = depozitId;
            ViewBag.TipSelectat = tip;
            ViewBag.ValoareTotala = tranzactii.Sum(t => t.ValoareTotala);
            ViewBag.NumarTranzactii = tranzactii.Count;

            return View(tranzactii);
        }

        public async Task<IActionResult> Activitate(DateTime? dataStart, DateTime? dataEnd, int? userId)
        {
            var query = _context.LogActivitati
                .Include(l => l.User)
                .Include(l => l.Depozit)
                .AsQueryable();

            if (dataStart.HasValue)
            {
                query = query.Where(l => l.DataOra >= dataStart.Value);
            }

            if (dataEnd.HasValue)
            {
                query = query.Where(l => l.DataOra <= dataEnd.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(l => l.UserId == userId.Value);
            }

            var logs = await query
                .OrderByDescending(l => l.DataOra)
                .Take(500)
                .ToListAsync();

            ViewBag.DataStart = dataStart ?? DateTime.Now.AddDays(-7);
            ViewBag.DataEnd = dataEnd ?? DateTime.Now;

            return View(logs);
        }
    }
}