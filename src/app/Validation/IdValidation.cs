using System;
using System.ComponentModel.DataAnnotations;

namespace Helium.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IdValidation : ValidationAttribute
    {
        private static int minimumCharacters;
        private static int maximumCharacters;
        private static string startingCharacters;

        public IdValidation(string startingCharacters, int minimumCharacters, int maximumCharacters)
        {
            IdValidation.startingCharacters = startingCharacters;
            IdValidation.minimumCharacters = minimumCharacters;
            IdValidation.maximumCharacters = maximumCharacters;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var id = (string) value;

            var isInvalid = id == null ||
                          id.Length < minimumCharacters ||
                          id.Length > maximumCharacters ||
                          id.Substring(0, 2) != startingCharacters ||
                          !int.TryParse(id.Substring(2), out var val) ||
                          val <= 0;

            string errorMessage =
                $"The parameter should start with '{startingCharacters}' and be between {minimumCharacters} and {maximumCharacters} characters in total";

            return isInvalid ? new ValidationResult(errorMessage) : ValidationResult.Success;
        }
    }
}
