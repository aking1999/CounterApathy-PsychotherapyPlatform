using System.ComponentModel.DataAnnotations;
using WebApplication9.ViewModels;

namespace WebApplication9.Validations
{
    public class ApplicationPrivacyPolicyCheckBoxRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var app = (TherapistApplicationsViewModel)validationContext.ObjectInstance;
            if (!app.PrivacyPolicy) return new ValidationResult("Field is required.");
            return ValidationResult.Success;
        }
    }
}
