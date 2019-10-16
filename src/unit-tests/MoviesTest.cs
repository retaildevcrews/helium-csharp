using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
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

            OkObjectResult ok = c.GetMovies(string.Empty) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public void GetMoviesBySearch()
        {

            OkObjectResult ok = c.GetMovies(AssertValues.MoviesSearchString) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesSearchCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async void GetMovieByIdPass()
        {
            // do not use c.GetMovieByIdAsync().Result
            // due to thread syncronization issues with build clients, it is not reliable
            var res = await c.GetMovieByIdAsync(AssertValues.MovieById);

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Movie mov = ok.Value as Movie;

            Assert.NotNull(mov);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(mov.MovieId), mov.PartitionKey);
            Assert.Equal(AssertValues.MovieByIdTitle, mov.Title);
        }

        [Fact]
        public async void GetMovieByIdFail()
        {
            // this will fail GetPartitionKey
            NotFoundResult nf = await c.GetMovieByIdAsync(AssertValues.BadId) as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal((int)System.Net.HttpStatusCode.NotFound, nf.StatusCode);

            // this will fail search
            nf = await c.GetMovieByIdAsync(AssertValues.MovieById + "000") as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal((int)System.Net.HttpStatusCode.NotFound, nf.StatusCode);
        }
    }
}
