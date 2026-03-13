using System.ComponentModel.DataAnnotations;

namespace Proiect_ASPDOTNET.Models.ViewModels
{
    public class CompanieViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele companiei este obligatoriu")]
        [Display(Name = "Nume Companie")]
        public string Nume { get; set; }

        [Required(ErrorMessage = "Cod_Inregistrare-ul este obligatoriu")]
        [Display(Name = "Cod_Inregistrare")]
        public string Cod_Inregistrare { get; set; }

        [Display(Name = "Adresa")]
        public string Adresa { get; set; }

        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Format telefon invalid")]
        public string Telefon { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Format email invalid")]
        public string Email { get; set; }
    }
}