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
        private readonly ILogger _logger;
        private readonly IDAL _dal;
        private readonly Random _rand = new Random(DateTime.Now.Millisecond);
        private List<string> _featuredMovies;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public FeaturedController(ILogger<FeaturedController> logger, IDAL dal)
        {
            _logger = logger;
            _dal = dal;
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
            string method = nameof(GetFeaturedMovieAsync);
            string notFound = "NotFound:" + method;

            _logger.LogInformation(method);

            try
            {
                // get a random movie from the featured movie list
                if (_featuredMovies == null || _featuredMovies.Count == 0)
                {
                    _featuredMovies = await _dal.GetFeaturedMovieListAsync().ConfigureAwait(false);
                }

                if (_featuredMovies != null && _featuredMovies.Count > 0)
                {
                    // get random featured movie by movieId
                    // CosmosDB API will throw an exception on a bad movieId
                    Movie m = await _dal.GetMovieAsync(_featuredMovies[_rand.Next(0, _featuredMovies.Count - 1)]).ConfigureAwait(false);

                    return Ok(m);
                }

                return NotFound();
            }

            catch (CosmosException ce)
            {
                // CosmosDB API will throw an exception on an movieId not found
                if (ce.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation(notFound);

                    // return a 404
                    return NotFound();
                }
                else
                {
                    // log and return Cosmos status code
                    _logger.LogError($"CosmosException:{method}:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                    return new ContentResult
                    {
                        Content = Constants.FeaturedControllerException,
                        StatusCode = (int)ce.StatusCode
                    };
                }
            }

            catch (Exception e)
            {
                // log and return 500
                _logger.LogError($"Exception:{method}:{e.Message}\n{e}");

                return new ContentResult
                {
                    Content = Constants.FeaturedControllerException,
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
