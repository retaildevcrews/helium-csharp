using CSE.Helium.Validation;
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
        public void UsingCustomValidation_GivesInvalidYear_ReturnsFalse(int year, bool expectedResult)
        {
            // Arrange
            var yearValidation = new YearValidation("Foo error");

            // Act
            var actualResult = yearValidation.IsValid(year);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
