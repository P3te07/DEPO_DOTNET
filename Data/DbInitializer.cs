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

            // 1. Creează SuperAdmin
            var superAdmin = new User
            {
                UserId = "USR20250311ADMIN1",
                Username = "admin",
                PasswordHash = AuthHelper.HashPassword("admin123"),
                Email = "admin@depo.md",
                NumeComplet = "Administrator Sistem",
                Rol = UserRole.SuperAdmin,
                DataCreare = DateTime.Now,
                Activ = true,
                PuncteReward = 0,
                CompanieId = null,
                DepozitId = null
            };

            context.Users.Add(superAdmin);
            context.SaveChanges();

            // 2. Creează companii din Republica Moldova
            var companie1 = new Companie
            {
                CompanieId = IdGenerator.GenerateCompanieId(),
                Nume = "Logistica Pro SRL",
                Cod_Inregistrare = "1012345678901",
                Adresa = "str. Stefan cel Mare 134, Chisinau",
                Telefon = "022-123-456",
                Email = "contact@logisticapro.md",
                DataInregistrare = DateTime.Now.AddMonths(-8),
                Activa = true
            };

            var companie2 = new Companie
            {
                CompanieId = IdGenerator.GenerateCompanieId(),
                Nume = "Transport Moldova SRL",
                Cod_Inregistrare = "1012345678902",
                Adresa = "bd. Dacia 47/2, Chisinau",
                Telefon = "022-789-012",
                Email = "office@transportmd.md",
                DataInregistrare = DateTime.Now.AddMonths(-5),
                Activa = true
            };

            var companie3 = new Companie
            {
                CompanieId = IdGenerator.GenerateCompanieId(),
                Nume = "Distributie Rapida SRL",
                Cod_Inregistrare = "1012345678903",
                Adresa = "str. Alba Iulia 23, Balti",
                Telefon = "0231-45-678",
                Email = "info@distributie.md",
                DataInregistrare = DateTime.Now.AddMonths(-3),
                Activa = true
            };

            context.Companii.AddRange(companie1, companie2, companie3);
            context.SaveChanges();

            // 3. Creează depozite în orașe din Moldova cu coordonate GPS reale
            var depozit1 = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = "Depozit Central Chisinau",
                CompanieId = companie1.Id,
                Adresa = "str. Uzinelor 3, Chisinau",
                Latitudine = 47.0245m,
                Longitudine = 28.8322m,
                CapacitateMaxima = 15000,
                DataDeschidere = DateTime.Now.AddMonths(-7),
                Activ = true
            };

            var depozit2 = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = "Depozit Botanica",
                CompanieId = companie1.Id,
                Adresa = "str. Sarmizegetusa 12, Chisinau",
                Latitudine = 46.9875m,
                Longitudine = 28.8625m,
                CapacitateMaxima = 8000,
                DataDeschidere = DateTime.Now.AddMonths(-4),
                Activ = true
            };

            var depozit3 = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = "Depozit Balti",
                CompanieId = companie2.Id,
                Adresa = "str. Independentei 45, Balti",
                Latitudine = 47.7617m,
                Longitudine = 27.9289m,
                CapacitateMaxima = 10000,
                DataDeschidere = DateTime.Now.AddMonths(-4),
                Activ = true
            };

            var depozit4 = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = "Depozit Cahul",
                CompanieId = companie2.Id,
                Adresa = "str. Republicii 78, Cahul",
                Latitudine = 45.9074m,
                Longitudine = 28.1944m,
                CapacitateMaxima = 6000,
                DataDeschidere = DateTime.Now.AddMonths(-2),
                Activ = true
            };

            var depozit5 = new Depozit
            {
                DepozitId = IdGenerator.GenerateDepozitId(),
                Nume = "Depozit Ungheni",
                CompanieId = companie3.Id,
                Adresa = "str. Vasile Alecsandri 34, Ungheni",
                Latitudine = 47.2108m,
                Longitudine = 27.7989m,
                CapacitateMaxima = 7000,
                DataDeschidere = DateTime.Now.AddMonths(-2),
                Activ = true
            };

            context.Depozite.AddRange(depozit1, depozit2, depozit3, depozit4, depozit5);
            context.SaveChanges();

            // 4. Creează utilizatori
            var director1 = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = "director",
                PasswordHash = AuthHelper.HashPassword("director123"),
                Email = "director@logisticapro.md",
                NumeComplet = "Ion Popescu",
                Rol = UserRole.DirectorCompanie,
                CompanieId = companie1.Id,
                DepozitId = null,
                DataCreare = DateTime.Now.AddMonths(-6),
                Activ = true,
                PuncteReward = 0
            };

            var director2 = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = "director2",
                PasswordHash = AuthHelper.HashPassword("director123"),
                Email = "director@transportmd.md",
                NumeComplet = "Maria Ionescu",
                Rol = UserRole.DirectorCompanie,
                CompanieId = companie2.Id,
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
                Email = "responsabil@logisticapro.md",
                NumeComplet = "Andrei Cazacu",
                Rol = UserRole.ResponsabilDepozit,
                CompanieId = companie1.Id,
                DepozitId = depozit1.Id,
                DataCreare = DateTime.Now.AddMonths(-5),
                Activ = true,
                PuncteReward = 0
            };

            var responsabil2 = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = "responsabil2",
                PasswordHash = AuthHelper.HashPassword("responsabil123"),
                Email = "responsabil.balti@transportmd.md",
                NumeComplet = "Elena Rusu",
                Rol = UserRole.ResponsabilDepozit,
                CompanieId = companie2.Id,
                DepozitId = depozit3.Id,
                DataCreare = DateTime.Now.AddMonths(-3),
                Activ = true,
                PuncteReward = 0
            };

            var muncitor1 = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = "muncitor",
                PasswordHash = AuthHelper.HashPassword("muncitor123"),
                Email = "muncitor@logisticapro.md",
                NumeComplet = "Vasile Cojocaru",
                Rol = UserRole.Muncitor,
                CompanieId = companie1.Id,
                DepozitId = depozit1.Id,
                DataCreare = DateTime.Now.AddMonths(-3),
                Activ = true,
                PuncteReward = 180
            };

            var muncitor2 = new User
            {
                UserId = IdGenerator.GenerateUserId(),
                Username = "muncitor2",
                PasswordHash = AuthHelper.HashPassword("muncitor123"),
                Email = "muncitor2@logisticapro.md",
                NumeComplet = "Nicolae Toma",
                Rol = UserRole.Muncitor,
                CompanieId = companie1.Id,
                DepozitId = depozit1.Id,
                DataCreare = DateTime.Now.AddMonths(-2),
                Activ = true,
                PuncteReward = 95
            };

            context.Users.AddRange(director1, director2, responsabil1, responsabil2, muncitor1, muncitor2);
            context.SaveChanges();

            // 5. Creează marfuri (produse tipice pentru Moldova)
            var marfuri = new List<Marfa>
            {
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Nume = "Vin Rosu Purcari",
                    Descriere = "Vin rosu sec, 750ml",
                    SKU = "VIN-PUR-750",
                    DepozitId = depozit1.Id,
                    CantitateCurenta = 500,
                    UnitateMasura = "buc",
                    PretUnitar = 120.00m,
                    Zona = "A",
                    Etaj = 1,
                    Raft = "R1",
                    Pozitie = "P1",
                    DataAdaugare = DateTime.Now.AddMonths(-2)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Nume = "Cereale Franzeluța",
                    Descriere = "Pachet cereale 500g",
                    SKU = "CER-FRZ-500",
                    DepozitId = depozit1.Id,
                    CantitateCurenta = 1000,
                    UnitateMasura = "buc",
                    PretUnitar = 25.00m,
                    Zona = "A",
                    Etaj = 2,
                    Raft = "R3",
                    Pozitie = "P5",
                    DataAdaugare = DateTime.Now.AddMonths(-1)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Nume = "Ulei de floarea soarelui",
                    Descriere = "Ulei rafinat 1L",
                    SKU = "ULEI-FS-1L",
                    DepozitId = depozit1.Id,
                    CantitateCurenta = 800,
                    UnitateMasura = "buc",
                    PretUnitar = 45.00m,
                    Zona = "B",
                    Etaj = 1,
                    Raft = "R2",
                    Pozitie = "P3",
                    DataAdaugare = DateTime.Now.AddMonths(-1)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Nume = "Faina Alba Neagra",
                    Descriere = "Faina tip 850, 3kg",
                    SKU = "FAINA-AN-3KG",
                    DepozitId = depozit1.Id,
                    CantitateCurenta = 600,
                    UnitateMasura = "buc",
                    PretUnitar = 35.00m,
                    Zona = "B",
                    Etaj = 1,
                    Raft = "R4",
                    Pozitie = "P2",
                    DataAdaugare = DateTime.Now.AddDays(-25)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Nume = "Zahar cristal",
                    Descriere = "Zahar alb cristal 1kg",
                    SKU = "ZAH-CR-1KG",
                    DepozitId = depozit1.Id,
                    CantitateCurenta = 750,
                    UnitateMasura = "buc",
                    PretUnitar = 22.00m,
                    Zona = "B",
                    Etaj = 2,
                    Raft = "R1",
                    Pozitie = "P4",
                    DataAdaugare = DateTime.Now.AddDays(-20)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Nume = "Paste Malai d'Oro",
                    Descriere = "Paste fainoase 500g",
                    SKU = "PST-MDO-500",
                    DepozitId = depozit2.Id,
                    CantitateCurenta = 400,
                    UnitateMasura = "buc",
                    PretUnitar = 18.00m,
                    Zona = "A",
                    Etaj = 1,
                    Raft = "R1",
                    Pozitie = "P1",
                    DataAdaugare = DateTime.Now.AddDays(-15)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Nume = "Conserve de peste",
                    Descriere = "Scrumbii in ulei 240g",
                    SKU = "CONS-SCR-240",
                    DepozitId = depozit3.Id,
                    CantitateCurenta = 350,
                    UnitateMasura = "buc",
                    PretUnitar = 28.00m,
                    Zona = "A",
                    Etaj = 1,
                    Raft = "R2",
                    Pozitie = "P3",
                    DataAdaugare = DateTime.Now.AddDays(-10)
                },
                new Marfa
                {
                    MarfaId = IdGenerator.GenerateMarfaId(),
                    Nume = "Branzeturi locale",
                    Descriere = "Branza de vaci 500g",
                    SKU = "BRZ-VAC-500",
                    DepozitId = depozit3.Id,
                    CantitateCurenta = 200,
                    UnitateMasura = "buc",
                    PretUnitar = 65.00m,
                    Zona = "C",
                    Etaj = 1,
                    Raft = "R1",
                    Pozitie = "P1",
                    DataAdaugare = DateTime.Now.AddDays(-5)
                }
            };

            context.Marfuri.AddRange(marfuri);
            context.SaveChanges();

            // 6. Creează tranzacții
            var tranzactii = new List<Tranzactie>
            {
                new Tranzactie
                {
                    TranzactieId = IdGenerator.GenerateTranzactieId(),
                    Tip = TipTranzactie.Intrare,
                    MarfaId = marfuri[0].Id,
                    Cantitate = 500,
                    DepozitSursaId = null,
                    DepozitDestinatieId = depozit1.Id,
                    UserId = responsabil1.Id,
                    DataTranzactie = DateTime.Now.AddDays(-60),
                    ValoareTotala = 500 * 120.00m,
                    Observatii = "Livrare initiala de vinuri"
                },
                new Tranzactie
                {
                    TranzactieId = IdGenerator.GenerateTranzactieId(),
                    Tip = TipTranzactie.Intrare,
                    MarfaId = marfuri[1].Id,
                    Cantitate = 1000,
                    DepozitSursaId = null,
                    DepozitDestinatieId = depozit1.Id,
                    UserId = responsabil1.Id,
                    DataTranzactie = DateTime.Now.AddDays(-30),
                    ValoareTotala = 1000 * 25.00m,
                    Observatii = "Aprovizionare cereale"
                },
                new Tranzactie
                {
                    TranzactieId = IdGenerator.GenerateTranzactieId(),
                    Tip = TipTranzactie.Iesire,
                    MarfaId = marfuri[0].Id,
                    Cantitate = 100,
                    DepozitSursaId = depozit1.Id,
                    DepozitDestinatieId = null,
                    UserId = muncitor1.Id,
                    DataTranzactie = DateTime.Now.AddDays(-15),
                    ValoareTotala = 100 * 120.00m,
                    Observatii = "Livrare catre magazin nr. 5"
                },
                new Tranzactie
                {
                    TranzactieId = IdGenerator.GenerateTranzactieId(),
                    Tip = TipTranzactie.Intrare,
                    MarfaId = marfuri[2].Id,
                    Cantitate = 800,
                    DepozitSursaId = null,
                    DepozitDestinatieId = depozit1.Id,
                    UserId = responsabil1.Id,
                    DataTranzactie = DateTime.Now.AddDays(-25),
                    ValoareTotala = 800 * 45.00m,
                    Observatii = "Aprovizionare ulei"
                },
                new Tranzactie
                {
                    TranzactieId = IdGenerator.GenerateTranzactieId(),
                    Tip = TipTranzactie.Iesire,
                    MarfaId = marfuri[1].Id,
                    Cantitate = 200,
                    DepozitSursaId = depozit1.Id,
                    DepozitDestinatieId = null,
                    UserId = muncitor1.Id,
                    DataTranzactie = DateTime.Now.AddDays(-5),
                    ValoareTotala = 200 * 25.00m,
                    Observatii = "Livrare la supermarket"
                }
            };

            context.Tranzactii.AddRange(tranzactii);
            context.SaveChanges();

            // 7. Creează sarcini pentru muncitori - TOATE PENTRU ASTAZI
            var sarcini = new List<Sarcina>
            {
                // Sarcini pentru muncitor1 (Vasile Cojocaru) - ASTAZI
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Inventariere zona A",
                    Descriere = "Verifica toate marfurile din zona A, etajele 1 si 2",
                    DataLimita = DateTime.Today.AddHours(17),
                    Finalizata = false,
                    PuncteReward = 50,
                    Prioritate = PrioritateSarcina.Ridicata
                },
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Pregatire comanda #MD-2401",
                    Descriere = "Pregateste pentru livrare: 50 sticle vin Purcari, 100 pachete cereale",
                    DataLimita = DateTime.Today.AddHours(14),
                    Finalizata = false,
                    PuncteReward = 35,
                    Prioritate = PrioritateSarcina.Ridicata
                },
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Verificare stoc ulei",
                    Descriere = "Numara si verifica starea ambalajelor pentru ulei",
                    DataLimita = DateTime.Today.AddHours(16),
                    Finalizata = false,
                    PuncteReward = 25,
                    Prioritate = PrioritateSarcina.Medie
                },
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Reorganizare raft R1",
                    Descriere = "Reorganizeaza produsele pe raftul R1 din zona B",
                    DataLimita = DateTime.Today.AddHours(18),
                    Finalizata = false,
                    PuncteReward = 20,
                    Prioritate = PrioritateSarcina.Scazuta
                },
                // Sarcini viitoare pentru muncitor1
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Receptie marfa noua",
                    Descriere = "Receptioneaza si distribuie marfa noua in depozit",
                    DataLimita = DateTime.Today.AddDays(1).AddHours(10),
                    Finalizata = false,
                    PuncteReward = 40,
                    Prioritate = PrioritateSarcina.Medie
                },
                new Sarcina
                {
                    UserId = muncitor1.Id,
                    Titlu = "Curatenie zona depozitare",
                    Descriere = "Curata zona de depozitare temporara",
                    DataLimita = DateTime.Today.AddDays(2).AddHours(15),
                    Finalizata = false,
                    PuncteReward = 15,
                    Prioritate = PrioritateSarcina.Scazuta
                },
                // Sarcini pentru muncitor2 (Nicolae Toma) - ASTAZI
                new Sarcina
                {
                    UserId = muncitor2.Id,
                    Titlu = "Etichetare produse noi",
                    Descriere = "Eticheteaza toate produsele noi sosite azi dimineata",
                    DataLimita = DateTime.Today.AddHours(15),
                    Finalizata = false,
                    PuncteReward = 30,
                    Prioritate = PrioritateSarcina.Ridicata
                },
                new Sarcina
                {
                    UserId = muncitor2.Id,
                    Titlu = "Verificare data expirare",
                    Descriere = "Verifica datele de expirare pentru toate conservele",
                    DataLimita = DateTime.Today.AddHours(16),
                    Finalizata = false,
                    PuncteReward = 25,
                    Prioritate = PrioritateSarcina.Medie
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
                    AdresaIP = "192.168.1.100",
                    DepozitId = null
                },
                new LogActivitate
                {
                    UserId = responsabil1.Id,
                    Actiune = "Adaugare Marfa",
                    Detalii = "Marfa adaugata: Vin Rosu Purcari",
                    DataOra = DateTime.Now.AddDays(-60),
                    DepozitId = depozit1.Id,
                    AdresaIP = "192.168.1.105"
                },
                new LogActivitate
                {
                    UserId = muncitor1.Id,
                    Actiune = "Tranzactie Iesire",
                    Detalii = "Iesire marfa: 100x Vin Rosu Purcari",
                    DataOra = DateTime.Now.AddDays(-15),
                    DepozitId = depozit1.Id,
                    AdresaIP = "192.168.1.110"
                },
                new LogActivitate
                {
                    UserId = director1.Id,
                    Actiune = "Login",
                    Detalii = "Director autentificat",
                    DataOra = DateTime.Now.AddHours(-5),
                    AdresaIP = "192.168.1.102",
                    DepozitId = null
                }
            };

            context.LogActivitati.AddRange(logs);
            context.SaveChanges();
        }
    }
}