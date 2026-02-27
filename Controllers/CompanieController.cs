using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Filters;
using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Controllers
{
    [AuthorizeRole(UserRole.SuperAdmin)]
    public class CompanieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanieController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var companii = await _context.Companii
                .Include(c => c.Depozite)
                .OrderByDescending(c => c.DataInregistrare)
                .ToListAsync();
            return View(companii);
        }
    }
}