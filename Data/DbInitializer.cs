using Proiect_ASPDOTNET.Models.Entities;
using Proiect_ASPDOTNET.Helpers;

namespace Proiect_ASPDOTNET.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Verifică dacă există deja utilizatori
            if (context.Users.Any())
            {
                return; // DB a fost deja seeded
            }

            // Creează SuperAdmin
            var superAdmin = new User
            {
                UserId = "USR20250101ADMIN1",
                Username = "admin",
                PasswordHash = AuthHelper.HashPassword("admin123"),
                Email = "admin@depo.ro",
                NumeComplet = "Super Administrator",
                Rol = UserRole.SuperAdmin,
                DataCreare = DateTime.Now,
                Activ = true,
                PuncteReward = 0
            };

            // Creează o companie de test
            var companie = new Companie
            {
                CompanieId = "CMP20250101TEST01",
                Nume = "Compania Test SRL",
                CUI = "RO12345678",
                Adresa = "Str. Testului nr. 1, Bucuresti",
                Telefon = "0721234567",
                Email = "contact@test.ro",
                DataInregistrare = DateTime.Now,
                Active = true
            };

            // Creează un depozit de test
            var depozit = new Depozit
            {
                DepozitId = "DEP20250101TEST01",
                Nume = "Depozit Central",
                CompanieId = 1, // ID-ul companiei (va fi 1 pentru prima companie)
                Adresa = "Str. Depozitului nr. 10, Bucuresti",
                Latitudine = 44.4268m,
                Longitudine = 26.1025m,
                CapacitateMaxima = 10000,
                DataDeschidere = DateTime.Now,
                Active = true
            };

            // Creează Director Companie
            var director = new User
            {
                UserId = "USR20250101DIR001",
                Username = "director",
                PasswordHash = AuthHelper.HashPassword("director123"),
                Email = "director@test.ro",
                NumeComplet = "Ion Director",
                Rol = UserRole.DirectorCompanie,
                CompanieId = 1,
                DataCreare = DateTime.Now,
                Activ = true,
                PuncteReward = 0
            };

            // Creează Responsabil Depozit
            var responsabil = new User
            {
                UserId = "USR20250101RESP01",
                Username = "responsabil",
                PasswordHash = AuthHelper.HashPassword("responsabil123"),
                Email = "responsabil@test.ro",
                NumeComplet = "Maria Responsabila",
                Rol = UserRole.ResponsabilDepozit,
                CompanieId = 1,
                DepozitId = 1,
                DataCreare = DateTime.Now,
                Activ = true,
                PuncteReward = 0
            };

            // Creează Muncitor
            var muncitor = new User
            {
                UserId = "USR20250101MUNC01",
                Username = "muncitor",
                PasswordHash = AuthHelper.HashPassword("muncitor123"),
                Email = "muncitor@test.ro",
                NumeComplet = "Vasile Muncitorul",
                Rol = UserRole.Muncitor,
                CompanieId = 1,
                DepozitId = 1,
                DataCreare = DateTime.Now,
                Activ = true,
                PuncteReward = 0
            };

            context.Users.Add(superAdmin);
            context.Companii.Add(companie);
            context.SaveChanges(); // Save pentru a genera ID-uri

            context.Depozite.Add(depozit);
            context.SaveChanges();

            context.Users.AddRange(director, responsabil, muncitor);
            context.SaveChanges();
        }
    }
}