using WebApplication9.PartialViewModels;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class TherapistPublicProfileForAuthenticatedNonClientVisitorViewModel
    {
        private double rating;

        public string UserId { get; set; }
        public string TherapistAccountId { get; set; }
        public bool IsSelf { get; set; }
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

        [Display(Name = "Gde želite da se seansa održi?", Prompt = "Kontakt metode")]
        public string Chosen_ContactMethodId { get; set; }

        //[Required(ErrorMessage = "Field is required.")]
        //[StringLength(maximumLength: 128, MinimumLength = 2, ErrorMessage = "Enter 2 to 128 characters.")]
        //public string ContactInfo { get; set; }

        public TherapistPublicProfileForAuthenticatedNonClientVisitorViewModel()
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
