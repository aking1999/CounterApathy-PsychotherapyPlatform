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
    public class TherapistPublicProfileForClientVisitorViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        private double rating;
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

        public TherapistPublicProfileForClientVisitorViewModel()
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
