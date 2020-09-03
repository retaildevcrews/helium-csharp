using System;
using System.Threading.Tasks;
using CSE.Helium.Validation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests
{
    public class ValidationProblemDetailsTests : BaseTest
    {
        private readonly IServiceCollection serviceCollection;

        public ValidationProblemDetailsTests()
        {
            serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public void ValidationProblemDetailsResult_ExecuteResultAsync_ShouldThrow()
        {
            var validationProblemDetailsResult = new ValidationProblemDetailsResult();
            Func<Task> funcResult = async () => await validationProblemDetailsResult.ExecuteResultAsync(default);

            funcResult.Should().Throw<ArgumentNullException>();
        }
    }
}
