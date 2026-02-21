using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Proiect_ASPDOTNET.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Asigură-te că baza de date este creată
            context.Database.EnsureCreated();

            // Verifică dacă există deja utilizatori
            if (context.Users.Any())
            {
                return; // DB a fost deja seeded
            }

            // 1. Creează SuperAdmin PRIMUL
            var superAdmin = new User
            {
                UserId = "USR20250220ADMIN1",
                Username = "admin",
                PasswordHash = AuthHelper.HashPassword("admin123"),
                Email = "admin@depo.ro",
                NumeComplet = "Super Administrator",
                Rol = UserRole.SuperAdmin,
                DataCreare = DateTime.Now,
                Activ = true,
                PuncteReward = 0,
                CompanieId = null,
                DepozitId = null
            };

            context.Users.Add(superAdmin);
            context.SaveChanges(); // Salvează pentru a genera ID

            // 2. Creează companii
            var companie1 = new Companie
            {
                CompanieId = IdGenerator.GenerateCompanieId(),
                Nume = "Compania Test SRL",
                CUI = "RO12345678",
                Adresa = "Str. Testului nr. 1, Bucuresti",
                Telefon = "0721234567",
                Email = "contact@test.ro",
                DataInregistrare = DateTime.Now.AddMonths(-6),
                Active = true
            };

            var companie2 = new Companie
            {
                CompanieId = IdGenerator.GenerateCompanieId(),
                Nume = "Logistica Pro SRL",
                CUI = "RO87654321",
                Adresa = "Bd. Unirii nr. 50, Cluj-Napoca",
                Telefon = "0731234567",
                Email = "info@logisticapro.ro",
                DataInregistrare = DateTime.Now.AddMonths(-3),
                Active = true
            };

            context.Companii.AddRange(companie1, companie2);
            context.SaveChanges();

            // 3. Creează depozite
            var depozit1 = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = "Depozit Central Bucuresti",
                CompanieId = companie1.Id,
                Adresa = "Str. Depozitului nr. 10, Bucuresti",
                Latitudine = 44.4268m,
                Longitudine = 26.1025m,
                CapacitateMaxima = 10000,
                DataDeschidere = DateTime.Now.AddMonths(-5),
                Active = true
            };

            var depozit2 = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = "Depozit Pipera",
                CompanieId = companie1.Id,
                Adresa = "Sos. Pipera nr. 100, Bucuresti",
                Latitudine = 44.4793m,
                Longitudine = 26.1257m,
                CapacitateMaxima = 5000,
                DataDeschidere = DateTime.Now.AddMonths(-2),
                Active = true
            };

            var depozit3 = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = "Depozit Cluj",
                CompanieId = companie2.Id,
                Adresa = "Str. Industriei nr. 25, Cluj-Napoca",
                Latitudine = 46.7712m,
                Longitudine = 23.6236m,
                CapacitateMaxima = 8000,
                DataDeschidere = DateTime.Now.AddMonths(-2),
                Active = true
            };

            context.Depozite.AddRange(depozit1, depozit2, depozit3);
            context.SaveChanges();

            // 4. Creează utilizatori
            var director1 = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = "director",
                PasswordHash = AuthHelper.HashPassword("director123"),
                Email = "director@test.ro",
                NumeComplet = "Ion Director",
                Rol = UserRole.DirectorCompanie,
                CompanieId = companie1.Id,
                DepozitId = null,
                DataCreare = DateTime.Now.AddMonths(-4),
                Activ = true,
                PuncteReward = 0
            };

            var responsabil1 = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = "responsabil",
                PasswordHash = AuthHelper.HashPassword("responsabil123"),
                Email = "responsabil@test.ro",
                NumeComplet = "Maria Responsabila",
                Rol = UserRole.ResponsabilDepozit,
                CompanieId = companie1.Id,
                DepozitId = depozit1.Id,
                DataCreare = DateTime.Now.AddMonths(-3),
                Activ = true,
                PuncteReward = 0
            };

            var muncitor1 = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = "muncitor",
                PasswordHash = AuthHelper.HashPassword("muncitor123"),
                Email = "muncitor@test.ro",
                NumeComplet = "Vasile Muncitorul",
                Rol = UserRole.Muncitor,
                CompanieId = companie1.Id,
                DepozitId = depozit1.Id,
                DataCreare = DateTime.Now.AddMonths(-2),
                Activ = true,
                PuncteReward = 150
            };

            context.Users.AddRange(director1, responsabil1, muncitor1);
            context.SaveChanges();

            // 5. Creează marfuri
            var marfuri = new List<Marfa>
            {
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Name = "Laptop Dell Latitude 5520",
                    Descriere = "Laptop profesional pentru business",
                    SKU = "DELL-LAT-5520",
                    DepozitId = depozit1.Id,
                    CapacitateCurenta = 50,
                    UnitateMasura = "buc",
                    PretUnitar = 3500.00m,
                    Zona = "A",
                    Etaj = 2,
                    Raft = "R5",
                    Pozitie = "P3",
                    DataAdaugare = DateTime.Now.AddMonths(-1)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Name = "Monitor LG 27 inch",
                    Descriere = "Monitor IPS Full HD",
                    SKU = "LG-MON-27FHD",
                    DepozitId = depozit1.Id,
                    CapacitateCurenta = 100,
                    UnitateMasura = "buc",
                    PretUnitar = 800.00m,
                    Zona = "A",
                    Etaj = 1,
                    Raft = "R3",
                    Pozitie = "P1",
                    DataAdaugare = DateTime.Now.AddMonths(-1)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Name = "Mouse Logitech MX Master 3",
                    Descriere = "Mouse wireless ergonomic",
                    SKU = "LOG-MX-M3",
                    DepozitId = depozit1.Id,
                    CapacitateCurenta = 200,
                    UnitateMasura = "buc",
                    PretUnitar = 350.00m,
                    Zona = "B",
                    Etaj = 1,
                    Raft = "R1",
                    Pozitie = "P5",
                    DataAdaugare = DateTime.Now.AddDays(-20)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Name = "Tastatura mecanica Keychron K2",
                    Descriere = "Tastatura mecanica wireless",
                    SKU = "KEY-K2-MEC",
                    DepozitId = depozit1.Id,
                    CapacitateCurenta = 75,
                    UnitateMasura = "buc",
                    PretUnitar = 450.00m,
                    Zona = "B",
                    Etaj = 1,
                    Raft = "R2",
                    Pozitie = "P3",
                    DataAdaugare = DateTime.Now.AddDays(-15)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Name = "Scaun ergonomic Herman Miller",
                    Descriere = "Scaun de birou premium",
                    SKU = "HM-CHAIR-ERG",
                    DepozitId = depozit2.Id,
                    CapacitateCurenta = 30,
                    UnitateMasura = "buc",
                    PretUnitar = 5000.00m,
                    Zona = "A",
                    Etaj = 1,
                    Raft = "R1",
                    Pozitie = "P1",
                    DataAdaugare = DateTime.Now.AddDays(-10)
                }
            };

            context.Marfuri.AddRange(marfuri);
            context.SaveChanges();

            // 6. Creează tranzacții
            var tranzactii = new List<Tranzactii>
            {
                new Tranzactii
                {
                    TranzactieId = IdGenerator.GenerateTranzactieId(),
                    Tip = TipTranzactie.Intrare,
                    MarfaId = marfuri[0].Id,
                    Cantitate = 50,
                    DepozitSursaId = null,
                    DepozitDestinatieId = depozit1.Id,
                    UserId = responsabil1.Id,
                    DataTranzactie = DateTime.Now.AddDays(-30),
                    ValoareTotala = 50 * 3500.00m,
                    Observatii = "Intrare initiala stoc"
                },
                new Tranzactii
                {
                    TranzactieId = IdGenerator.GenerateTranzactieId(),
                    Tip = TipTranzactie.Intrare,
                    MarfaId = marfuri[1].Id,
                    Cantitate = 100,
                    DepozitSursaId = null,
                    DepozitDestinatieId = depozit1.Id,
                    UserId = responsabil1.Id,
                    DataTranzactie = DateTime.Now.AddDays(-25),
                    ValoareTotala = 100 * 800.00m,
                    Observatii = "Intrare monitoare"
                },
                new Tranzactii
                {
                    TranzactieId = IdGenerator.GenerateTranzactieId(),
                    Tip = TipTranzactie.Iesire,
                    MarfaId = marfuri[0].Id,
                    Cantitate = 10,
                    DepozitSursaId = depozit1.Id,
                    DepozitDestinatieId = null,
                    UserId = muncitor1.Id,
                    DataTranzactie = DateTime.Now.AddDays(-5),
                    ValoareTotala = 10 * 3500.00m,
                    Observatii = "Livrare catre client"
                }
            };

            context.Tranzactii.AddRange(tranzactii);
            context.SaveChanges();

            // 7. Creează sarcini pentru muncitor
            var sarcini = new List<Sarcina>
            {
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Inventariere Zona A",
                    Descriere = "Verifica toate marfurile din zona A, etajul 1",
                    DataLimita = DateTime.Today.AddHours(18),
                    Finalizata = false,
                    PuncteReward = 50,
                    Prioritate = PrioritateSarcina.Ridicata
                },
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Pregatire comanda #1234",
                    Descriere = "Pregateste comanda pentru livrare: 5x Laptopuri, 10x Monitoare",
                    DataLimita = DateTime.Today.AddHours(14),
                    Finalizata = false,
                    PuncteReward = 30,
                    Prioritate = PrioritateSarcina.Medie
                },
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Organizare raft R5",
                    Descriere = "Reorganizeaza produsele pe raftul R5",
                    DataLimita = DateTime.Today.AddDays(1),
                    Finalizata = false,
                    PuncteReward = 20,
                    Prioritate = PrioritateSarcina.Scazuta
                }
            };

            context.Sarcini.AddRange(sarcini);
            context.SaveChanges();

            // 8. Creează log-uri de activitate
            var logs = new List<LogActivitate>
            {
                new LogActivitate
                {
                    UserId = superAdmin.Id,
                    Actiune = "Login",
                    Detalii = "Utilizator autentificat",
                    DataOra = DateTime.Now.AddHours(-2),
                    AdresaIP = "127.0.0.1",
                    DepozitId = null
                },
                new LogActivitate
                {
                    UserId = responsabil1.Id,
                    Actiune = "Adaugare Marfa",
                    Detalii = "Marfa adaugata: Laptop Dell Latitude 5520",
                    DataOra = DateTime.Now.AddDays(-30),
                    DepozitId = depozit1.Id,
                    AdresaIP = "127.0.0.1"
                },
                new LogActivitate
                {
                    UserId = muncitor1.Id,
                    Actiune = "Tranzactie Iesire",
                    Detalii = "Iesire marfa: 10x Laptop Dell Latitude 5520",
                    DataOra = DateTime.Now.AddDays(-5),
                    DepozitId = depozit1.Id,
                    AdresaIP = "127.0.0.1"
                }
            };

            context.LogActivitati.AddRange(logs);
            context.SaveChanges();
        }
    }
}
