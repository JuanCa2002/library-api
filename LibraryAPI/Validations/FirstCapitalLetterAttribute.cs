using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Validations
{
    public class FirstCapitalLetterAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var firstLetter = value.ToString()![0].ToString();
            if (firstLetter.Equals(firstLetter.ToUpper()))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("The first letter must be capitalized");
        }
    }
}
