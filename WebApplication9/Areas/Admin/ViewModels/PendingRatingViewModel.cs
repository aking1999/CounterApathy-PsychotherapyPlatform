using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class PendingRatingViewModel
    {
        public string Id { get; set; }
        public string TherapistId { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public string TherapistEmail { get; set; }
        private double therapistAverageStarsRating { get; set; }
        public string ClientId { get; set; }
        public string ClientEmail { get; set; }

        // !!! trenutno samo obicni klijenti mogu zakazivati sesije, tako da ne treba da se prikazuje ClientRole
        // jer se podrazumeva da je Client. Kad budu mogli i terapeuti da zakazuju, tad ce trebati da se stavi i ClientRole
        // da bi moglo da se vidi ko je dao ocenu koju treba potvrditi.
        //public string ClientRole { get; set; }

        public string SessionId { get; set; }
        public string SessionDate { get; set; }
        public string SessionStartTime { get; set; }
        public string SessionEndTime { get; set; }
        public string BookedSessionId { get; set; }
        public double StarsRating { get; set; }
        public string Comment { get; set; }
        public string RatingDate { get; set; }
        public int? Refused { get; set; }

        public bool IsRefused
        {
            get
            {
                if (Refused.HasValue)
                {
                    if (Refused.Value > 0)
                        return true;
                }

                return false;
            }
        }

        public double TherapistAverageStarsRating
        {
            get
            {
                return therapistAverageStarsRating;
            }
            set
            {
                therapistAverageStarsRating = Math.Ceiling(value);
            }
        }
    }
}
