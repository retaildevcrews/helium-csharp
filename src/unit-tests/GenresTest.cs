using Helium.Controllers;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
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

            OkObjectResult ok = await _controller.GetGenresAsync().ConfigureAwait(false) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<string>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.GenresCount, ie.ToList().Count);
        }
    }
}
