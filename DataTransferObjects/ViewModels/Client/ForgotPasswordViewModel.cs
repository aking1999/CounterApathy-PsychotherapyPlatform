using Framework.Helpers.ExtensionMethods;
using System.ComponentModel.DataAnnotations;

namespace DataTransferObjects.ViewModels.Client
{
    public class ForgotPasswordViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        private string email;

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Imejl", Prompt = "example@gmail.com")]
        [EmailAddress(ErrorMessage = "Unesite validan imejl.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Unesite validan imejl.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Unesite od 5 do 100 karaktera.")] //256 in database
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
    }
}
