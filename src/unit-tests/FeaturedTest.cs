using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class FeaturedTest
    {
        private readonly Mock<ILogger<FeaturedController>> _logger = new Mock<ILogger<FeaturedController>>();
        private readonly FeaturedController _controller;

        public FeaturedTest()
        {
            _controller = new FeaturedController(_logger.Object, TestApp.MockDal);
        }

        [Fact]
        public async Task GetFeaturedMovie()
        {
            var list = await new MockDal().GetFeaturedMovieListAsync();

            Assert.NotNull(list);
            Assert.Equal(7, list.Count);

            var res = await _controller.GetFeaturedMovieAsync();

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Movie mov = ok.Value as Movie;

            Assert.NotNull(mov);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(mov.MovieId), mov.PartitionKey);
        }
    }
}
