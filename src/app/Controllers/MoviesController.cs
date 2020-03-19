using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle all of the /api/movies requests
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class MoviesController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDAL _dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public MoviesController(ILogger<MoviesController> logger, IDAL dal)
        {
            _logger = logger;
            _dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a JSON array of Movie objects</remarks>
        /// <param name="q">(optional) The term used to search by movie title (rings)</param>
        /// <param name="genre">(optional) Movies of a genre (Action)</param>
        /// <param name="year">(optional) Get movies by year (2005)</param>
        /// <param name="rating">(optional) Get movies with a rating >= rating (8.5)</param>
        /// <param name="actorId">(optional) Get movies by Actor Id (nm0000704)</param>
        /// <param name="pageNumber">1 based page index</param>
        /// <param name="pageSize">page size (1000 max)</param>
        /// <response code="200">JSON array of Movie objects or empty array if not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(Movie[]), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> GetMoviesAsync([FromQuery]string q = null, [FromQuery] string genre = null, [FromQuery] int year = 0, [FromQuery] double rating = 0, [FromQuery] string actorId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = Constants.DefaultPageSize)
        {
            string method = GetMethodText(q, genre, year, rating, actorId, pageNumber, pageSize);

            // validate query string parameters
            if (!ParameterValidator.Movies(HttpContext?.Request?.Query, q, genre, year, rating, actorId, pageNumber, pageSize, method, _logger, out ContentResult result))
            {
                return result;
            }

            // convert to zero based page index
            pageNumber = pageNumber > 1 ? pageNumber - 1 : 0;

            return await ResultHandler.Handle(_dal.GetMoviesByQueryAsync(q, genre, year, rating, actorId, pageNumber * pageSize, pageSize), method, Constants.MoviesControllerException, _logger).ConfigureAwait(false);
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a single JSON Movie by movieId</remarks>
        /// <param name="movieId">The movieId</param>
        /// <response code="404">movieId not found</response>
        [HttpGet("{movieId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Movie), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(void), 404)]
        public async System.Threading.Tasks.Task<IActionResult> GetMovieByIdAsync(string movieId)
        {
            string method = "GetMovieByIdAsync " + movieId;

            // validate movieId
            if (!ParameterValidator.MovieId(movieId, method, _logger, out ContentResult result))
            {
                return result;
            }

            // get movie by movieId
            return await ResultHandler.Handle(_dal.GetMovieAsync(movieId), method, Constants.MoviesControllerException, _logger).ConfigureAwait(false);
        }

        /// <summary>
        /// Add parameters to the method name if specified in the query string
        /// </summary>
        /// <param name="q"></param>
        /// <param name="genre"></param>
        /// <param name="year"></param>
        /// <param name="rating"></param>
        /// <param name="actorId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private string GetMethodText(string q, string genre, int year, double rating, string actorId, int pageNumber, int pageSize)
        {
            string method = "GetMovies";

            if (HttpContext != null && HttpContext.Request != null && HttpContext.Request.Query != null)
            {
                // add the query parameters to the method name if exists
                if (HttpContext.Request.Query.ContainsKey("q"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:q:{q}");
                }

                if (HttpContext.Request.Query.ContainsKey("genre"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:genre:{genre}");
                }

                if (HttpContext.Request.Query.ContainsKey("year"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:year:{year}");
                }

                if (HttpContext.Request.Query.ContainsKey("rating"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:rating:{rating}");
                }

                if (HttpContext.Request.Query.ContainsKey("actorId"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:actorId:{actorId}");
                }

                if (HttpContext.Request.Query.ContainsKey("pageNumber"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:pageNumber:{pageNumber}");
                }

                if (HttpContext.Request.Query.ContainsKey("pageSize"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:pageSize:{pageSize}");
                }
            }

            return method;
        }
    }
}
