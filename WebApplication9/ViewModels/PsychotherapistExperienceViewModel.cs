using System;
using System.Collections.Generic;
using System.Text;
using WebApplication9.PartialViewModels;

namespace WebApplication9.ViewModels
{
    public class PsychotherapistExperienceViewModel
    {
        private double rating;

        public string TherapistId { get; set; }
        public string TherapistProfilePhotoPath { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public bool UnderSupervision { get; set; }
        public string PsychotherapyTechniqueNamesForDisplay { get; set; }
        public string SpecialtyNamesForDisplay { get; set; }
        public bool HasUpcomingConsultation { get; set; }
        public List<ClientReviewPartialViewModel> ClientReviews { get; set; }
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

        public PsychotherapistExperienceViewModel()
        {
            ClientReviews = new List<ClientReviewPartialViewModel>();
        }
    }
}
