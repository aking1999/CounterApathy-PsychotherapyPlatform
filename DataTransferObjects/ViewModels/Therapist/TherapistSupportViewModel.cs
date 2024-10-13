using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObjects.ViewModels.Therapist
{
    public class TherapistSupportViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";
        
        [Display(Name = "Ime", Prompt = "Aleksa")]
        public string FirstName { get; set; }

        [Display(Name = "Prezime", Prompt = "Aleksić")]
        public string LastName { get; set; }

        [Display(Name = "Imejl", Prompt = "aleksa@gmail.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(2047, MinimumLength = 10, ErrorMessage = "Unesite od 10 do 2047 karaktera.")]
        [Display(Name = "Tekst", Prompt = "Vaše pitanje ili povratnu informaciju napišite ovde...")]
        public string Text { get; set; }

        [Display(Name = "Tema", Prompt = "Tema")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        public string Chosen_TopicId { get; set; }

        public List<SelectListItem> ToChooseFrom_Topics { get; set; }

        public TherapistSupportViewModel()
        {
            ToChooseFrom_Topics = new List<SelectListItem>();
        }
    }
}
