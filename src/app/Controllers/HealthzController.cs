using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
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
        ///     Movies: 100\r\nActors: 531\r\nGenres: 19
        /// </remarks>
        /// <returns>200 OK or 500 Error</returns>
        /// <response code="200">returns a count of the Actors, Genres and Movies as text/plain</response>
        /// <response code="500">failed due to unexpected results</response>
        [HttpGet]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public IActionResult Healthz()
        {
            // healthcheck counts the document types

            try
            {
                logger.LogInformation("Healthz");

                // return 200 OK with payload
                return Ok(dal.GetHealthz());
            }

            catch (DocumentClientException dce)
            {
                // log and return 500
                logger.LogError("DocumentClientException:Healthz:{0}:{1}:{2}:{3}\r\n{4}", dce.StatusCode, dce.Error, dce.ActivityId, dce.Message, dce);

                return new ObjectResult("HealthzControllerException")
                {
                    StatusCode = Constants.ServerError
                };
            }

            catch (Exception ex)
            {
                // log and return 500
                logger.LogError("Exception:Healthz\r\n{0}", ex);

                return new ObjectResult("HealthzControllerException")
                {
                    StatusCode = Constants.ServerError
                };
            }
        }
    }
}
