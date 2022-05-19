using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Admin.Models;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class AdminDashboardViewModel
    {
        //public IEnumerable<TherapistApplications> TherapistApplications { get; set; }
        public IEnumerable<object> AggregatedInformationUnparsed { get; set; }
        public List<AdminDashboardAggregatedInformation> AggregatedInformationParsed { get; private set; }

        public void ParseAggregatedInformation()
        {
            if (AggregatedInformationParsed == null) AggregatedInformationParsed = new List<AdminDashboardAggregatedInformation>();
            if (AggregatedInformationUnparsed == null) throw new Exception("Property 'AggregatedInformationUnparsed' is Null.");

            foreach(var row in AggregatedInformationUnparsed)
            {
                Type type = row.GetType();

                string profilePhoto = null;
                var profilePhotoTemp = type.GetProperty("ProfilePhoto").GetValue(row, null);

                if (profilePhotoTemp != null) profilePhoto = profilePhotoTemp.ToString();

                var userId = type.GetProperty("Id").GetValue(row, null).ToString();
                var firstName = type.GetProperty("FirstName").GetValue(row, null).ToString();
                var lastName = type.GetProperty("LastName").GetValue(row, null).ToString();
                var email = type.GetProperty("Email").GetValue(row, null).ToString();
                var applicationDate = type.GetProperty("ApplicationDate").GetValue(row, null).ToString();
                int accepted;
                //var accepted = type.GetProperty("Accepted").GetValue(row, null).ToString();

                if (int.TryParse(type.GetProperty("Accepted").GetValue(row, null).ToString(), out accepted))
                {
                    AggregatedInformationParsed.Add(new AdminDashboardAggregatedInformation
                    {
                        UserId = userId,
                        ProfilePhoto = profilePhoto,
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        ApplicationDate = applicationDate,
                        Accepted = accepted
                    });
                }
                else
                {
                    throw new Exception("Error while creating object of type AggregatedInformationParsed.");
                }
            }
        }
    }
}
