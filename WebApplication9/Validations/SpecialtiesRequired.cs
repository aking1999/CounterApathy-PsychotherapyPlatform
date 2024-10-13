using System.Linq;
using WebApplication9.ViewModels;
using System.ComponentModel.DataAnnotations;



namespace WebApplication9.Validations
{
    public class SpecialtiesRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var app = (TherapistApplicationsViewModel)validationContext.ObjectInstance;
            if (app.Chosen_SpecialtiesIds.ToList().All(spec => string.IsNullOrWhiteSpace(spec)))
            {
                return new ValidationResult(ErrorMessage == null ? "Polje je obavezno." : ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
