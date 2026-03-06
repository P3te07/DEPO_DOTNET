using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Proiect_ASPDOTNET.Helpers
{
    public static class AuthHelper
    {
        private const string SessionKeyUser = "_CurrentUser";

        public static void SetCurrentUser(ISession session, int id, string userId, string username,
      string email, string numeComplet, UserRole rol, int? companieId, int? depozitId)
        {
            var userData = new Dictionary<string, object>
    {
        { "Id", id },
        { "UserId", userId },
        { "Username", username },
        { "Email", email },
        { "NumeComplet", numeComplet },
        { "Rol", rol.ToString() },
        { "CompanieId", companieId ?? 0 },
        { "DepozitId", depozitId ?? 0 }
    };

            session.SetString("_CurrentUser", JsonSerializer.Serialize(userData));
        }

        public static dynamic GetCurrentUser(ISession session)
        {
            var userData = session.GetString(SessionKeyUser);
            if (string.IsNullOrEmpty(userData))
                return null;

            return JsonSerializer.Deserialize<dynamic>(userData);
        }

        public static int? GetCurrentUserId(ISession session)
        {
            var userData = session.GetString(SessionKeyUser);
            if (string.IsNullOrEmpty(userData))
                return null;

            var user = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userData);
            return user["Id"].GetInt32();
        }

        public static UserRole? GetCurrentUserRole(ISession session)
        {
            var userData = session.GetString(SessionKeyUser);
            if (string.IsNullOrEmpty(userData))
                return null;

            var user = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userData);
            var rolString = user["Rol"].GetString();
            return Enum.Parse<UserRole>(rolString);
        }

        public static bool IsAuthenticated(ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString(SessionKeyUser));
        }

        public static void Logout(ISession session)
        {
            session.Remove(SessionKeyUser);
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}