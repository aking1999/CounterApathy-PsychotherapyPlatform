using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Therapist.Models
{
    public class AddConsultation
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";

        [Display(Name = "Datum")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(maximumLength: 63, ErrorMessage = "Unesite validan datum održavanja.")]
        public string StartEndDate { get; set; }

        [Display(Name = "Vreme početka")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(maximumLength: 63, ErrorMessage = "Unesite validno vreme početka.")]
        public string StartTime { get; set; }
    }
}
