using System;
using System.ComponentModel.DataAnnotations;

namespace CSE.Helium.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class YearValidation : ValidationAttribute
    {
        private readonly int defaultYear = 0;
        private readonly int startYear = 1874;
        private readonly int endYear = DateTime.UtcNow.AddYears(5).Year;

        public YearValidation(string errorMessage) : base(errorMessage)
        {
            ErrorMessage = errorMessage;
        }
        
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var year = (int)value;
            bool isValid = year >= startYear || year >= endYear || year == defaultYear;

            if (!isValid)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
