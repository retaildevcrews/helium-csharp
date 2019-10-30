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
        private readonly Mock<ILogger<MoviesController>> logger = new Mock<ILogger<MoviesController>>();
        private readonly MoviesController c;

        public MoviesTest()
        {
            c = new MoviesController(logger.Object, TestApp.MockDal);
        }

        [Fact]
        public async Task GetAllMovies()
        {

            OkObjectResult ok = await c.GetMoviesAsync(string.Empty) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesBySearch()
        {

            OkObjectResult ok = await c.GetMoviesAsync(q: AssertValues.MoviesSearchString) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesSearchCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByYear()
        {

            OkObjectResult ok = await c.GetMoviesAsync(year: 2000) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(9, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByRating()
        {

            OkObjectResult ok = await c.GetMoviesAsync(rating: 8.9) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(2, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByTopRated()
        {

            OkObjectResult ok = await c.GetMoviesAsync(topRated: true) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(4, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByGenre()
        {

            OkObjectResult ok = await c.GetMoviesAsync(genre: "Action") as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(19, ie.ToList<Movie>().Count);
        }

        [Fact]
        public async Task GetMoviesByActorId()
        {

            OkObjectResult ok = await c.GetMoviesAsync(actorId: AssertValues.ActorById) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            //Assert.Equal(1, ie.ToList<Movie>().Count);

            Assert.Single(ie.ToList<Movie>());
        }

        [Fact]
        public async Task GetMovieByIdPass()
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
        public async Task GetMovieByIdFail()
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

        [Fact]
        public async Task GetFeaturedMovie()
        {
            var list = await new MockDal().GetFeaturedMovieListAsync();

            Assert.NotNull(list);
            Assert.Equal(7, list.Count);

            var res = await c.GetMovieByIdAsync(list[0]);

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Movie mov = ok.Value as Movie;

            Assert.NotNull(mov);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(mov.MovieId), mov.PartitionKey);
            Assert.Equal(list[0], mov.MovieId);
        }
    }
}
