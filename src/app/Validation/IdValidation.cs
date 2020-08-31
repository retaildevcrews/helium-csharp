using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Helium.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IdValidation : ValidationAttribute
    {
        private int minimumCharacters;
        private int maximumCharacters;
        private bool allowNulls;
        private string startingCharacters;
        private List<string> memberName = new List<string>();

        public IdValidation(string startingCharacters, int minimumCharacters, int maximumCharacters, bool allowNulls)
        {
            this.startingCharacters = startingCharacters;
            this.minimumCharacters = minimumCharacters;
            this.maximumCharacters = maximumCharacters;
            this.allowNulls = allowNulls;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
                return ValidationResult.Success;

            if (allowNulls && value == null)
                return ValidationResult.Success;
            
            memberName.Add(validationContext.MemberName);
            var errorMessage = $"The parameter '{memberName[0]}' should start with '{startingCharacters}' and be between {minimumCharacters} and {maximumCharacters} characters in total";

            if (!allowNulls && value == null)
                return new ValidationResult(errorMessage, memberName);

            var id = (string) value;

            var isInvalid = id == null ||
                          id.Length < minimumCharacters ||
                          id.Length > maximumCharacters ||
                          id.Substring(0, 2) != startingCharacters ||
                          !int.TryParse(id.Substring(2), out var val) ||
                          val <= 0;
            
            return isInvalid ? new ValidationResult(errorMessage, memberName) : ValidationResult.Success;
        }
    }
}
