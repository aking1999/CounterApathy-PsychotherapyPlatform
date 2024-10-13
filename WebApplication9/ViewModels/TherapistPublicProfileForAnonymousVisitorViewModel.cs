using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.PartialViewModels;

namespace WebApplication9.ViewModels
{
    public class TherapistPublicProfileForAnonymousVisitorViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        private double rating;
        private string clientFirstName;
        private string clientLastName;

        public string UserId { get; set; }
        public string TherapistAccountId { get; set; }
        public string ProfilePhoto { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public GenderViewModel Gender { get; set; }
        public bool UnderSupervision { get; set; }
        public string SessionPrice { get; set; }
        public string YearOfBirth { get; set; }
        public string About { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public bool HasUpcomingConsultation { get; set; }
        public List<ClientReviewPartialViewModel> ClientReviews { get; set; }
        public List<PsychotherapyTechniqueExperienceProfileViewModel> PsychotherapyTechniquesExperiences { get; set; }
        public List<SpecialtyExperienceProfileViewModel> SpecialtiesExperiences { get; set; }
        public List<SessionViewModel> Sessions { get; set; }
        public double Rating
        {
            get
            {
                return rating;
            }
            set
            {
                try
                {
                    rating = Math.Ceiling(value);
                }
                catch (Exception)
                {
                    rating = value;
                }
            }
        }
        public string SerializedSessions
        {
            get
            {
                try
                {
                    return JsonConvert.SerializeObject(Sessions);
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Ime ili alijas", Prompt = "Ime ili alijas")]
        [StringLength(maximumLength: 32, ErrorMessage = "Unesite do 32 karaktera.")]
        public string ClientFirstName
        {
            get
            {
                return clientFirstName;
            }
            set
            {
                clientFirstName = value.RemoveSpecialCharacters();
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Prezime ili alijas", Prompt = "Prezime ili alijas")]
        [StringLength(maximumLength: 32, ErrorMessage = "Unesite do 32 karaktera.")]
        public string ClientLastName
        {
            get
            {
                return clientLastName;
            }
            set
            {
                clientLastName = value.RemoveSpecialCharacters();
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Imejl", Prompt = "Imejl")]
        [EmailAddress(ErrorMessage = "Unesite validan imejl.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Unesite validan imejl.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Unesite od 5 do 100 karaktera.")]
        public string ClientEmail { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Mobilni telefon", Prompt = "Mobilni telefon")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"[\d ?]{5,13}", ErrorMessage = "Unesite 5 do 13 cifara.")]
        public string ClientPhoneNumber { get; set; }

        public List<SelectListItem> ToChooseFrom_ContactMethods { get; set; }
        public bool HasInPersonContactMethod
        {
            get
            {
                return ToChooseFrom_ContactMethods.Any(i => i.Value.ToLower().Contains(Framework.Providers.ContactMethodsProvider.InPerson.Id));
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Gde želite da se seansa održi?", Prompt = "Kontakt metode")]
        public string Chosen_ContactMethodId { get; set; }

        //[Required(ErrorMessage = "Field is required.")]
        //[StringLength(maximumLength: 128, MinimumLength = 2, ErrorMessage = "Enter 2 to 128 characters.")]
        //public string ContactInfo { get; set; }

        public TherapistPublicProfileForAnonymousVisitorViewModel()
        {
            Gender = new GenderViewModel();
            ClientReviews = new List<ClientReviewPartialViewModel>();
            PsychotherapyTechniquesExperiences = new List<PsychotherapyTechniqueExperienceProfileViewModel>();
            SpecialtiesExperiences = new List<SpecialtyExperienceProfileViewModel>();
            Sessions = new List<SessionViewModel>();
            ToChooseFrom_ContactMethods = new List<SelectListItem>();
        }
    }
}
