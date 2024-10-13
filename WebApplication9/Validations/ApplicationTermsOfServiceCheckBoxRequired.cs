using System.ComponentModel.DataAnnotations;
using WebApplication9.ViewModels;

namespace WebApplication9.Validations
{
    public class ApplicationTermsOfServiceCheckBoxRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var app = (TherapistApplicationsViewModel)validationContext.ObjectInstance;
            if (!app.TermsOfService) return new ValidationResult("Field is required.");
            return ValidationResult.Success;
        }
    }
}
