namespace Proiect_ASPDOTNET.Models.Entities
{
    public class Marfa
    {
        public int Id { get; set; }
        public string MarfaId { get; set; }
        public string Name { get; set; }
        public string Descriere { get; set; }
        public string SKU { get; set; }
        public int DepozitId { get; set; }
        public int CapacitateCurenta { get; set; }
        public string UnitateMasura { get; set; }
        public decimal PretUnitar { get; set; }

        // localizare depozit
        public string Zona { get; set; }
        public int Etaj { get; set; }
        public string Raft { get; set; }
        public string Pozitie { get; set; }

        public virtual DateTime DataAdaugare { get; set; }
        public virtual DateTime? DataUltimaModificare { get; set; }

        //nav

        public virtual Depozit Depozit { get; set; }
        public virtual ICollection<Tranzactii> Tranzactii { get; set; }
    }
}
