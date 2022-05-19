using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Framework.Validations;

namespace WebApplication9.ViewModels
{
    public class RegisterInputViewModel
    {
        [Display(Name = "Profile Photo")]
        [AllowedFileExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile ProfilePhoto { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(maximumLength: 127, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(127)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Phone]
        [Required]
        [StringLength(100)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Year of Birth")]
        public int YearOfBirth { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
