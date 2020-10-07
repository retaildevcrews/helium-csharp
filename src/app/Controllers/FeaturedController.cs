// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    /// <summary>
    /// Handle /api/featured/movie requests
    /// </summary>
    [Route("api/[controller]")]
    public class FeaturedController : Controller
    {
        private readonly ILogger logger;
        private readonly IDAL dal;
        private readonly Random rand = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public FeaturedController(ILogger<FeaturedController> logger, IDAL dal)
        {
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// Returns a random movie from the featured movie list as a JSON Movie
        /// </summary>
        /// <response code="200">OK</response>
        /// <returns>IActionResult</returns>
        [HttpGet("movie")]
        public async Task<IActionResult> GetFeaturedMovieAsync()
        {
            string method = nameof(GetFeaturedMovieAsync);
            logger.LogInformation(method);

            List<string> featuredMovies = await dal.GetFeaturedMovieListAsync().ConfigureAwait(false);

            if (featuredMovies != null && featuredMovies.Count > 0)
            {
                // get random featured movie by movieId
                string movieId = featuredMovies[rand.Next(0, featuredMovies.Count - 1)];

                // get movie by movieId
                return await ResultHandler.Handle(dal.GetMovieAsync(movieId), method, Constants.FeaturedControllerException, logger).ConfigureAwait(false);
            }

            return NotFound();
        }
    }
}
