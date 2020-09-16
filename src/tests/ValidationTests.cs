using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CSE.Helium.Tests
{
    public class ValidationTests
    {
        [Theory]
        [InlineData(12, true)]
        [InlineData(1200, true)]
        [InlineData(10000, true)]
        [InlineData(10001, false)]
        [InlineData(10006, false)]
        [InlineData(10007, false)]
        [InlineData(10002, false)]
        [InlineData(0, false)]
        public void PageNumberInput_ValidateModel_ReturnsExpectedResult(int input, bool expectedResult)
        {
            // Arrange
            var queryParameters = new ActorQueryParameters { PageNumber = input };

            // Act
            var actualValue = IsValidProperty(queryParameters, input, "PageNumber");

            // Assert
            Assert.Equal(expectedResult, actualValue);
        }

        [Theory]
        [InlineData(12, true)]
        [InlineData(1000, true)]
        [InlineData(1001, false)]
        [InlineData(0, false)]
        public void PageSizeInput_ValidateModel_ReturnsExpectedResult(int input, bool expectedResult)
        {
            // Arrange
            var queryParameters = new ActorQueryParameters();

            // Act
            var actualValue = IsValidProperty(queryParameters, input, "PageSize");

            // Assert
            Assert.Equal(expectedResult, actualValue);
        }

        [Theory]
        [InlineData("aaa", true)]
        [InlineData("AAA", true)]
        [InlineData("12345678901234567890", true)]
        [InlineData("123456789012345678901", false)]
        [InlineData("aa", false)]
        public void GenreInput_ValidateModel_ReturnsExpectedResult(string input, bool expectedResult)
        {
            // Arrange
            var queryParameters = new MovieQueryParameters();

            // Act
            var actualValue = IsValidProperty(queryParameters, input, "Genre");

            // Assert
            Assert.Equal(expectedResult, actualValue);
        }

        [Theory]
        [InlineData(9.9, true)]
        [InlineData(0.1, true)]
        [InlineData(5, true)]
        [InlineData(999, false)]
        [InlineData(10.00001, false)]
        [InlineData(100.1, false)]
        [InlineData(-5, false)]
        public void RatingInput_ValidateModel_ReturnsExpectedResult(double input, bool expectedResult)
        {
            // Arrange
            var queryParameters = new MovieQueryParameters();

            // Act
            var actualValue = IsValidProperty(queryParameters, input, "Rating");

            // Assert
            Assert.Equal(expectedResult, actualValue);
        }

        [Theory]
        [InlineData(1874, true)]
        [InlineData(2025, true)]
        [InlineData(2001, true)]
        [InlineData(1870, false)]
        [InlineData(123, false)]
        public void YearInput_ValidateRegularExpression_ReturnsExpectedResult(int input, bool expectedResult)
        {
            // Arrange
            var yearValidation = new MovieQueryParameters();

            // Act
            var actualResult = IsValidProperty(yearValidation, input, "Year");

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("nm123456789", true)]
        [InlineData("nm12345678", true)]
        [InlineData("nm1234567", true)]
        [InlineData("nm0000000", false)]
        [InlineData("tt0000001", false)]
        [InlineData("nm1234", false)]
        [InlineData("nM12345", false)]
        [InlineData("ab132456", false)]
        [InlineData("123456789", false)]
        [InlineData("12345", false)]
        public void ActorId_ValidateRegularExpression_ReturnsExpectedResult(string input, bool expectedResult)
        {
            // Arrange
            var actorIdParameter = new ActorIdParameter();

            // Act
            var isValid = IsValidProperty(actorIdParameter, input, "ActorId");

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        [Theory]
        [InlineData("nm123456789", true)]
        [InlineData("nm12345678", true)]
        [InlineData("nm1234567", true)]
        [InlineData("nm0000000", false)]
        [InlineData("tt0000001", false)]
        [InlineData("nm1234", false)]
        [InlineData("nM12345", false)]
        [InlineData("ab132456", false)]
        [InlineData("123456789", false)]
        [InlineData("12345", false)]
        public void ActorIdInMovieQueryParameters_ValidateRegularExpression_ReturnsExpectedResult(string input, bool expectedResult)
        {
            // Arrange
            var actorIdParameter = new MovieQueryParameters();

            // Act
            var isValid = IsValidProperty(actorIdParameter, input, "ActorId");

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        [Theory]
        [InlineData("tt123456789", true)]
        [InlineData("tt12345678", true)]
        [InlineData("tt12345", true)]
        [InlineData("tt0000000", false)]
        [InlineData("nm123456789", false)]
        [InlineData("tt1234", false)]
        [InlineData("tT123456789111", false)]
        [InlineData("ab132456", false)]
        [InlineData("123456789", false)]
        [InlineData("12345", false)]
        public void MovieId_ValidateRegularExpression_ReturnsExpectedResult(string input, bool expectedResult)
        {
            // Arrange
            var movieIdParameter = new MovieIdParameter();

            // Act
            var isValid = IsValidProperty(movieIdParameter, input, "MovieId");

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        [Theory]
        [InlineData("The", true)]
        [InlineData("the matrix", true)]
        [InlineData("2001", true)]
        [InlineData("the quick brown fox jumped over the lazy dog", false)]
        [InlineData("t", false)]
        public void QueryString_ValidateRegularExpression_ReturnsExpectedResult(string input, bool expectedResult)
        {
            // Arrange
            var queryParameter = new ActorQueryParameters();

            // Act
            var isValid = IsValidProperty(queryParameter, input, "Q");

            // Assert
            Assert.Equal(expectedResult, isValid);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(55, 54)]
        [InlineData(10000, 9999)]
        public void GivenPageNumber_ValidateZeroBasedIndex_ReturnsExpectedResult(int pageNumber, int expectedResult)
        {
            // Arrange
            var queryParameters = new ActorQueryParameters { PageNumber = pageNumber };

            // Act
            var actualResult = queryParameters.GetZeroBasedPageNumber();

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        private static bool IsValidProperty(object inputObject, object input, string memberName)
        {
            var validationContext = new ValidationContext(inputObject) { MemberName = memberName };
            return Validator.TryValidateProperty(input, validationContext, null);
        }
    }
}
