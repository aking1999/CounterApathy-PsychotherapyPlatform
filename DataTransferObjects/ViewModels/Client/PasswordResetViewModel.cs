using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObjects.ViewModels.Client
{
    public class PasswordResetViewModel
    {
        private const string FIELD_REQUIRED = "Polje je obavezno.";

        public string Uid { get; set; }
        public string Token { get; set; }

        public string ProfilePhoto { get; set; }
        public string FullName { get; set; }

        // !!! ovo moram da popravim kasnije da stavim da min length bude vece u deploymentu
        [DataType(DataType.Password)]
        [Required(ErrorMessage = FIELD_REQUIRED)]
        [Display(Name = "Nova lozinka", Prompt = "Unesite novu lozinku")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Unesite od 6 do 100 karaktera.")]
        public string Password { get; set; }

        // !!! ovo moram da popravim kasnije da stavim da min length bude vece u deploymentu
        [DataType(DataType.Password)]
        [Required(ErrorMessage = FIELD_REQUIRED)]
        [Compare("Password", ErrorMessage = "Nova lozinka i potvrda nove lozinke se ne poklapaju.")]
        [Display(Name = "Potvrda nove lozinke", Prompt = "Ponovo unesite novu lozinku")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Unesite od 6 do 100 karaktera.")]
        public string ConfirmPassword { get; set; }
    }
}
