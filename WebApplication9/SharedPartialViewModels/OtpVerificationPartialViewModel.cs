using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.SharedPartialViewModels
{
    public class OtpVerificationPartialViewModel
    {
        [Required]
        [Range(0, 9)]
        public int Digit1 { get; set; }

        [Required]
        [Range(0, 9)]
        public int Digit2 { get; set; }

        [Required]
        [Range(0, 9)]
        public int Digit3 { get; set; }

        [Required]
        [Range(0, 9)]
        public int Digit4 { get; set; }

        [Required]
        [Range(0, 9)]
        public int Digit5 { get; set; }

        [Required]
        [Range(0, 9)]
        public int Digit6 { get; set; }
    }
}
