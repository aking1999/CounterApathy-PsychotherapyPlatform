using Framework.Validations;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication9.Validations;

namespace WebApplication9.ViewModels
{
    public class TherapistApplicationsViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        private string firstName;
        private string lastName;
        private string email;
        private string street;
        private string houseNumber;
        private string city;
        private string postalCode;
        private string country;
        private string university;
        private string pastCompanies;

        [DataType(DataType.Upload)]
        [Display(Name = "Profilna slika")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [AllowedFileExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile ProfilePhoto { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Ime", Prompt = "Aleksa")]
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
        [Display(Name = "Prezime", Prompt = "Aleksić")]
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

        //no more data annotations here
        //because we do not allow the user to enter Email
        //on this Apply page, just to view it, so annotations not needed
        [Display(Name = "Imejl", Prompt = "aleksa@gmail.com")]
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

        public List<SelectListItem> ToChooseFrom_Genders { get; set; }

        [Display(Name = "Pol", Prompt = "Muški")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(29, ErrorMessage = "Izaberite validan pol.")]
        public string Chosen_GenderId { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Godina rođenja", Prompt = "1999")]
        [Range(1932, 2001, ErrorMessage = "Unesite vrednost od 1932 do 2001.")]
        public int? YearOfBirth { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Mobilni telefon", Prompt = "631826932")]
        [RegularExpression(@"[\d ?]{5,13}", ErrorMessage = "Unesite od 5 do 13 cifara.")]
        public string PhoneNumber { get; set; }

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

        //no more data annotations here
        //because we do not allow the user to enter Country
        //on this Apply page, just to view it, so annotations not needed
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

        [Display(Name = "Supervizija")]
        public bool UnderSupervision { get; set; }

        [Display(Name = "Pravno lice")]
        public bool LegalEntity { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Fakultet i trenutna titula", Prompt = "Medicinski fakultet. Geštalt psihoterapeut")]
        [StringLength(511, MinimumLength = 5, ErrorMessage = "Unesite od 5 do 511 karaktera.")]
        public string University
        {
            get
            {
                return university;
            }
            set
            {
                university = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAbout);
            }
        }

        [Display(Name = "Sertifikati i iskustvo u radu", Prompt = "Sertifikat iz xyz. Mesta gde ste radili ili trenutno radite. Navedite ako ste član nekog udruženja ili ako ste pravno lice")]
        [StringLength(497, MinimumLength = 4, ErrorMessage = "Unesite od 4 do 497 karaktera.")]
        public string PastCompanies
        {
            get
            {
                return pastCompanies;
            }
            set
            {
                pastCompanies = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAbout);
            }
        }

        [ApplicationTermsOfServiceCheckBoxRequired(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Uslovi korišćenja za klijente")]
        public bool TermsOfService { get; set; }

        [ApplicationPrivacyPolicyCheckBoxRequired(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Politika privatnosti")]
        public bool PrivacyPolicy { get; set; }

        [ApplicationTherapistTermsOfServiceCheckBoxRequired(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Uslovi korišćenja za psihoterapeute")]
        public bool TherapistTermsOfService { get; set; }

        public List<SelectListItem> ToChooseFrom_Specialties { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [SpecialtiesRequired(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Specijalnosti", Prompt = "Specijalnosti")]
        public string[] Chosen_SpecialtiesIds { get; set; }

        public List<SelectListItem> ToChooseFrom_PsychotherapyTechniques { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [PsychotherapyTechniquesRequired(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Psihoterapijske tehnike", Prompt = "Psihoterapijske tehnike")]
        public string[] Chosen_PsychotherapyTechniquesIds { get; set; }

        public TherapistApplicationsViewModel()
        {
            ToChooseFrom_Specialties = new List<SelectListItem>();
            ToChooseFrom_PsychotherapyTechniques = new List<SelectListItem>();
        }
    }
}
