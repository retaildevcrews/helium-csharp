using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle the /healthz request
    /// </summary>
    [Route("[controller]")]
    public class HealthzController : Controller
    {
        private readonly ILogger logger;
        private readonly IDAL dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public HealthzController(ILogger<HealthzController> logger, IDAL dal)
        {
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// The health check end point
        /// </summary>
        /// <remarks>
        /// Returns a count of the Actors, Genres and Movies as text/plain
        /// 
        ///     Expected Response:
        /// 
        ///     Movies: 100\r\nActors: 553\r\nGenres: 20
        /// </remarks>
        /// <returns>200 OK or 400 Error</returns>
        /// <response code="200">returns a count of the Actors, Genres and Movies as text/plain</response>
        /// <response code="400">failed due to unexpected results</response>
        [HttpGet]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(void), 400)]
        public IActionResult Healthz()
        {
            // healthcheck counts the document types
            // Should return: "Movies: 100\r\nActors: 553\r\nGenres: 20" as text/plain

            try
            {
                string res = string.Empty;

                logger.LogInformation("Healthz");

                // return 200 OK with payload
                return Ok(dal.GetHealthz());
            }
            catch (Exception ex)
            {
                logger.LogError("Healthz Exception: {0}", ex);

                return new ObjectResult("healthz exception")
                {
                    StatusCode = Constants.ServerError
                };
            }
        }
    }
}
