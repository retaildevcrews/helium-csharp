using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly ILogger logger;
        private readonly IDAL dal;
        private readonly Random rand = new Random(DateTime.Now.Millisecond);

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
        /// <param name="q">(optional) The term used to search by movie title (rings)</param>
        /// <param name="genre">(optional) Movies of a genre (Action)</param>
        /// <param name="year">(optional) Get movies by year (2005)</param>
        /// <param name="rating">(optional) Get movies with a rating >= rating (8.5)</param>
        /// <param name="topRated">(optional) Get top rated movies (true)</param>
        /// <param name="actorId">(optional) Get movies by Actor Id (nm0000704)</param>
        /// <response code="200">json array of Movie objects or empty array if not found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Movie[]), 200)]
        public async Task<IActionResult> GetMoviesAsync([FromQuery]string q = "", [FromQuery] string genre = "", [FromQuery] int year = 0, [FromQuery] double rating = 0, [FromQuery] bool topRated = false, [FromQuery] string actorId = "")
        {
            string method = string.IsNullOrEmpty(q) ? "GetMovies" : string.Format("SearchMovies {0}", q);

            logger.LogInformation(method);

            try
            {
                return Ok(await dal.GetMoviesByQueryAsync(q, genre, year, rating, topRated, actorId));
            }

            catch (CosmosException ce)
            {
                // log and return 500
                logger.LogError("CosmosException:" + method + ":{0}:{1}:{2}:{3}\r\n{4}", ce.StatusCode, ce.ActivityId, ce.Message, ce.ToString());

                return new ObjectResult(Constants.MoviesControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (System.AggregateException age)
            {
                var root = age.GetBaseException();

                if (root == null)
                {
                    root = age;
                }

                // log and return 500
                logger.LogError($"AggregateException|{method}|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new ObjectResult(Constants.MoviesControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception ex)
            {
                logger.LogError(method + "\r\n{0}", ex);

                return new ObjectResult(Constants.MoviesControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
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

            // movieId isn't well formed
            catch (ArgumentException)
            {
                logger.LogInformation("NotFound:GetMovieByIdAsync:{0}", movieId);

                // return a 404
                return NotFound();
            }

            catch (CosmosException ce)
            {
                // CosmosDB API will throw an exception on an movieId not found
                if (ce.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogInformation("NotFound:GetMovieByIdAsync:{0}", movieId);

                    // return a 404
                    return NotFound();
                }
                else
                {
                    // log and return 500
                    logger.LogError("CosmosException:MovieByIdAsync:{0}:{1}:{2}:{3}\r\n{4}", ce.StatusCode, ce.ActivityId, ce.Message, ce.ToString());

                    return new ObjectResult(Constants.MoviesControllerException)
                    {
                        StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                    };
                }
            }

            catch (System.AggregateException age)
            {
                var root = age.GetBaseException();

                if (root == null)
                {
                    root = age;
                }

                // log and return 500
                logger.LogError($"AggregateException|MovieByIdAsync|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new ObjectResult(Constants.MoviesControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception e)
            {
                // log and return 500
                logger.LogError("Exception:GetActorByIdAsync:{0}\r\n{1}", e.Message, e);

                return new ObjectResult(Constants.MoviesControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
