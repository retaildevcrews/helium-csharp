using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using System;

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
        public IActionResult GetMovies([FromQuery]string q)
        {
            if (q == null)
            {
                q = string.Empty;
            }

            q = q.Trim();

            string method = string.IsNullOrEmpty(q) ? "GetMovies" : string.Format("SearchMovies {0}", q);

            logger.LogInformation(method, q);

            try
            {
                return Ok(dal.GetMoviesByQuery(q));
            }

            catch (DocumentClientException dce)
            {
                // log and return 500
                logger.LogError("DocumentClientException:" + method + ":{0}:{1}:{2}:{3}\r\n{4}", dce.StatusCode, dce.Error, dce.ActivityId, dce.Message, dce);

                return new ObjectResult("MoviesControllerException")
                {
                    StatusCode = Constants.ServerError
                };
            }

            catch (Exception ex)
            {
                logger.LogError(method + "\r\n{0}", ex);

                return new ObjectResult("MoviesControllerException")
                {
                    StatusCode = Constants.ServerError
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

            catch (DocumentClientException dce)
            {
                // CosmosDB API will throw an exception on an movieId not found
                if (dce.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogInformation("NotFound:GetMovieByIdAsync:{0}", movieId);

                    // return a 404
                    return NotFound();
                }
                else
                {
                    // log and return 500
                    logger.LogError("DocumentClientException:MovieByIdAsync:{0}:{1}:{2}:{3}\r\n{4}", dce.StatusCode, dce.Error, dce.ActivityId, dce.Message, dce);

                    return new ObjectResult("MovieControllerException")
                    {
                        StatusCode = Constants.ServerError
                    };
                }
            }

            catch (Exception e)
            {
                // log and return 500
                logger.LogError("Exception:GetActorByIdAsync:{0}\r\n{1}", e.Message, e);

                return new ObjectResult("MovieControllerException")
                {
                    StatusCode = Constants.ServerError
                };
            }
        }
    }
}
