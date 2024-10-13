using Framework.Helpers.ExtensionMethods;
using System.ComponentModel.DataAnnotations;

namespace WebApplication9.ViewModels
{
    public class RegisterInputViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";

        private string firstName;
        private string lastName;
        private string email;

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Ime", Prompt = "Ime")]
        [StringLength(maximumLength: 32, ErrorMessage = "Unesite do 32 karaktera.")] //128 in database
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                firstName = value.RemoveSpecialCharacters();
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Prezime", Prompt = "Prezime")]
        [StringLength(maximumLength: 32, ErrorMessage = "Unesite do 32 karaktera.")] //128 in database
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                lastName = value.RemoveSpecialCharacters();
            }
        }

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

        [DataType(DataType.Password)]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Potvrda lozinke", Prompt = "Potvrda lozinke")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Unesite od 6 do 100 karaktera.")]
        [Compare("Password", ErrorMessage = "Lozinka i potvrda lozinke treba da se poklapaju.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Mobilni telefon", Prompt = "Mobilni telefon")]
        [RegularExpression(@"[\d ?]{5,13}", ErrorMessage = "Unesite 5 do 13 cifara.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Politika privatnosti")]
        public bool PrivacyPolicy { get; set; }

        [Display(Name = "Uslovi korišćenja")]
        public bool TermsOfService { get; set; }

        public string ReturnUrl { get; set; }

        //public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}