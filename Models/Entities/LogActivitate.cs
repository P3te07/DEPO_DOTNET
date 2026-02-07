namespace Proiect_ASPDOTNET.Models.Entities
{
    public class LogActivitate
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Actiune { get; set; }
        public string Detalii { get; set; }
        public DateTime DataOra { get; set; }
        public string AdresaIP { get; set; }
        public int? DepozitId { get; set; }

        // nav
        public virtual User User { get; set; }
        public virtual Depozit Depozit { get; set; }
    }
}
