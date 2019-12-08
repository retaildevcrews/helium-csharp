using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class MoviesTest
    {
        private readonly Mock<ILogger<MoviesController>> _logger = new Mock<ILogger<MoviesController>>();
        private readonly MoviesController _controller;

        public MoviesTest()
        {
            _controller = new MoviesController(_logger.Object, TestApp.MockDal);
        }

        [Fact]
        public async Task GetAllMovies()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(string.Empty) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesBySearch()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(q: AssertValues.MoviesSearchString) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesSearchCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByYear()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(year: 2000) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(47, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByRating()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(rating: 8.9) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(3, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByTopRated()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(topRated: true) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(6, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByGenre()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(genre: "Action") as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(380, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByActorId()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(actorId: AssertValues.ActorById) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(49, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMovieByIdPass()
        {
            // do not use c.GetMovieByIdAsync().Result
            // due to thread syncronization issues with build clients, it is not reliable
            var res = await _controller.GetMovieByIdAsync(AssertValues.MovieById);

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Movie mov = ok.Value as Movie;

            Assert.NotNull(mov);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(mov.MovieId), mov.PartitionKey);
            Assert.Equal(AssertValues.MovieByIdTitle, mov.Title);
        }

        [Fact]
        public async Task GetMovieByIdFail()
        {
            // this will fail GetPartitionKey
            NotFoundResult nf = await _controller.GetMovieByIdAsync(AssertValues.BadId) as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal((int)System.Net.HttpStatusCode.NotFound, nf.StatusCode);

            // this will fail search
            nf = await _controller.GetMovieByIdAsync(AssertValues.MovieById + "000") as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal((int)System.Net.HttpStatusCode.NotFound, nf.StatusCode);
        }

        [Fact]
        public async Task GetFeaturedMovie()
        {
            var list = await new MockDal().GetFeaturedMovieListAsync();

            Assert.NotNull(list);
            Assert.Equal(7, list.Count);

            var res = await _controller.GetMovieByIdAsync(list[0]);

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Movie mov = ok.Value as Movie;

            Assert.NotNull(mov);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(mov.MovieId), mov.PartitionKey);
            Assert.Equal(list[0], mov.MovieId);
        }
    }
}
