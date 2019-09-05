using Helium.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class GenresTest
    {
        private readonly Mock<ILogger<GenresController>> logger = new Mock<ILogger<GenresController>>();
        private readonly GenresController c;

        public GenresTest()
        {
            c = new GenresController(logger.Object, TestApp.MockDal);
        }

        [Fact]
        public void GetGenres()
        {

            var l = c.GetGenres().ToList();

            Assert.Equal(AssertValues.GenresCount, l.Count);

        }
    }
}
