using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.Models
{
    public class AdminDashboardAggregatedInformation
    {
        public string UserId { get; set; }
        public string ProfilePhoto { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ApplicationDate { get; set; }
        public int? Accepted { get; set; }

        /*public AdminDashboardAggregatedInformation(object aggregatedInformation)
        {
            Type type = aggregatedInformation.GetType();
            var profilePhoto = (AspNetUsers)type.GetProperty("ProfilePhoto").GetValue(aggregatedInformation, null);
            var firstName = (AspNetUsers)type.GetProperty("FirstName").GetValue(aggregatedInformation, null);
            var lastName = (AspNetUsers)type.GetProperty("LastName").GetValue(aggregatedInformation, null);
            var email = (AspNetUsers)type.GetProperty("Email").GetValue(aggregatedInformation, null);
            var application = (TherapistApplications)type.GetProperty("Accepted").GetValue(aggregatedInformation, null);
            
        }*/
    }
}
