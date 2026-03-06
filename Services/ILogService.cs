namespace Proiect_ASPDOTNET.Services
{
    public interface ILogService
    {
        Task LogActivityAsync(int userId, string actiune, string detalii, string? adresaIP = null, int? depozitId = null);
        Task<List<Models.Entities.LogActivitate>> GetUserLogsAsync(int userId);
        Task<List<Models.Entities.LogActivitate>> GetDepozitLogsAsync(int depozitId);
    }
}