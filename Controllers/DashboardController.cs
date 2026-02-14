using Proiect_ASPDOTNET.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Models.ViewModels;
using System.Text.Json;

namespace Proiect_ASPDOTNET.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!AuthHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userRole = AuthHelper.GetCurrentUserRole(HttpContext.Session);

            return userRole switch
            {
                UserRole.SuperAdmin => await SuperAdminDashboard(),
                UserRole.DirectorCompanie => await DirectorCompanieDashboard(),
                UserRole.ResponsabilDepozit => await ResponsabilDepozitDashboard(),
                UserRole.Muncitor => await MuncitorDashboard(),
                _ => RedirectToAction("Login", "Auth")
            };
        }

        [AuthorizeRole(UserRole.SuperAdmin)]
        private async Task<IActionResult> SuperAdminDashboard()
        {
            var model = new SuperAdminDashboardViewModel
            {
                TotalCompanii = await _context.Companii.CountAsync(c => c.Active),
                TotalDepozite = await _context.Depozite.CountAsync(d => d.Active),
                TotalUtilizatori = await _context.Users.CountAsync(u => u.Activ),
                TotalTranzactii = await _context.Tranzactii.CountAsync(),
                CompaniiRecente = await _context.Companii
                    .OrderByDescending(c => c.DataInregistrare)
                    .Take(5)
                    .ToListAsync()
            };

            // Calculare crestere companii pe ultimele 12 luni
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            model.CrestereCompanii = new List<CompanieGrowthData>();
            foreach (var month in last12Months)
            {
                var count = await _context.Companii
                    .CountAsync(c => c.DataInregistrare.Year == month.Year &&
                                    c.DataInregistrare.Month == month.Month);

                model.CrestereCompanii.Add(new CompanieGrowthData
                {
                    Luna = month.ToString("MMM yyyy"),
                    NumarCompanii = count
                });
            }

            return View("SuperAdminDashboard", model);
        }

        [AuthorizeRole(UserRole.DirectorCompanie)]
        private async Task<IActionResult> DirectorCompanieDashboard()
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var companieId = ((JsonElement)userData.GetProperty("CompanieId")).GetInt32();

            var companie = await _context.Companii
                .Include(c => c.Depozite)
                .FirstOrDefaultAsync(c => c.Id == companieId);

            var depoziteStats = new List<DepozitStatistici>();

            foreach (var depozit in companie.Depozite)
            {
                var numarTranzactii = await _context.Tranzactii
                    .CountAsync(t => t.DepozitSursaId == depozit.Id || t.DepozitDestinatieId == depozit.Id);

                var valoare = await _context.Marfuri
                    .Where(m => m.DepozitId == depozit.Id)
                    .SumAsync(m => m.CapacitateCurenta * m.PretUnitar);

                var numarMarfuri = await _context.Marfuri
                    .CountAsync(m => m.DepozitId == depozit.Id);

                depoziteStats.Add(new DepozitStatistici
                {
                    Depozit = depozit,
                    NumarTranzactii = numarTranzactii,
                    ValoareDepozit = valoare,
                    NumarMarfuri = numarMarfuri
                });
            }

            var model = new DirectorCompanieDashboardViewModel
            {
                Companie = companie,
                Depozite = depoziteStats,
                ValoareTotala = depoziteStats.Sum(d => d.ValoareDepozit),
                TotalTranzactii = depoziteStats.Sum(d => d.NumarTranzactii)
            };

            return View("DirectorCompanieDashboard", model);
        }

        [AuthorizeRole(UserRole.ResponsabilDepozit)]
        private async Task<IActionResult> ResponsabilDepozitDashboard()
        {
            var userData = AuthHelper.GetCurrentUser(HttpContext.Session);
            var depozitId = ((JsonElement)userData.GetProperty("DepozitId")).GetInt32();

            var depozit = await _context.Depozite
                .Include(d => d.Companie)
                .FirstOrDefaultAsync(d => d.Id == depozitId);

            var marfuri = await _context.Marfuri
                .Where(m => m.DepozitId == depozitId)
                .OrderBy(m => m.Zona)
                .ThenBy(m => m.Etaj)
                .ToListAsync();

            var tranzactiiRecente = await _context.Tranzactii
                .Include(t => t.Marfa)
                .Include(t => t.User)
                .Where(t => t.DepozitSursaId == depozitId || t.DepozitDestinatieId == depozitId)
                .OrderByDescending(t => t.DataTranzactie)
                .Take(10)
                .ToListAsync();

            var model = new ResponsabilDepozitDashboardViewModel
            {
                Depozit = depozit,
                Marfuri = marfuri,
                TotalMarfuri = marfuri.Count,
                ValoareTotala = marfuri.Sum(m => m.CapacitateCurenta * m.PretUnitar),
                TranzactiiRecente = tranzactiiRecente
            };

            return View("ResponsabilDepozitDashboard", model);
        }

        [AuthorizeRole(UserRole.Muncitor)]
        private async Task<IActionResult> MuncitorDashboard()
        {
            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session).Value;

            var user = await _context.Users.FindAsync(userId);

            var today = DateTime.Today;
            var sarciniAzi = await _context.Sarcini
                .Where(s => s.UserId == userId &&
                           s.DataLimita.Date == today &&
                           !s.Finalizata)
                .OrderBy(s => s.Prioritate)
                .ToListAsync();

            var sarciniViitoare = await _context.Sarcini
                .Where(s => s.UserId == userId &&
                           s.DataLimita.Date > today &&
                           !s.Finalizata)
                .OrderBy(s => s.DataLimita)
                .Take(5)
                .ToListAsync();

            var sarciniFinalizateAstazi = await _context.Sarcini
                .CountAsync(s => s.UserId == userId &&
                                s.Finalizata &&
                                s.DataFinalizare.HasValue &&
                                s.DataFinalizare.Value.Date == today);

            var model = new MuncitorDashboardViewModel
            {
                User = user,
                SarciniAzi = sarciniAzi,
                PuncteRewardTotal = user.PuncteReward,
                SarciniFinalizateAstazi = sarciniFinalizateAstazi,
                SarciniViitoare = sarciniViitoare
            };

            return View("MuncitorDashboard", model);
        }
    }
}