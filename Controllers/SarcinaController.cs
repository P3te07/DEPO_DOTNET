using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Controllers
{
    public class SarcinaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public SarcinaController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        [HttpPost]
        public async Task<IActionResult> Finalizeaza(int id)
        {
            var userId = AuthHelper.GetCurrentUserId(HttpContext.Session);

            var sarcina = await _context.Sarcini
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (sarcina == null)
            {
                return Json(new { success = false, message = "Sarcina nu a fost gasita." });
            }

            if (sarcina.Finalizata)
            {
                return Json(new { success = false, message = "Sarcina este deja finalizata." });
            }

            sarcina.Finalizata = true;
            sarcina.DataFinalizare = DateTime.Now;

            // Adauga puncte reward
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.PuncteReward += sarcina.PuncteReward;
            }

            await _context.SaveChangesAsync();

            await _logService.LogActivityAsync(userId.Value, "Finalizare Sarcina",
                $"Sarcina finalizata: {sarcina.Titlu}");

            return Json(new { success = true, puncte = sarcina.PuncteReward });
        }
    }
}