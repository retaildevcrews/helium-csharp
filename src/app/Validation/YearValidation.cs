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

        public YearValidation(string errorMessage) : base(errorMessage) => ErrorMessage = errorMessage;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var isValid = (int)value >= startYear || (int)value >= endYear || (int)value == defaultYear;

            return !isValid ? new ValidationResult(ErrorMessage) : ValidationResult.Success;
        }
    }
}
