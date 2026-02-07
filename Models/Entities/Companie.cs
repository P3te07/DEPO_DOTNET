using System.Globalization;

namespace Proiect_ASPDOTNET.Models.Entities
{
    public class Companie
    {
        public int Id { get; set; }
        public string CompanieId { get; set; }
        public string Nume { get; set; }
        public string CUI { get; set; }
        public string Adresa { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public DateTime DataInregistrare { get; set; }
        public bool Active { get; set; }

        //nav

        public virtual ICollection<Depozit> Depozite { get; set; }
        public virtual ICollection<User> Utilizatori { get; set; }

    }
}
