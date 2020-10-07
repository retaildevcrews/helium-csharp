using System;
using System.ComponentModel.DataAnnotations;

namespace CSE.Helium.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IdValidation : ValidationAttribute
    {
        private readonly int minimumCharacters;
        private readonly int maximumCharacters;
        private readonly bool allowNulls;
        private readonly string startingCharacters;

        public IdValidation(string startingCharacters, int minimumCharacters, int maximumCharacters, bool allowNulls)
        {
            this.startingCharacters = startingCharacters;
            this.minimumCharacters = minimumCharacters;
            this.maximumCharacters = maximumCharacters;
            this.allowNulls = allowNulls;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null || allowNulls && value == null)
            {
                return ValidationResult.Success;
            }

            string errorMessage = $"The parameter '{validationContext.MemberName}' should start with '{startingCharacters}' and be between {minimumCharacters} and {maximumCharacters} characters in total";

            if (!allowNulls && value == null)
            {
                return new ValidationResult(errorMessage);
            }

            // cast value to string
            string id = (string)value;

            // check id has correct starting characters and is between min/max values specified
            bool isInvalid = id == null ||
                          id.Length < minimumCharacters ||
                          id.Length > maximumCharacters ||
                          id.Substring(0, 2) != startingCharacters ||
                          !int.TryParse(id.Substring(2), out int val) ||
                          val <= 0;

            return isInvalid ? new ValidationResult(errorMessage) : ValidationResult.Success;
        }
    }
}
