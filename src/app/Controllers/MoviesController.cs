using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle all of the /api/actors requests
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
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
        /// </summary>
        /// <remarks>Returns a json array of all Movie objects</remarks>
        /// <param name="q">(optional) The term used to search by movie title</param>
        /// <response code="200">json array of Movie objects or empty array if not found</response>
        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<Movie> GetMovies([FromQuery]string q)
        {
            // limit by search string
            if (!string.IsNullOrEmpty(q))
            {
                logger.LogInformation("GetMoviesBySearch {0}", q);

                return dal.GetMoviesByQuery(q);
            }

            logger.LogInformation("GetMoviesByAll");

            // get all movies
            return dal.GetMovies();
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a single JSON Movie by movieId</remarks>
        /// <param name="movieId">The movieId</param>
        /// <response code="404">movieId not found</response>
        [HttpGet("{movieId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Movie), 200)]
        [ProducesResponseType(typeof(void), 404)]
        public async System.Threading.Tasks.Task<IActionResult> GetMovieByIdAsync(string movieId)
        {
            logger.LogInformation("GetMovieByIdAsync {0}", movieId);

            try
            {
                // get movie by movieId
                // CosmosDB API will throw an exception on a bad movieId
                Movie m = await dal.GetMovieAsync(movieId);

                return Ok(m);
            }
            catch (System.Exception e)
            {
                if (e.Message == "Invalid id")
                {
                    logger.LogError("NotFound: GetMovieByIdAsync {0}", movieId);

                    // return 404
                    return NotFound();
                }
                else
                {
                    logger.LogError("Exception: GetMovieByIdAsync {0}", e);

                    return new ObjectResult("MoviesController exception")
                    {
                        StatusCode = Constants.ServerError
                    };
                }
            }
        }
    }
}
