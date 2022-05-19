using Database.Models;
using Database.RepositoryImplementations;
using Framework.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApplication9.ViewModels
{
    public class TherapistApplicationsViewModel
    {
        [Required]
        [Display(Name = "Profile photo")]
        [DataType(DataType.Upload)]
        [AllowedFileExtensions(new string[] { ".jpg", ".jpeg", ".png", ".svg" })]
        public IFormFile ProfilePhoto { get; set; }

        public string Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        [Display(Name = "First name", Prompt = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name", Prompt = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Email", Prompt = "Email")]
        public string Email { get; set; }

        public string NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string ConcurrencyStamp { get; set; }

        public string WebCredit { get; set; }

        [Required]
        [StringLength(29)]
        public string Gender { get; set; }

        [Display(Name = "Year of birth", Prompt = "Year of birth")]
        public int? YearOfBirth { get; set; }

        [Display(Name = "Phone number", Prompt = "Phone number")]
        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        [StringLength(127)]
        [Display(Name = "Street", Prompt = "Street (opt)")]
        public string Street { get; set; }

        [StringLength(127)]
        [Display(Name = "House No.", Prompt = "House No. (opt)")]
        public string HouseNumber { get; set; }

        [StringLength(127)]
        [Display(Name = "City", Prompt = "City / Place (opt)")]
        public string City { get; set; }

        [StringLength(127)]
        [Display(Name = "Country", Prompt = "Country (opt)")]
        public string Country { get; set; }

        [StringLength(63)]
        [Display(Name = "Postal code", Prompt = "Postal Code (opt)")]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(511)]
        [Display(Name = "University", Prompt = "Your graduation university")]
        public string University { get; set; }

        [Required]
        [StringLength(511)]
        [Display(Name = "Past companies", Prompt = "Places you worked for")]
        public string PastCompanies { get; set; }

        [Required]
        public bool TermsAndConditions { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        public string ApplicationDate { get; set; }

        public List<SelectListItem> ToChooseFrom_Specialities { get; set; }

        [Required]
        [Display(Name = "Specialities", Prompt = "Specialities")]
        public string[] Chosen_SpecialitiesIds { get; set; }

        //public List<Specialities> Chosen_Specialities { get; private set; }

        //public TherapistApplicationsViewModel()
        //{
        //    Chosen_Specialities = new List<Specialities>();
        //}

        /*public void FindSpecialitiesFromDatabaseBasedOnSelectedIds(UnitOfWork context)
        {
            if (Chosen_SpecialitiesIds == null || Chosen_SpecialitiesIds.Length == 0)
                throw new Exception("No speciality selected.");

            if (context == null || context == default)
                throw new Exception("No context provided for type TherapistApplicationsViewModel.");

            if (context.Specialities.CountEntities() < 1)
                throw new Exception("Selected context has no specialities.");

            foreach(var specialityId in this.Chosen_SpecialitiesIds)
            {
                Chosen_Specialities.Add(context.Specialities.GetById(specialityId));
            }
        }*/

        public TherapistApplicationsViewModel()
        {
            ToChooseFrom_Specialities = new List<SelectListItem>();
        }
    }
}
