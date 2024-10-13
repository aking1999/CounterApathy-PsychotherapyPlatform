using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication9.Areas.Therapist.Models
{
    public class TileAddSession
    {
        private const string REQUIRED_FIELD = "Polje je obavezno.";

        //[Required]
        //[StringLength(maximumLength: 63)]
        //public string Subject { get; set; }

        //[StringLength(maximumLength: 191)]
        //public string Description { get; set; }

        [Display(Name = "Cena", Prompt = "Cena")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [Range(minimum: 500, maximum: 4500, ErrorMessage = "Unesite vrednost od 500 do 4500.")]
        public double? Price { get; set; }

        [Display(Name = "Tip seanse")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        public int? Type { get; set; }

        [Display(Name = "Datum")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(maximumLength: 63, ErrorMessage = "Unesite validan datum održavanja.")]
        //does not work with these
        //[DataType(DataType.Date)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:dd\/MM\/yyyy}")]
        public string StartEndDate { get; set; }

        [Display(Name = "Vreme početka")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(maximumLength: 63, ErrorMessage = "Unesite validno vreme početka.")]
        public string StartTime { get; set; }

        [Display(Name = "Vreme završetka")]
        [Required(ErrorMessage = REQUIRED_FIELD)]
        [StringLength(maximumLength: 63, ErrorMessage = "Unesite validno vreme završetka.")]
        public string EndTime { get; set; }
    }
}
