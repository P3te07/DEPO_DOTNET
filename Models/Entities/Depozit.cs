namespace Proiect_ASPDOTNET.Models.Entities
{
    public class Depozit
    {
        public int Id { get; set; }
        public string DepozitId { get; set; }
        public string Nume { get; set; }
        public int CompanieId { get; set; }
        public string Adresa { get; set; }
        public decimal Latitudine { get; set; }
        public decimal Longitudine { get; set; }
        public int CapacitateMaxima { get; set; }
        public DateTime DataDeschidere { get; set; }
        public bool Active { get; set; }

        //nav

        public virtual Companie Companie { get; set; }
        public virtual ICollection<User> Utilizatori { get; set; }
        public virtual ICollection<Marfa> Marfuri { get; set; }
        public virtual ICollection<Tranzactie> TranzactiiSursa { get; set; }
        public virtual ICollection<Tranzactie> TranzactiiDestinatie { get; set; }
    }
}
