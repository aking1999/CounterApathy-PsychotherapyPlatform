using Database.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class ApplicationDetailsViewModel
    {
        //CustomClient's properties
        public string UserId { get; set; }

        [Display(Name = "Ime")]
        public string FirstName { get; set; }

        [Display(Name = "Prezime")]
        public string LastName { get; set; }

        [Display(Name = "Imejl")]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Mobilni telefon")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Veb kredit")]
        public string WebCredit { get; set; }

        [Display(Name = "Godina rođenja")]
        public string YearOfBirth { get; set; }

        //TherapistApplications' properties
        public string TherapistApplicationId { get; set; }

        [Display(Name = "Datum apliciranja")]
        public string ApplicationDate { get; set; }

        public bool UnderSupervision { get; set; }

        public int Accepted { get; set; }

        [Display(Name = "Ulica")]
        public string Street { get; set; }

        [Display(Name = "Broj zgrade")]
        public string HouseNumber { get; set; }

        [Display(Name = "Grad")]
        public string City { get; set; }

        [Display(Name = "Država")]
        public string Country { get; set; }

        [Display(Name = "Poštanski broj")]
        public string PostalCode { get; set; }

        [Display(Name = "Pol")]
        public string Gender { get; set; }

        [Display(Name = "Fakultet i trenutna titula")]
        public string University { get; set; }

        [Display(Name = "Sertifikati i iskustvo u radu")]
        public string PastCompanies { get; set; }

        public string ProfilePhoto { get; set; }

        [Display(Name = "Specijalnosti")]
        public List<Specialities> Specialities { get; set; }

        [Display(Name = "Psihoterapijske tehnike")]
        public List<PsychotherapyTechniques> PsychotherapyTechniques { get; set; }

        public ApplicationDetailsViewModel()
        {
            Specialities = new List<Specialities>();
            PsychotherapyTechniques = new List<PsychotherapyTechniques>();
        }
    }
}
