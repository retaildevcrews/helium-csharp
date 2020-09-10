using CSE.Helium.DataAccessLayer;
using CSE.Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    /// <summary>
    /// Handle all of the /api/movies requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : Controller
    {
        private readonly ILogger logger;
        private readonly IDAL dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public MoviesController(ILogger<MoviesController> logger, IDAL dal)
        {
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// Returns a JSON array of Movie objects
        /// </summary>
        /// <param name="movieQueryParameters"></param>
        [HttpGet]
        public async Task<IActionResult> GetMoviesAsync([FromQuery] MovieQueryParameters movieQueryParameters)
        {
            _ = movieQueryParameters ?? throw new ArgumentNullException(nameof(movieQueryParameters));

            // convert to zero based page index
            movieQueryParameters.PageNumber = movieQueryParameters.PageNumber > 1 ? movieQueryParameters.PageNumber - 1 : 0;

            return await ResultHandler.Handle(dal.GetMoviesAsync(
                    movieQueryParameters.Q,
                    movieQueryParameters.Genre,
                    movieQueryParameters.Year,
                    movieQueryParameters.Rating,
                    movieQueryParameters.ActorId,
                    movieQueryParameters.PageNumber * movieQueryParameters.PageSize,
                    movieQueryParameters.PageSize),
                    GetMethodText(movieQueryParameters),
                    Constants.MoviesControllerException,
                    logger)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a single JSON Movie by movieIdParameter
        /// </summary>
        /// <param name="movieIdParameter"></param>
        /// <returns></returns>
        [HttpGet("{movieId}")]
        public async System.Threading.Tasks.Task<IActionResult> GetMovieByIdAsync([FromRoute]MovieIdParameter movieIdParameter)
        {
            _ = movieIdParameter ?? throw new ArgumentNullException(nameof(movieIdParameter));

            string method = nameof(GetMovieByIdAsync) + movieIdParameter.MovieId;

            // get movie by movieIdParameter
            return await ResultHandler.Handle(dal.GetMovieAsync(movieIdParameter.MovieId), method, "Movie Not Found", logger).ConfigureAwait(false);
        }

        /// <summary>
        /// Add parameters to the method name if specified in the query string
        /// </summary>
        /// <param name="movieQueryParameters"></param>
        /// <returns></returns>
        private string GetMethodText(MovieQueryParameters movieQueryParameters)
        {
            string method = "GetMovies";

            if (HttpContext != null && HttpContext.Request != null && HttpContext.Request.Query != null)
            {
                // add the query parameters to the method name if exists
                if (HttpContext.Request.Query.ContainsKey("q"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:q:{movieQueryParameters.Q}");
                }

                if (HttpContext.Request.Query.ContainsKey("genre"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:genre:{movieQueryParameters.Genre}");
                }

                if (HttpContext.Request.Query.ContainsKey("year"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:year:{movieQueryParameters.Year}");
                }

                if (HttpContext.Request.Query.ContainsKey("rating"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:rating:{movieQueryParameters.Rating}");
                }

                if (HttpContext.Request.Query.ContainsKey("actorId"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:actorId:{movieQueryParameters.ActorId}");
                }

                if (HttpContext.Request.Query.ContainsKey("pageNumber"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:pageNumber:{movieQueryParameters.PageNumber}");
                }

                if (HttpContext.Request.Query.ContainsKey("pageSize"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:pageSize:{movieQueryParameters.PageSize}");
                }
            }

            return method;
        }
    }
}
