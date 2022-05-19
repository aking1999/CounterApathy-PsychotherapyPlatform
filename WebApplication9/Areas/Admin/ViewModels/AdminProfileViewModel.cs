using Framework.Validations;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class AdminProfileViewModel
    {
        [Display(Name = "Profile photo")]
        [AllowedFileExtensions(new string[] { ".jpg", ".jpeg", ".png", ".svg" })]
        public IFormFile ProfilePhoto { get; set; }

        public string Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }
      
        [Required]
        [Display(Name = "First name", Prompt = "First name")]
        [StringLength(maximumLength: 127, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(127)]
        [Display(Name = "Last name", Prompt = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email", Prompt = "Email")]
        public string Email { get; set; }

        public string NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string ConcurrencyStamp { get; set; }

        [Display(Name = "Web credit")]
        public string WebCredit { get; set; }

        [Required]
        [Display(Name = "Year of birth")]
        [Range(1950, 2021, ErrorMessage = "Number must be between 1950 and 2021.")]
        public int? YearOfBirth { get; set; }

        [Phone]
        [Required]
        [StringLength(100)]
        [Display(Name = "Phone number", Prompt = "Phone number")]
        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }
    }
}
