using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class TherapistAccountSetupViewModel
    {
        [Required]
        [StringLength(127)]
        public string Street { get; set; }

        [Required]
        [StringLength(127)]
        [Display(Name = "House number")]
        public string HouseNumber { get; set; }

        [Required]
        [StringLength(127)]
        public string City { get; set; }

        [Required]
        [Display(Name = "Postal code")]
        [Range(1, 999999, ErrorMessage = "Enter a number between 1 and 999999.")]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(127)]
        public string Country { get; set; }

        [Required]
        [Display(Name = "About me (1023 letters max.)", Prompt = "A few words about you to introduce you to the clients...")]
        [StringLength(1023)]
        public string About { get; set; }

        public List<SelectListItem> ToChooseFrom_ContactMethods { get; set; }

        [Required]
        [Display(Name = "Contact Methods", Prompt = "Your Contact Methods")]
        public string[] Chosen_ContactMethodsIds { get; set; }

        public TherapistAccountSetupViewModel()
        {
            ToChooseFrom_ContactMethods = new List<SelectListItem>();
        }
    }
}
