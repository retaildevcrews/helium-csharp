using CSE.Helium.Model;
using CSE.Helium.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CSE.Helium.Tests
{
    public class ValidationTests
    {
        [Theory]
        [InlineData(1874, true)]
        [InlineData(2025, true)]
        [InlineData(2001, true)]
        [InlineData(1870, false)]
        [InlineData(123, false)]
        public void VaryYearInput_ValidateRegularExpression_ReturnsExpectedResult(int year, bool expectedResult)
        {
            // Arrange
            var yearValidation = new YearValidation();

            // Act
            var actualResult = yearValidation.IsValid(year);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("nm123456789", 0)]
        [InlineData("nm12345678", 0)]
        [InlineData("nm1234567", 0)]
        [InlineData("nm0000000", 1)]
        [InlineData("tt0000001", 1)]
        [InlineData("nm1234", 1)]
        [InlineData("nM12345", 1)]
        [InlineData("ab132456", 1)]
        [InlineData("123456789", 1)]
        [InlineData("12345", 1)]
        public void VaryActorId_ValidateRegularExpression_ReturnsExpectedResult(string input, int errorCount)
        {
            // Arrange
            var actorIdParameter = new ActorIdParameter {ActorId = input};
            
            // Act
            var actorIdResults = ValidateModel(actorIdParameter);
            
            var matchesActorErrorCount = actorIdResults.Count.Equals(errorCount);
            
            // Assert
            Assert.True(matchesActorErrorCount);
            Assert.True(actorIdResults.Count.Equals(errorCount));
        }

        [Theory]
        [InlineData("tt12345678910", 0)]
        [InlineData("tt123456789", 0)]
        [InlineData("tt12345678", 0)]
        [InlineData("tt1234567", 0)]
        [InlineData("tt0000000", 1)]
        [InlineData("nm123456789", 1)]
        [InlineData("tt123456", 1)]
        [InlineData("tt12345", 1)]
        [InlineData("tt1234", 1)]
        [InlineData("tT123456789111", 1)]
        [InlineData("ab132456", 1)]
        [InlineData("123456789", 1)]
        [InlineData("12345", 1)]
        public void VaryMovieId_ValidateRegularExpression_ReturnsExpectedResult(string input, int errorCount)
        {
            // Arrange
            var movieIdParameter = new MovieIdParameter() { MovieId = input };
            
            // Act
            var validationResults = ValidateModel(movieIdParameter);
            var matchesErrorCount = validationResults.Count.Equals(errorCount);

            // Assert
            Assert.True(matchesErrorCount);
        }

        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}
