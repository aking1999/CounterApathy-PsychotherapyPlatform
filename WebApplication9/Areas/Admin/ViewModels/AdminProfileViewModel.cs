using Framework.Helpers.ExtensionMethods;
using Framework.Validations;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class AdminProfileViewModel
    {
        private string firstName;
        private string lastName;

        [DataType(DataType.Upload)]
        [Display(Name = "Profile photo")]
        [AllowedFileExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile ProfilePhoto { get; set; }

        [Required(ErrorMessage = "Field is required.")]
        [Display(Name = "First name", Prompt = "First name")]
        [StringLength(maximumLength: 32, ErrorMessage = "Enter up to 32 characters.")]
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                firstName = value.RemoveSpecialCharacters();
            }
        }

        [Required(ErrorMessage = "Field is required.")]
        [Display(Name = "Last name", Prompt = "Last name")]
        [StringLength(maximumLength: 32, ErrorMessage = "Enter up to 32 characters.")]
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                lastName = value.RemoveSpecialCharacters();
            }
        }

        [Display(Name = "Email", Prompt = "Email")]
        public string Email { get; set; }

        [Display(Name = "Web credit")]
        public string WebCredit { get; set; }

        [Display(Name = "Year of birth")]
        [Range(1932, 2007, ErrorMessage = "Must be between 1932 and 2007.")]
        public int? YearOfBirth { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone number", Prompt = "Phone number")]
        [RegularExpression(@"[\d ?]{5,13}", ErrorMessage = "Enter 5 to 13 digits.")]
        public string PhoneNumber { get; set; }
    }
}
