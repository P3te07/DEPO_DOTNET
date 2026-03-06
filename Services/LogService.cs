using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(int userId, string actiune, string detalii, string? adresaIP = null, int? depozitId = null)
        {
            var log = new LogActivitate
            {
                UserId = userId,
                Actiune = actiune,
                Detalii = detalii,
                DataOra = DateTime.Now,
                AdresaIP = adresaIP ?? "Unknown",
                DepozitId = depozitId
            };

            _context.LogActivitati.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LogActivitate>> GetUserLogsAsync(int userId)
        {
            return await _context.LogActivitati
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.DataOra)
                .ToListAsync();
        }

        public async Task<List<LogActivitate>> GetDepozitLogsAsync(int depozitId)
        {
            return await _context.LogActivitati
                .Where(l => l.DepozitId == depozitId)
                .OrderByDescending(l => l.DataOra)
                .ToListAsync();
        }
    }
}