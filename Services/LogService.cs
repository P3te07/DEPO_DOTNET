using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Services
{
    public interface ILogService
    {
        Task LogActivityAsync(int userId, string actiune, string detalii, int? depozitId = null, string adresaIP = null);
        Task<List<LogActivitate>> GetUserLogsAsync(int userId, DateTime? dataStart = null, DateTime? dataEnd = null);
        Task<List<LogActivitate>> GetDepozitLogsAsync(int depozitId, DateTime? dataStart = null, DateTime? dataEnd = null);
    }

    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(int userId, string actiune, string detalii, int? depozitId = null, string adresaIP = null)
        {
            var log = new LogActivitate
            {
                UserId = userId,
                Actiune = actiune,
                Detalii = detalii,
                DataOra = DateTime.Now,
                DepozitId = depozitId,
                AdresaIP = adresaIP
            };

            _context.LogActivitati.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LogActivitate>> GetUserLogsAsync(int userId, DateTime? dataStart = null, DateTime? dataEnd = null)
        {
            var query = _context.LogActivitati
                .Include(l => l.User)
                .Include(l => l.Depozit)
                .Where(l => l.UserId == userId);

            if (dataStart.HasValue)
                query = query.Where(l => l.DataOra >= dataStart.Value);

            if (dataEnd.HasValue)
                query = query.Where(l => l.DataOra <= dataEnd.Value);

            return await query.OrderByDescending(l => l.DataOra).ToListAsync();
        }

        public async Task<List<LogActivitate>> GetDepozitLogsAsync(int depozitId, DateTime? dataStart = null, DateTime? dataEnd = null)
        {
            var query = _context.LogActivitati
                .Include(l => l.User)
                .Include(l => l.Depozit)
                .Where(l => l.DepozitId == depozitId);

            if (dataStart.HasValue)
                query = query.Where(l => l.DataOra >= dataStart.Value);

            if (dataEnd.HasValue)
                query = query.Where(l => l.DataOra <= dataEnd.Value);

            return await query.OrderByDescending(l => l.DataOra).ToListAsync();
        }
    }
}