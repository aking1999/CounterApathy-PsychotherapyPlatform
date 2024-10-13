using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTransferObjects.PartialViewModels
{
    public class RatingPartialViewModel
    {
        [Required(ErrorMessage = "Unesite do 5 zvezdica.")]
        [Range(minimum: 1, maximum: 5, ErrorMessage = "Unesite do 5 zvezdica.")]
        public double StarsRating { get; set; }

        [Display(Name = "Komentar", Prompt = "Unesite Vaš komentar o seansi...")]
        [Required(ErrorMessage = "Unesite od 5 do 767 karaktera.")]
        [StringLength(maximumLength: 767, MinimumLength = 5, ErrorMessage = "Unesite od 5 do 767 karaktera.")]
        public string Comment { get; set; }

        public bool HasRating
        {
            get
            {
                return StarsRating > 0;
            }
        }
    }
}
