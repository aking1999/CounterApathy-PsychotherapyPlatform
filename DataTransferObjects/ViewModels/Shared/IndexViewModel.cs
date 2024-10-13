using DataTransferObjects.ViewModels.Client;
using System.Collections.Generic;

namespace DataTransferObjects.ViewModels.Shared
{
    public class IndexViewModel
    {
        public int TherapistCount { get; set; }
        public int SessionCount { get; set; }
        public int ClientCount { get; set; }
        public int PsychotherapyTechniquesCount { get; set; }
        public int SpecialtiesCount { get; set; }
        public int TherapistReviewsCount { get; set; }
        public int ContactMethodsCount { get; set; }
        public List<TherapistShowcaseViewModel> TherapistsWithConsultations { get; set; }
        public List<SkillTherapistsViewModel> PsychotherapyTechniquesTherapists { get; set; }
        public List<SkillTherapistsViewModel> SpecialtiesTherapists { get; set; }
        public NewsletterSubscriptionViewModel Newsletter { get; set; }

        public IndexViewModel()
        {
            TherapistsWithConsultations = new List<TherapistShowcaseViewModel>();
            PsychotherapyTechniquesTherapists = new List<SkillTherapistsViewModel>();
            SpecialtiesTherapists = new List<SkillTherapistsViewModel>();
            Newsletter = new NewsletterSubscriptionViewModel();
        }
    }
}
