using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests
{
    public class FeaturedTest
    {
        private readonly Mock<ILogger<FeaturedController>> logger = new Mock<ILogger<FeaturedController>>();
        private readonly FeaturedController c;

        public FeaturedTest()
        {
            c = new FeaturedController(logger.Object, TestApp.MockDal);
        }

        [Fact]
        public async void GetFeaturedMovie()
        {
            var list = new MockDal().GetFeaturedMovieList();

            Assert.NotNull(list);
            Assert.Equal(7, list.Count);

            var res = await c.GetFeaturedMovieAsync();

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Movie mov = ok.Value as Movie;

            Assert.NotNull(mov);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(mov.MovieId), mov.PartitionKey);
        }
    }
}
