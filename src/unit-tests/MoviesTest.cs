using Helium;
using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class MoviesTest
    {
        private readonly Mock<ILogger<MoviesController>> logger = new Mock<ILogger<MoviesController>>();
        private readonly MoviesController c;

        public MoviesTest()
        {
            c = new MoviesController(logger.Object, TestApp.MockDal);
        }

        [Fact]
        public void GetAllMovies()
        {

            var l = c.GetMovies(string.Empty).ToList();

            Assert.Equal(AssertValues.MoviesCount, l.Count);

        }

        [Fact]
        public void GetMoviesBySearch()
        {

            var l = c.GetMovies(AssertValues.MoviesSearchString).ToList();

            Assert.Equal(AssertValues.MoviesSearchCount, l.Count);

        }

        [Fact]
        public async void GetMovieByIdPass()
        {
            // do not use c.GetMovieByIdAsync().Result
            // due to thread syncronization issues with build clients, it is not reliable
            var res = await c.GetMovieByIdAsync(AssertValues.MovieById);

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Movie m = ok.Value as Movie;

            Assert.NotNull(m);
            Assert.Equal(AssertValues.MovieByIdTitle, m.Title);
        }

        [Fact]
        public async void GetMovieByIdFail()
        {
            var res = await c.GetMovieByIdAsync(AssertValues.BadId);

            NotFoundResult nf = res as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal(Constants.NotFound, nf.StatusCode);
        }
    }
}
