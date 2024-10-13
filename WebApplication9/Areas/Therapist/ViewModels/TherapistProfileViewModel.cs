using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Framework.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication9.Validations;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class TherapistProfileViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        private string about;
        private string street;
        private string houseNumber;
        private string city;
        private string postalCode;
        private string country;

        [DataType(DataType.Upload)]
        [Display(Name = "Profile photo")]
        //[Required(ErrorMessage = "Field is required.")] ako ovde stoji required, na frontu to pravi gresku jer nema slike i FormIsValid ne prolazi. Treba dinamicki da stavlja sliku
        [AllowedFileExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile ProfilePhoto { get; set; }

        public string Id { get; set; }

        [Display(Name = "Ime", Prompt = "Aleksa")]
        public string FirstName { get; set; }

        [Display(Name = "Prezime", Prompt = "Aleksić")]
        public string LastName { get; set; }

        [Display(Name = "Imejl", Prompt = "aleksa@gmail.com")]
        public string Email { get; set; }

        //[Display(Name = "Web credit")]
        //public string WebCredit { get; set; }

        [Display(Name = "Godina rođenja")]
        public int? YearOfBirth { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Mobilni telefon", Prompt = "631826932")]
        [RegularExpression(@"[\d ?]{5,13}", ErrorMessage = "Unesite od 5 do 13 cifara.")]
        public string PhoneNumber { get; set; }

        //[Display(Name = "Amount due")]
        //public string AmountDue { get; set; }

        [Display(Name = "Sredstva / zarada")]
        public double Earnings { get; set; }
        public bool HasEarnings
        {
            get
            {
                return Earnings >= 3500;
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(2047, MinimumLength = 50, ErrorMessage = "Unesite od 50 do 2047 karaktera.")]
        [Display(Name = "O meni", Prompt = "Nekoliko rečenica o Vama koje bi Vas predstavile klijentima. Do 2047 karaktera.")]
        public string About
        {
            get
            {
                return about;
            }
            set
            {
                about = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAbout);
            }
        }

        [Display(Name = "Ulica", Prompt = "Savska")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(127, ErrorMessage = "Unesite do 127 karaktera.")]
        public string Street
        {
            get
            {
                return street;
            }
            set
            {
                street = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAddress);
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Broj zgrade", Prompt = "79A")]
        [StringLength(32, ErrorMessage = "Unesite do 32 karaktera.")]
        public string HouseNumber
        {
            get
            {
                return houseNumber;
            }
            set
            {
                houseNumber = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAddress);
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Grad", Prompt = "Grad / Mesto")]
        [StringLength(127, ErrorMessage = "Unesite do 127 karaktera.")]
        public string City
        {
            get
            {
                return city;
            }
            set
            {
                city = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAddress);
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Poštanski broj", Prompt = "11032")]
        [RegularExpression(@"^\d{2,8}$", ErrorMessage = "Unesite od 2 do 8 cifara.")]
        public string PostalCode
        {
            get
            {
                return postalCode;
            }
            set
            {
                postalCode = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAddress);
            }
        }

        [Display(Name = "Država", Prompt = "Srbija")]
        public string Country
        {
            get
            {
                return country;
            }
            set
            {
                country = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAddress);
            }
        }

        public bool UnderSupervision { get; set; }

        public bool HasSetupStripeAccount { get; set; }

        [Display(Name = "Psihoterapijske tehnike")]
        public List<PsychotherapyTechniques> PsychotherapyTechniques { get; set; }

        [Display(Name = "Specijalnosti")]
        public List<Specialities> Specialities { get; set; }

        public List<SelectListItem> ToChooseFrom_ContactMethods { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [ContactMethodsTherapistProfileRequired(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Gde sve želite da održavate seanse?", Prompt = "Kontakt metode")]
        public string[] Chosen_ContactMethodsIds { get; set; }

        public TherapistProfileViewModel()
        {
            PsychotherapyTechniques = new List<PsychotherapyTechniques>();
            Specialities = new List<Specialities>();
            ToChooseFrom_ContactMethods = new List<SelectListItem>();
            //Chosen_ContactMethodsIds = new string[] { };
        }
    }
}
