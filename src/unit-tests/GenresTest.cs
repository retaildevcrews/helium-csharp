using Helium.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class GenresTest
    {
        private readonly Mock<ILogger<GenresController>> _logger = new Mock<ILogger<GenresController>>();
        private readonly GenresController _controller;

        public GenresTest()
        {
            _controller = new GenresController(_logger.Object, TestApp.MockDal);
        }

        [Fact]
        public async Task GetGenres()
        {

            OkObjectResult ok = await _controller.GetGenresAsync() as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<string>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.GenresCount, ie.ToList().Count);
        }
    }
}
