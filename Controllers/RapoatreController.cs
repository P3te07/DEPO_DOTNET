using Proiect_ASPDOTNET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using System.Text.Json;

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
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            IQueryable<Marfa> query = _context.Marfuri.Include(m => m.Depozit);

            if (userRole == UserRole.ResponsabilDepozit)
            {
                var userDepozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
                query = query.Where(m => m.DepozitId == userDepozitId);
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                query = query.Where(m => m.Depozit.CompanieId == companieId);
            }

            if (depozitId.HasValue)
            {
                query = query.Where(m => m.DepozitId == depozitId.Value);
            }

            var marfuri = await query.ToListAsync();

            // Load depozite for dropdown
            await LoadDepoziteForFilter(userRole, userData);

            ViewBag.DepozitSelectat = depozitId;
            ViewBag.DataRaport = data ?? DateTime.Now;
            ViewBag.ValoareTotala = marfuri.Sum(m => m.CapacitateCurenta * m.PretUnitar);

            return View(marfuri);
        }

        public async Task<IActionResult> Tranzactii(DateTime? dataStart, DateTime? dataEnd, int? depozitId, TipTranzactie? tip)
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            IQueryable<Tranzactii> query = _context.Tranzactii
                .Include(t => t.Marfa)
                .Include(t => t.DepozitSursa)
                .Include(t => t.DepozitDestinatie)
                .Include(t => t.User);

            if (userRole == UserRole.ResponsabilDepozit)
            {
                var userDepozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
                query = query.Where(t => t.DepozitSursaId == userDepozitId || t.DepozitDestinatieId == userDepozitId);
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                query = query.Where(t =>
                    (t.DepozitSursa != null && t.DepozitSursa.CompanieId == companieId) ||
                    (t.DepozitDestinatie != null && t.DepozitDestinatie.CompanieId == companieId));
            }

            if (dataStart.HasValue)
                query = query.Where(t => t.DataTranzactie >= dataStart.Value);

            if (dataEnd.HasValue)
                query = query.Where(t => t.DataTranzactie <= dataEnd.Value);

            if (depozitId.HasValue)
                query = query.Where(t => t.DepozitSursaId == depozitId || t.DepozitDestinatieId == depozitId);

            if (tip.HasValue)
                query = query.Where(t => t.Tip == tip.Value);

            var tranzactii = await query.OrderByDescending(t => t.DataTranzactie).ToListAsync();

            await LoadDepoziteForFilter(userRole, userData);

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
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            IQueryable<LogActivitate> query = _context.LogActivitati
                .Include(l => l.User)
                .Include(l => l.Depozit);

            if (userRole == UserRole.ResponsabilDepozit)
            {
                var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();
                query = query.Where(l => l.DepozitId == depozitId);
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                query = query.Where(l => l.User.CompanieId == companieId);
            }

            if (dataStart.HasValue)
                query = query.Where(l => l.DataOra >= dataStart.Value);

            if (dataEnd.HasValue)
                query = query.Where(l => l.DataOra <= dataEnd.Value);

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

            var logs = await query.OrderByDescending(l => l.DataOra).Take(500).ToListAsync();

            ViewBag.DataStart = dataStart ?? DateTime.Now.AddDays(-7);
            ViewBag.DataEnd = dataEnd ?? DateTime.Now;
            ViewBag.UserSelectat = userId;

            return View(logs);
        }

        private async Task LoadDepoziteForFilter(UserRole? userRole, dynamic userData)
        {
            if (userRole == UserRole.SuperAdmin)
            {
                ViewBag.Depozite = await _context.Depozite.Where(d => d.Active).ToListAsync();
            }
            else if (userRole == UserRole.DirectorCompanie)
            {
                var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();
                ViewBag.Depozite = await _context.Depozite
                    .Where(d => d.CompanieId == companieId && d.Active)
                    .ToListAsync();
            }
        }
    }
}