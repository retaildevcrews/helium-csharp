using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Helium.Controllers
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
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public GenresController(ILogger<GenresController> logger, IDAL dal)
        {
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a JSON string array of Genre</remarks>
        /// <response code="200">json array of strings or empty array if not found</response>
        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<string> GetGenres()
        {
            // get list of genres as list of string
            logger.LogInformation("GetGenres");

            return dal.GetGenres();
        }
    }
}
