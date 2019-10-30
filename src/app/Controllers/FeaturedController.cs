using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle /api/featured/movie requests
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FeaturedController : Controller
    {
        private readonly ILogger logger;
        private readonly IDAL dal;
        private readonly Random rand = new Random(DateTime.Now.Millisecond);
        private List<string> featuredMovies;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public FeaturedController(ILogger<FeaturedController> logger, IDAL dal)
        {
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a random movie from the featured movie list as a JSON Movie</remarks>
        /// <response code="200">OK</response>
        [HttpGet("movie")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Movie), 200)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> GetFeaturedMovieAsync()
        {
            logger.LogInformation("GetFeaturedMovieAsync");

            try
            {
                // get a random movie from the featured movie list
                if (featuredMovies == null || featuredMovies.Count == 0)
                {
                    featuredMovies = await dal.GetFeaturedMovieListAsync();
                }

                if (featuredMovies != null && featuredMovies.Count > 0)
                {
                    // get random featured movie by movieId
                    // CosmosDB API will throw an exception on a bad movieId
                    Movie m = await dal.GetMovieAsync(featuredMovies[rand.Next(0, featuredMovies.Count - 1)]);

                    return Ok(m);
                }

                return NotFound();
            }

            // movieId isn't well formed
            catch (ArgumentException)
            {
                logger.LogInformation("NotFound:GetFeaturedMovieAsync");

                // return a 404
                return NotFound();
            }

            catch (CosmosException ce)
            {
                // CosmosDB API will throw an exception on an movieId not found
                if (ce.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogInformation("NotFound:GetFeaturedMovieAsync");

                    // return a 404
                    return NotFound();
                }
                else
                {
                    // log and return 500
                    logger.LogError("CosmosException:GetFeaturedMovieAsync:{0}:{1}:{2}:{3}\r\n{4}", ce.StatusCode, ce.ActivityId, ce.Message, ce.ToString());

                    return new ObjectResult(Constants.FeaturedControllerException)
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
                logger.LogError($"AggregateException|GetFeaturedMovieAsync|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new ObjectResult(Constants.FeaturedControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception e)
            {
                // log and return 500
                logger.LogError("Exception:GetFeaturedMovieAsync:{0}\r\n{1}", e.Message, e);

                return new ObjectResult(Constants.FeaturedControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
