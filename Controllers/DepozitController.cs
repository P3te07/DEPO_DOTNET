using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.SuperAdmin, UserRole.DirectorCompanie)]
    public class DepozitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepozitController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var depozite = await _context.Depozite
                .Include(d => d.Companie)
                .ToListAsync();
            return View(depozite);
        }

        public async Task<IActionResult> Details(int id)
        {
            var depozit = await _context.Depozite
                .Include(d => d.Companie)
                .Include(d => d.Marfuri)
                .Include(d => d.Utilizatori)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (depozit == null)
            {
                TempData["Error"] = "Depozitul nu a fost gasit.";
                return RedirectToAction("Index");
            }

            return View(depozit);
        }

        
    }
}