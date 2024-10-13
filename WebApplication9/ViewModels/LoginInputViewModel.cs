using Framework.Helpers.ExtensionMethods;
using System.ComponentModel.DataAnnotations;

namespace WebApplication9.ViewModels
{
    public class LoginInputViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";

        private string email;

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Imejl", Prompt = "Imejl")]
        [EmailAddress(ErrorMessage = "Unesite validan imejl.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Unesite validan imejl.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Unesite od 5 do 100 karaktera.")]
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForEmail);
            }
        }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Lozinka", Prompt = "Lozinka")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Unesite od 6 do 100 karaktera.")]
        public string Password { get; set; }

        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }

        //public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        public string ErrorMessage { get; set; }
    }
}
