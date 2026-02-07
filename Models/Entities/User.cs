using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Proiect_ASPDOTNET.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string NumeComplet { get; set; }
        public UserRole Rol { get; set; }
        public int? CompanieId { get; set; }
        public int? DepozitId { get; set; }
        public DateTime DataCreare { get; set; }
        public bool Activ { get; set; }
        public int PuncteReward {  get; set; }

        //Nav 
        public virtual Companie Companie { get; set; }
        public virtual Depozit Depozit { get; set; }
        public virtual ICollection<LogActivitate> LogActivitati { get; set; }
        public virtual ICollection<Sarcina> Sarcini {  get; set; }


    }
    public enum UserRole { 
        SuperAdmin = 1,
        DirectorCompanie = 2,
        ResponsabilDepozit = 3,
        Muncitor = 4
    }
}
