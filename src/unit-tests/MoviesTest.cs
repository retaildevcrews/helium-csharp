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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
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

            OkObjectResult ok = await _controller.GetMoviesAsync(string.Empty).ConfigureAwait(false) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesBySearch()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(q: AssertValues.MoviesSearchString).ConfigureAwait(false) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesSearchCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByYear()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(year: 2000).ConfigureAwait(false) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(47, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByRating()
        {

            OkObjectResult ok = await _controller.GetMoviesAsync(rating: 8.9).ConfigureAwait(false) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(3, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByGenre()
        {
            OkObjectResult ok = await _controller.GetMoviesAsync(genre: "Action").ConfigureAwait(false) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(380, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByActorId()
        {
            OkObjectResult ok = await _controller.GetMoviesAsync(actorId: AssertValues.ActorById).ConfigureAwait(false) as OkObjectResult;

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
            var res = await _controller.GetMovieByIdAsync(AssertValues.MovieById).ConfigureAwait(false);

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
            ContentResult nfRes = await _controller.GetMovieByIdAsync(AssertValues.MovieById + "00").ConfigureAwait(false) as ContentResult;

            Assert.NotNull(nfRes);
            Assert.Equal((int)System.Net.HttpStatusCode.NotFound, nfRes.StatusCode);


            ContentResult badRes = await _controller.GetMovieByIdAsync(AssertValues.BadId).ConfigureAwait(false) as ContentResult;

            Assert.NotNull(badRes);
            Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRes.StatusCode);

            badRes = await _controller.GetMovieByIdAsync(AssertValues.MovieById + "000").ConfigureAwait(false) as ContentResult;

            Assert.NotNull(badRes);
            Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, badRes.StatusCode);
        }

        [Fact]
        public async Task GetFeaturedMovie()
        {
            var list = await new MockDal().GetFeaturedMovieListAsync().ConfigureAwait(false);

            Assert.NotNull(list);
            Assert.Equal(7, list.Count);

            var res = await _controller.GetMovieByIdAsync(list[0]).ConfigureAwait(false);

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Movie mov = ok.Value as Movie;

            Assert.NotNull(mov);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(mov.MovieId), mov.PartitionKey);
            Assert.Equal(list[0], mov.MovieId);
        }
    }
}
