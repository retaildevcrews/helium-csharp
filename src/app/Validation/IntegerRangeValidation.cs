using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Helium.Validation
{
    public class IntegerRangeValidation : ValidationAttribute
    {
        private int minValue;
        private int maxValue;
        private List<string> memberName = new List<string>();

        public IntegerRangeValidation(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
                return ValidationResult.Success;

            memberName.Add(validationContext.MemberName);
            var errorMessage = $"The parameter '{memberName[0]}' should be between {minValue} and {maxValue}.";

            var isValid = (int)value >= minValue && (int)value <= maxValue;

            return isValid ? ValidationResult.Success : new ValidationResult(errorMessage, memberName);
        }
    }
}
