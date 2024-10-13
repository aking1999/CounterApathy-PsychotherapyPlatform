using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApplication9.Areas.Therapist.ViewModels;

namespace WebApplication9.Validations
{
    public class ContactMethodsTherapistProfileRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var app = (TherapistProfileViewModel)validationContext.ObjectInstance;
            if (app.Chosen_ContactMethodsIds.ToList().All(spec => string.IsNullOrWhiteSpace(spec)))
            {
                return new ValidationResult(ErrorMessage == null ? "Field is required." : ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
