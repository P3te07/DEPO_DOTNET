namespace Proiect_ASPDOTNET.Models.Entities
{
    public class Sarcina
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Titlu { get; set; }
        public string Descriere { get; set; }
        public DateTime DataLimita { get; set; }
        public bool Finalizata { get; set; }
        public DateTime? DataFinalizare { get; set; }
        public int PuncteReward { get; set; }
        public PrioritateSarcina Prioritate { get; set; }

        //nav
        public virtual User User { get; set; }
    }
    public enum PrioritateSarcina
    {
        Scazuta = 1,
        Medie = 2,
        Ridicata = 3
    }
}
