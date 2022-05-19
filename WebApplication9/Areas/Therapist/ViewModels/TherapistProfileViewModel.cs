using Framework.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class TherapistProfileViewModel
    {
        [Display(Name = "Profile photo")]
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

        [Display(Name = "Web credit")]
        public string WebCredit { get; set; }

        [Display(Name = "Year of birth")]
        public int? YearOfBirth { get; set; }

        [Phone]
        [Required]
        [StringLength(100)]
        [Display(Name = "Phone number", Prompt = "Phone number")]
        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        //[Display(Name = "Amount due")]
        //public string AmountDue { get; set; }
        public string Earnings { get; set; }
        public bool HasEarnings { get; set; }

        [Required]
        [Display(Name = "About me (1023 letters max)", Prompt = "A few words about you to introduce you to the clients...")]
        [StringLength(1023)]
        public string About { get; set; }

        [StringLength(127)]
        [Display(Name = "Street", Prompt = "Street")]
        public string Street { get; set; }

        [StringLength(127)]
        [Display(Name = "House No.", Prompt = "House No.")]
        public string HouseNumber { get; set; }

        [StringLength(127)]
        [Display(Name = "City", Prompt = "City / Place")]
        public string City { get; set; }

        [StringLength(127)]
        [Display(Name = "Country", Prompt = "Country")]
        public string Country { get; set; }

        [StringLength(63)]
        [Display(Name = "Postal code", Prompt = "Postal Code")]
        public string PostalCode { get; set; }

        public int? OnVacation { get; set; }

        public string Gender { get; set; }

        public string Specialities { get; set; }

        public int NumberOfSpecialities { get; set; }

        [Display(Name = "Your chosen contact methods")]
        public List<SelectListItem> ToChooseFrom_ContactMethods { get; set; }

        [Required]
        [Display(Name = "Contact Methods", Prompt = "Your Contact Methods")]
        public string[] Chosen_ContactMethodsIds { get; set; }
    }
}
