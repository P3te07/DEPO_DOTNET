using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Proiect_ASPDOTNET.Data;
using Proiect_ASPDOTNET.Helpers;
using Proiect_ASPDOTNET.Models.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace Proiect_ASPDOTNET.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class ApiTestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiTestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/test/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    u.Username,
                    u.NumeComplet,
                    u.Email,
                    Rol = u.Rol.ToString(),
                    Companie = u.Companie != null ? u.Companie.Nume : null,
                    Depozit = u.Depozit != null ? u.Depozit.Nume : null,
                    u.Activ
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/test/companii
        [HttpGet("companii")]
        public async Task<IActionResult> GetCompanii()
        {
            var companii = await _context.Companii
                .Include(c => c.Depozite)
                .Select(c => new
                {
                    c.Id,
                    c.CompanieId,
                    c.Nume,
                    c.CUI,
                    c.Email,
                    NumarDepozite = c.Depozite.Count,
                    c.Active
                })
                .ToListAsync();

            return Ok(companii);
        }

        // GET: api/test/depozite
        [HttpGet("depozite")]
        public async Task<IActionResult> GetDepozite()
        {
            var depozite = await _context.Depozite
                .Include(d => d.Companie)
                .Select(d => new
                {
                    d.Id,
                    d.DepozitId,
                    d.Nume,
                    d.Adresa,
                    Companie = d.Companie.Nume,
                    d.Latitudine,
                    d.Longitudine,
                    d.Active
                })
                .ToListAsync();

            return Ok(depozite);
        }

        // GET: api/test/marfuri
        [HttpGet("marfuri")]
        public async Task<IActionResult> GetMarfuri()
        {
            var marfuri = await _context.Marfuri
                .Include(m => m.Depozit)
                .Select(m => new
                {
                    m.Id,
                    m.MarfaId,
                    m.Name,
                    m.SKU,
                    m.CapacitateCurenta,
                    m.UnitateMasura,
                    m.PretUnitar,
                    ValoareTotala = m.CapacitateCurenta * m.PretUnitar,
                    Depozit = m.Depozit.Nume,
                    Localizare = $"Zona {m.Zona}, Etaj {m.Etaj}, Raft {m.Raft}"
                })
                .ToListAsync();

            return Ok(marfuri);
        }

        // GET: api/test/tranzactii
        [HttpGet("tranzactii")]
        public async Task<IActionResult> GetTranzactii()
        {
            var tranzactii = await _context.Tranzactii
                .Include(t => t.Marfa)
                .Include(t => t.User)
                .Include(t => t.DepozitSursa)
                .Include(t => t.DepozitDestinatie)
                .OrderByDescending(t => t.DataTranzactie)
                .Take(50)
                .Select(t => new
                {
                    t.Id,
                    t.TranzactieId,
                    Tip = t.Tip.ToString(),
                    Marfa = t.Marfa.Name,
                    t.Cantitate,
                    t.ValoareTotala,
                    DepozitSursa = t.DepozitSursa != null ? t.DepozitSursa.Nume : null,
                    DepozitDestinatie = t.DepozitDestinatie != null ? t.DepozitDestinatie.Nume : null,
                    Utilizator = t.User.NumeComplet,
                    t.DataTranzactie
                })
                .ToListAsync();

            return Ok(tranzactii);
        }

        // POST: api/test/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Companie)
                .Include(u => u.Depozit)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Activ);

            if (user == null || !AuthHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Username sau parola incorecte" });
            }

            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    user.Id,
                    user.UserId,
                    user.Username,
                    user.NumeComplet,
                    user.Email,
                    Rol = user.Rol.ToString(),
                    Companie = user.Companie?.Nume,
                    Depozit = user.Depozit?.Nume
                }
            });
        }

        // POST: api/test/companie
        [HttpPost("companie")]
        public async Task<IActionResult> CreateCompanie([FromBody] CreateCompanieRequest request)
        {
            var companie = new Companie
            {
                CompanieId = IdGenerator.GenerateCompanieId(),
                Nume = request.Nume,
                CUI = request.CUI,
                Adresa = request.Adresa,
                Telefon = request.Telefon,
                Email = request.Email,
                DataInregistrare = DateTime.Now,
                Active = true
            };

            _context.Companii.Add(companie);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Companie creata cu succes",
                companie = new
                {
                    companie.Id,
                    companie.CompanieId,
                    companie.Nume,
                    companie.CUI
                }
            });
        }

        // POST: api/test/marfa
        [HttpPost("marfa")]
        public async Task<IActionResult> CreateMarfa([FromBody] CreateMarfaRequest request)
        {
            var marfa = new Marfa
            {
                MarfaId = IdGenerator.GenerateMarfaId(),
                Name = request.Nume,
                Descriere = request.Descriere,
                SKU = request.SKU,
                DepozitId = request.DepozitId,
                CapacitateCurenta = request.Cantitate,
                UnitateMasura = request.UnitateMasura,
                PretUnitar = request.PretUnitar,
                Zona = request.Zona ?? "A",
                Etaj = request.Etaj,
                Raft = request.Raft ?? "1",
                Pozitie = request.Pozitie ?? "1",
                DataAdaugare = DateTime.Now
            };

            _context.Marfuri.Add(marfa);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Marfa adaugata cu succes",
                marfa = new
                {
                    marfa.Id,
                    marfa.MarfaId,
                    marfa.Name,
                    marfa.SKU,
                    marfa.CapacitateCurenta
                }
            });
        }

        // Models for requests
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class CreateCompanieRequest
        {
            public string Nume { get; set; }
            public string CUI { get; set; }
            public string Adresa { get; set; }
            public string Telefon { get; set; }
            public string Email { get; set; }
        }

        public class CreateMarfaRequest
        {
            public string Nume { get; set; }
            public string Descriere { get; set; }
            public string SKU { get; set; }
            public int DepozitId { get; set; }
            public int Cantitate { get; set; }
            public string UnitateMasura { get; set; }
            public decimal PretUnitar { get; set; }
            public string Zona { get; set; }
            public int Etaj { get; set; }
            public string Raft { get; set; }
            public string Pozitie { get; set; }
        }
    }
}