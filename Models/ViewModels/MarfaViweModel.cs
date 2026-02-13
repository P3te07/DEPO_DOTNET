using System.ComponentModel.DataAnnotations;

namespace Proiect_ASPDOTNET.Models.ViewModels
{
    public class MarfaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele marfii este obligatoriu")]
        [Display(Name = "Nume Marfa")]
        public string Nume { get; set; }

        [Display(Name = "Descriere")]
        public string Descriere { get; set; }

        [Required(ErrorMessage = "SKU este obligatoriu")]
        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Cantitatea este obligatorie")]
        [Display(Name = "Cantitate")]
        public int CantitateCurenta { get; set; }

        [Required(ErrorMessage = "Unitatea de masura este obligatorie")]
        [Display(Name = "Unitate Masura")]
        public string UnitateMasura { get; set; }

        [Required(ErrorMessage = "Pretul este obligatoriu")]
        [Display(Name = "Pret Unitar")]
        public decimal PretUnitar { get; set; }

        [Display(Name = "Zona")]
        public string Zona { get; set; }

        [Display(Name = "Etaj")]
        public int Etaj { get; set; }

        [Display(Name = "Raft")]
        public string Raft { get; set; }

        [Display(Name = "Pozitie")]
        public string Pozitie { get; set; }

        public int DepozitId { get; set; }
    }
}