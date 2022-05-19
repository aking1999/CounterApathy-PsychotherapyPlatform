using WebApplication9.PartialViewModels;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class TherapistPublicProfileViewModel
    {
        public string Id { get; set; }
        public string TherapistAccountId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public string YearOfBirth { get; set; }
        public string About { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public List<ClientReviewPartialViewModel> ClientReviews { get; set; }
        public List<SpecialitiesTherapistDescription> SpecialitiesTherapistDescription { get; set; }
        public List<SessionJsonViewModel> Sessions { get; set; }
        public string SerializedSessions { get; private set; }
        public List<SelectListItem> ToChooseFrom_ContactMethods { get; set; }

        [Required]
        [Display(Name = "Contact Methods", Prompt = "Your Contact Methods")]
        public string Chosen_ContactMethodId { get; set; }

        [Required(ErrorMessage = "This fiend is required.")]
        [StringLength(maximumLength: 128)]
        public string ContactInfo { get; set; }

        public void SerializeSessions()
        {
            SerializedSessions = JsonConvert.SerializeObject(Sessions);
        }

        public TherapistPublicProfileViewModel()
        {
            ClientReviews = new List<ClientReviewPartialViewModel>();
            SpecialitiesTherapistDescription = new List<SpecialitiesTherapistDescription>();
            Sessions = new List<SessionJsonViewModel>();
            ToChooseFrom_ContactMethods = new List<SelectListItem>();
        }
    }
}
