using System.ComponentModel.DataAnnotations;

namespace WebApplication9.ViewModels
{
    public class ChangePasswordViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";

        [DataType(DataType.Password)]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Trenutna lozinka", Prompt = "Trenutna lozinka")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Unesite od 6 do 100 karaktera.")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Nova lozinka", Prompt = "Nova lozinka")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Unesite od 6 do 100 karaktera.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potvrda nove lozinke", Prompt = "Ponovo unesite novu lozinku")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Unesite od 6 do 100 karaktera.")]
        [Compare("NewPassword", ErrorMessage = "Nova lozinka i potvrda nove lozinke treba da se poklapaju.")]
        public string ConfirmPassword { get; set; }
    }
}
