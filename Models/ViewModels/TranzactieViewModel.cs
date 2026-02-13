using Proiect_ASPDOTNET.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Proiect_ASPDOTNET.Models.ViewModels
{
    public class TranzactieViewModel
    {
        [Required(ErrorMessage = "Tipul tranzactiei este obligatoriu")]
        [Display(Name = "Tip Tranzactie")]
        public TipTranzactie Tip { get; set; }

        [Required(ErrorMessage = "Marfa este obligatorie")]
        [Display(Name = "Marfa")]
        public int MarfaId { get; set; }

        [Required(ErrorMessage = "Cantitatea este obligatorie")]
        [Display(Name = "Cantitate")]
        [Range(1, int.MaxValue, ErrorMessage = "Cantitatea trebuie sa fie mai mare ca 0")]
        public int Cantitate { get; set; }

        [Display(Name = "Depozit Sursa")]
        public int? DepozitSursaId { get; set; }

        [Display(Name = "Depozit Destinatie")]
        public int? DepozitDestinatieId { get; set; }

        [Display(Name = "Observatii")]
        public string Observatii { get; set; }
    }
}