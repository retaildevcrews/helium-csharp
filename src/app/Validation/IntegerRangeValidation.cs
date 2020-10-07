using System.ComponentModel.DataAnnotations;

namespace CSE.Helium.Validation
{
    public class IntegerRangeValidation : ValidationAttribute
    {
        private readonly int minValue;
        private readonly int maxValue;

        public IntegerRangeValidation(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                return ValidationResult.Success;
            }

            string errorMessage = $"The parameter '{validationContext.MemberName}' should be between {minValue} and {maxValue}.";

            bool isValid = (int)value >= minValue && (int)value <= maxValue;

            return isValid ? ValidationResult.Success : new ValidationResult(errorMessage);
        }
    }
}
