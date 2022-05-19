using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.PartialViewModels
{
    public class RatingPartialViewModel
    {
        [Required(ErrorMessage = "Please select a star.")]
        [Range(minimum: 1, maximum: 5, ErrorMessage = "Please select a star.")]
        public double StarsRating { get; set; }

        [StringLength(maximumLength: 255, ErrorMessage = "Please enter less than 255 letters.")]
        public string Comment { get; set; }

        public bool HasRating
        {
            get
            {
                if (StarsRating > 0)
                    return true;

                else return false;
            }
        }
    }
}
