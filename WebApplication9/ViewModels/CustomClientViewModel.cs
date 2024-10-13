using Framework.Validations;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication9.ViewModels
{
    public class CustomClientViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        private string firstName;
        private string lastName;

        [DataType(DataType.Upload)]
        [Display(Name = "Profilna slika")]
        [AllowedFileExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile ProfilePhoto { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        //[RegularExpression("^(\\p{L}\\p{M}*)+$")]
        //[RegularExpression("^\\p{L}+$", ErrorMessage = "Only letters allowed.")] //Dozvoljena samo slova.
        [Display(Name = "Ime", Prompt = "Aleksa")]
        [StringLength(maximumLength: 32, ErrorMessage = "Unesite do 32 karaktera.")]
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
        [Display(Name = "Prezime", Prompt = "Aleksić")]
        [StringLength(maximumLength: 32, ErrorMessage = "Unesite do 32 karaktera.")]
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

        //[Required]
        //[EmailAddress]
        //[StringLength(255)]
        [Display(Name = "Imejl", Prompt = "aleksa@gmail.com")]
        public string Email { get; set; }

        [Display(Name = "Veb kredit")]
        public string WebCredit { get; set; }

        //[Required]
        [Display(Name = "Godina rođenja")]
        [Range(1932, 2007, ErrorMessage = "Unesite vrednost od 1932 do 2007.")]
        public int? YearOfBirth { get; set; }

        [DataType(DataType.PhoneNumber)]
        //[Required(ErrorMessage = "Field is required.")]
        [Display(Name = "Mobilni telefon", Prompt = "631826932")]
        [RegularExpression(@"[\d ?]{5,13}", ErrorMessage = "Unesite od 5 do 13 cifara.")]
        public string PhoneNumber { get; set; }

        public bool HasAppliedForTherapistAccount { get; set; } = false;

        //[Display(Name = "Amount due")]
        //public string AmountDue { get; set; }
    }
}
