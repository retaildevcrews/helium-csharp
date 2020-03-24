using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle /api/featured/movie requests
    /// </summary>
    [Route("api/[controller]")]
    public class FeaturedController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDAL _dal;
        private readonly Random _rand = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public FeaturedController(ILogger<FeaturedController> logger, IDAL dal)
        {
            _logger = logger;
            _dal = dal;
        }

        /// <summary>
        /// Returns a random movie from the featured movie list as a JSON Movie
        /// </summary>
        /// <response code="200">OK</response>
        [HttpGet("movie")]
        public async Task<IActionResult> GetFeaturedMovieAsync()
        {
            string method = nameof(GetFeaturedMovieAsync);
            _logger.LogInformation(method);

            List<string> featuredMovies = await _dal.GetFeaturedMovieListAsync().ConfigureAwait(false);

            if (featuredMovies != null && featuredMovies.Count > 0)
            {
                // get random featured movie by movieId
                string movieId = featuredMovies[_rand.Next(0, featuredMovies.Count - 1)];

                // get movie by movieId
                return await ResultHandler.Handle(_dal.GetMovieAsync(movieId), method, Constants.FeaturedControllerException, _logger).ConfigureAwait(false);
            }

            return NotFound();
        }
    }
}
