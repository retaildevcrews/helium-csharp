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

            OkObjectResult ok = c.GetMovies(q: AssertValues.MoviesSearchString) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.MoviesSearchCount, ie.ToList<Movie>().Count);
        }

        [Fact]
        public void GetMoviesByYear()
        {

            OkObjectResult ok = c.GetMovies(year: 2000) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(9, ie.ToList<Movie>().Count);
        }

        [Fact]
        public void GetMoviesByRating()
        {

            OkObjectResult ok = c.GetMovies(rating: 8.9) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(2, ie.ToList<Movie>().Count);
        }

        [Fact]
        public void GetMoviesByTopRated()
        {

            OkObjectResult ok = c.GetMovies(topRated: true) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(4, ie.ToList<Movie>().Count);
        }

        [Fact]
        public void GetMoviesByGenre()
        {

            OkObjectResult ok = c.GetMovies(genre: "Action") as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            Assert.Equal(19, ie.ToList<Movie>().Count);
        }

        [Fact]
        public void GetMoviesByActorId()
        {

            OkObjectResult ok = c.GetMovies(actorId: AssertValues.ActorById) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Movie>;

            Assert.NotNull(ie);

            //Assert.Equal(1, ie.ToList<Movie>().Count);

            Assert.Single(ie.ToList<Movie>());
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

        [Fact]
        public async void GetFeaturedMovie()
        {
            var list = new MockDal().GetFeaturedMovieList();

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
