using CSE.Helium.DataAccessLayer;
using Helium.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    /// <summary>
    /// Handle all of the /api/movies requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : BaseController
    {
        private readonly ILogger logger;
        private readonly IDAL dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public MoviesController(ILogger<MoviesController> logger, IDAL dal, IConfiguration configuration) : base(logger, dal, configuration)
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

            // todo: encapsulate getmethodtext and const
            return await Handle(
                RetryCosmosPolicy.ExecuteAsync(() => dal.GetMoviesAsync(movieQueryParameters)), movieQueryParameters.GetMethodText(HttpContext), Constants.MoviesControllerException, logger)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a single JSON Movie by movieIdParameter
        /// </summary>
        /// <param name="movieIdParameter"></param>
        /// <returns></returns>
        [HttpGet("{movieId}")]
        public async System.Threading.Tasks.Task<IActionResult> GetMovieByIdAsync([FromRoute] MovieIdParameter movieIdParameter)
        {
            _ = movieIdParameter ?? throw new ArgumentNullException(nameof(movieIdParameter));

            string method = nameof(GetMovieByIdAsync) + movieIdParameter.MovieId;

            return await Handle(
                RetryCosmosPolicy.ExecuteAsync(() => dal.GetMovieAsync(movieIdParameter.MovieId)), method, "Movie Not Found", logger)
                .ConfigureAwait(false);
        }
    }
}
