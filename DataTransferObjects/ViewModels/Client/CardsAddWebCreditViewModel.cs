using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObjects.ViewModels.Client
{
    public class CardsAddWebCreditViewModel
    {
        [Display(Name = "Količina", Prompt = "Količina")]
        [Required(ErrorMessage = "Polje je obavezno.")]
        [Range(100, 9000, ErrorMessage = "Unesite vrednost od 100 do 9000.")]
        public double Amount { get; set; }
    }
}
