using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
    public class CustomClient : IdentityUser
    {
        [MaxLength(128)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [MaxLength(128)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        public double? WebCredit { get; set; } = 0;

        [MaxLength(450)]
        [Display(Name = "Profile photo")]
        public string ProfilePhoto { get; set; }

        [Display(Name = "Year of birth")]
        public int? YearOfBirth { get; set; }

        [MaxLength(450)]
        public string SurveyId { get; set; }

        [MaxLength(450)]
        public string TherapistAccountId { get; set; }

        public double? AmountDue { get; set; } = 0;
    }
}
