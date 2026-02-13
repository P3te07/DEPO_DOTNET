using Proiect_ASPDOTNET.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Proiect_ASPDOTNET.Helpers
{
    public static class AuthHelper
    {
        private const string SessionKeyUser = "_CurrentUser";

        public static void SetCurrentUser(ISession session, User user)
        {
            var userData = new
            {
                user.Id,
                user.UserId,
                user.Username,
                user.NumeComplet,
                user.Email,
                Rol = user.Rol.ToString(),
                user.CompanieId,
                user.DepozitId
            };
            session.SetString(SessionKeyUser, JsonSerializer.Serialize(userData));
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