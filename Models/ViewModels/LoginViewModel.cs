using System.ComponentModel.DataAnnotations;

namespace Proiect_ASPDOTNET.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username-ul este obligatoriu")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Parola este obligatorie")]
        [DataType(DataType.Password)]
        [Display(Name = "Parola")]
        public string Password { get; set; }

        [Display(Name = "Tine-ma minte")]
        public bool RememberMe { get; set; }
    }
}