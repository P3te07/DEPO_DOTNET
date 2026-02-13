using System.ComponentModel.DataAnnotations;

namespace Proiect_ASPDOTNET.Models.ViewModels
{
    public class DepozitViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele depozitului este obligatoriu")]
        [Display(Name = "Nume Depozit")]
        public string Nume { get; set; }

        [Required(ErrorMessage = "Compania este obligatorie")]
        [Display(Name = "Companie")]
        public int CompanieId { get; set; }

        [Display(Name = "Adresa")]
        public string Adresa { get; set; }

        [Required(ErrorMessage = "Latitudinea este obligatorie")]
        [Display(Name = "Latitudine")]
        [Range(-90, 90, ErrorMessage = "Latitudine invalida")]
        public decimal Latitudine { get; set; }

        [Required(ErrorMessage = "Longitudinea este obligatorie")]
        [Display(Name = "Longitudine")]
        [Range(-180, 180, ErrorMessage = "Longitudine invalida")]
        public decimal Longitudine { get; set; }

        [Display(Name = "Capacitate Maxima")]
        public int CapacitateMaxima { get; set; }
    }
}