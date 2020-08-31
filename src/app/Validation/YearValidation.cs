using System;
using System.ComponentModel.DataAnnotations;

namespace CSE.Helium.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class YearValidation : ValidationAttribute
    {
        private const int StartYear = 1874;
        private static readonly int EndYear = DateTime.UtcNow.AddYears(5).Year;
        private readonly string errorMessage = $"The parameter should be between {StartYear} and {EndYear}.";

        public YearValidation()
        {
            // required for unit test
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var isValid = (int) value >= StartYear && (int) value <= EndYear || (int) value == 0;

            return !isValid ? new ValidationResult(errorMessage) : ValidationResult.Success;
        }
    }
}
