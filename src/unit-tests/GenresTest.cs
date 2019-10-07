using Helium.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
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

            OkObjectResult ok = c.GetGenres() as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<string>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.GenresCount, ie.ToList().Count);

        }
    }
}
