using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Therapist.Models
{
    public class TileAddSession
    {
        [Required]
        [StringLength(maximumLength: 63)]
        public string Subject { get; set; }

        [StringLength(maximumLength: 191)]
        public string Description { get; set; }

        [Required]
        [Range(minimum: 500, maximum: 8000)]
        public double? Price { get; set; }

        [Required]
        [Display(Name = "Session type")]
        public int? Type { get; set; }

        [Required]
        [Display(Name = "Date")]
        [StringLength(maximumLength: 63)]
        //[DataType(DataType.Date)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:dd\/MM\/yyyy}")]
        public string StartEndDate { get; set; }

        [Required]
        [Display(Name = "Start time")]
        [StringLength(maximumLength: 63)]
        public string StartTime { get; set; }

        [Required]
        [Display(Name = "End time")]
        [StringLength(maximumLength: 63)]
        public string EndTime { get; set; }
    }
}
