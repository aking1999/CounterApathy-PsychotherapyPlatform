using DataTransferObjects.ViewModels.Shared;
using System;
using System.Collections.Generic;

namespace WebApplication9.ViewModels
{
    public class TherapistCardViewModel
    {
        private double rating;

        public string Id { get; set; }
        public string ProfilePhoto { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SessionPrice { get; set; }
        public int NumberOfBookedSessions { get; set; }
        public int NumberOfUniqueClients { get; set; }
        public List<ContactMethodViewModel> ContactMethods { get; set; }
        public string SpecialtyNamesForDisplay { get; set; }
        public string PsychotherapyTechniqueNamesForDisplay { get; set; }
        public bool HasUpcomingConsultation { get; set; }

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

        public TherapistCardViewModel()
        {
            ContactMethods = new List<ContactMethodViewModel>();
        }
    }
}
