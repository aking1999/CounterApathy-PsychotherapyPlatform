using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Validations
{
    public class OnlyLettersAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var input = value as string;

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (input.All(c => char.IsLetter(c)))
                    return ValidationResult.Success;
                else return new ValidationResult("Enter letters only.");
            }
            // !!! mozda se ovde desi greska, u tom slucaju treba da stoji ValidationResult.Success;
            else return new ValidationResult("Invalid input.");
        }
    }
}
