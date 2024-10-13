using System.Linq;
using WebApplication9.ViewModels;
using System.ComponentModel.DataAnnotations;



namespace WebApplication9.Validations
{
    public class PsychotherapyTechniquesRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var app = (TherapistApplicationsViewModel)validationContext.ObjectInstance;
            if (app.Chosen_PsychotherapyTechniquesIds.ToList().All(tech => string.IsNullOrWhiteSpace(tech)))
            {
                return new ValidationResult(ErrorMessage == null ? "Polje je obavezno." : ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
