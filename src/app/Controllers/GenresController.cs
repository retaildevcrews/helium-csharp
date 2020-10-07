// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    /// <summary>
    /// Handle the single /api/genres requests
    /// </summary>
    [Route("api/[controller]")]
    public class GenresController : Controller
    {
        private readonly ILogger logger;
        private readonly IDAL dal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public GenresController(ILogger<GenresController> logger, IDAL dal)
        {
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// Returns a JSON string array of Genre
        /// </summary>
        /// <response code="200">JSON array of strings or empty array if not found</response>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> GetGenresAsync()
        {
            // get list of genres as list of string
            return await ResultHandler.Handle(dal.GetGenresAsync(), nameof(GetGenresAsync), Constants.GenresControllerException, logger).ConfigureAwait(false);
        }
    }
}
