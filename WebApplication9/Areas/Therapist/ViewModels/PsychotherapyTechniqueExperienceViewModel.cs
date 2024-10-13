using System;
using Framework.Helpers.ExtensionMethods;
using System.ComponentModel.DataAnnotations;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class PsychotherapyTechniqueExperienceViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        private string city;
        private string country;
        private string description;

        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Datum početka prakse")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-mm-dd}")]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Datum završetka prakse")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [DisplayFormat(ApplyFormatInEditMode = false, DataFormatString = "{0:yyyy-mm-dd}")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "I dalje praktikujem oblast")]
        public bool Present { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Grad / Mesto rada", Prompt = "Grad u kome ste stekli iskustvo")]
        [StringLength(maximumLength: 64, ErrorMessage = "Unesite do 64 karaktera.")]
        public string City
        {
            get
            {
                return city;
            }
            set
            {
                city = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAddress);
            }
        }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Država", Prompt = "Država u kojoj ste stekli iskustvo")]
        [StringLength(maximumLength: 64, ErrorMessage = "Unesite do 64 karaktera.")]
        public string Country
        {
            get
            {
                return country;
            }
            set
            {
                country = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAddress);
            }
        }

        [Display(Name = "Opis iskustva", Prompt = "Opišite Vaše iskustvo iz oblasti. Do 2047 karaktera.")]
        [StringLength(2047, MinimumLength = 20, ErrorMessage = "Unesite od 20 do 2047 karaktera.")]
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value.RemoveSpecialCharacters(AllowedSpecialCharacters.ForAbout);
            }
        }
    }
}
