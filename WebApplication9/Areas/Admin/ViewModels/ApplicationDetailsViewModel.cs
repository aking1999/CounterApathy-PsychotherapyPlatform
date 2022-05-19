using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class ApplicationDetailsViewModel
    {
        //CustomClient's properties
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public string WebCredit { get; set; }
        public string YearOfBirth { get; set; }
        public string AmountDue { get; set; }


        //TherapistApplications' properties

        public string TherapistApplicationId { get; set; }
        public string ApplicationDate { get; set; }
        public string Accepted { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Gender { get; set; }
        public string HouseNumber { get; set; }
        public string University { get; set; }
        public string PastCompanies { get; set; }
        public string ProfilePhoto { get; set; }

        public List<Specialities> Specialities { get; set; } = new List<Specialities>();
    }
}
