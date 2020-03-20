﻿using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle the single /api/genres requests
    /// </summary>
    [Route("api/[controller]")]
    public class GenresController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDAL _dal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public GenresController(ILogger<GenresController> logger, IDAL dal)
        {
            _logger = logger;
            _dal = dal;
        }

        /// <summary>
        /// Returns a JSON string array of Genre
        /// </summary>
        /// <response code="200">JSON array of strings or empty array if not found</response>
        [HttpGet]
        public async Task<IActionResult> GetGenresAsync()
        {
            // get list of genres as list of string
            return await ResultHandler.Handle(_dal.GetGenresAsync(), nameof(GetGenresAsync), Constants.GenresControllerException, _logger).ConfigureAwait(false);
        }
    }
}
