using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataTransferObjects.ViewModels.Client
{
    public class CustomerSupportViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Ime", Prompt = "Aleksa")]
        [StringLength(maximumLength: 32, ErrorMessage = "Unesite do 32 karaktera.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Prezime", Prompt = "Aleksić")]
        [StringLength(maximumLength: 32, ErrorMessage = "Unesite do 32 karaktera.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Imejl", Prompt = "aleksa@gmail.com")]
        [EmailAddress(ErrorMessage = "Unesite validan imejl.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Unesite validan imejl.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Unesite od 5 do 100 karaktera.")]
        public string Email { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(2047, MinimumLength = 10, ErrorMessage = "Unesite od 10 do 2047 karaktera.")]
        [Display(Name = "Tekst", Prompt = "Vaše pitanje ili povratnu informaciju napišite ovde...")]
        public string Text { get; set; }

        [Display(Name = "Tema", Prompt = "Tema")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        public string Chosen_TopicId { get; set; }

        public List<SelectListItem> ToChooseFrom_Topics { get; set; }

        public CustomerSupportViewModel()
        {
            ToChooseFrom_Topics = new List<SelectListItem>();
        }
    }
}
