namespace Proiect_ASPDOTNET.Models.Entities
{
    public class Tranzactii
    {
        public int Id { get; set; }
        public string TranzactieId { get; set; }
        public TipTranzactie Tip { get; set; }
        public int MarfaId { get; set; }
        public int Cantitate { get; set; }
        public int? DepozitSursaId { get; set; }
        public int? DepozitDestinatieId { get; set; }
        public int UserId { get; set; }
        public DateTime DataTranzactie { get; set; }
        public string Observatii { get; set; }
        public decimal ValoareToatala { get; set; }

        //nav
        public virtual Marfa Marfa { get; set; }
        public virtual Depozit DepozitSursa { get; set; }
        public virtual Depozit DepozitDestinatie { get; set; }
        public virtual User User { get; set; }
    }

    public enum TipTranzactie
    {
        Intare = 1,
        Iesire = 2,
        Transfer = 3
    }
}
