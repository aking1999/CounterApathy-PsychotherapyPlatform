using Microsoft.AspNetCore.Mvc.Rendering;
using Framework.Helpers.ExtensionMethods;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication9.Validations;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class TherapistAccountSetupViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        private string about;

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(2047, MinimumLength = 50, ErrorMessage = "Unesite od 50 do 2047 karaktera.")]
        [Display(Name = "O meni", Prompt = "Nekoliko rečenica o Vama koje bi Vas predstavile klijentima. Do 2047 karaktera.")]
        public string About
        {
            get { return about; }
            set { about = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAbout); }
        }

        public List<SelectListItem> ToChooseFrom_ContactMethods { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [ContactMethodsAccountSetupRequired(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Gde sve želite da održavate seanse?", Prompt = "Kontakt metode")]
        public string[] Chosen_ContactMethodsIds { get; set; }

        public TherapistAccountSetupViewModel()
        {
            ToChooseFrom_ContactMethods = new List<SelectListItem>();
        }
    }
}
