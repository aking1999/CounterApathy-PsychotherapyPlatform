using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataTransferObjects.ViewModels.Admin
{
    public class AdminAddWebCreditPaymentTypesViewModel
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";

        public List<AdminAddWebCreditViewModel> Clients { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        public string UserId { get; set; }

        [Display(Name = "Količina", Prompt = "Količina")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Range(1, 50000, ErrorMessage = "Unesite vrednost od 1 do 50000.")]
        public double Amount { get; set; }

        public List<SelectListItem> ToChooseFrom_PaymentTypes { get; set; }

        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Display(Name = "Na koji način je klijent izvršio uplatu?", Prompt = "Način uplate")]
        public string Chosen_PaymentTypeId { get; set; }

        public AdminAddWebCreditPaymentTypesViewModel()
        {
            Clients = new List<AdminAddWebCreditViewModel>();
            ToChooseFrom_PaymentTypes = new List<SelectListItem>();
        }
    }
}
