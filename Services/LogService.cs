using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Proiect_ASPDOTNET.Services
{
    public interface ILogService
    {
        Task LogActivityAsync(string actiune, string detalii, int? depozitId = null);
        Task<List<LogActivitate>> GetUserLogsAsync(int userId, DateTime? dataStart = null, DateTime? dataEnd = null);
        Task<List<LogActivitate>> GetDepozitLogsAsync(int depozitId, DateTime? dataStart = null, DateTime? dataEnd = null);
    }

    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActivityAsync(string actiune, string detalii, int? depozitId = null)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;

                // Verifică dacă sesiunea există
                if (session == null)
                {
                    Console.WriteLine("Session is null - cannot log activity");
                    return;
                }

                var userIdString = session.GetString("_UserId");

                // Verifică dacă utilizatorul este autentificat
                if (string.IsNullOrEmpty(userIdString))
                {
                    Console.WriteLine("User not authenticated - cannot log activity");
                    return;
                }

                if (!int.TryParse(userIdString, out int userId))
                {
                    Console.WriteLine($"Invalid user ID format: {userIdString}");
                    return;
                }

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

                var log = new LogActivitate
                {
                    UserId = userId,
                    Actiune = actiune,
                    Detalii = detalii,
                    DataOra = DateTime.Now,
                    AdresaIP = ipAddress,
                    DepozitId = depozitId
                };

                _context.LogActivitati.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error dar nu oprește execuția
                Console.WriteLine($"Error logging activity: {ex.Message}");
            }
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

            return await query
                .OrderByDescending(l => l.DataOra)
                .Take(500)
                .ToListAsync();
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

            return await query
                .OrderByDescending(l => l.DataOra)
                .Take(500)
                .ToListAsync();
        }
    }
}