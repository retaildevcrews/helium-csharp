using Helium.DataAccessLayer;
using Helium.Model;
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
        /// Returns a HealthzSuccess or HealthzError as application/json
        /// </remarks>
        /// <returns>200 OK or 503 Error</returns>
        /// <response code="200">Returns a HealthzSuccess as application/json</response>
        /// <response code="503">Returns a HealthzError as application/json due to unexpected results</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(HealthzSuccess), 200)]
        [ProducesResponseType(typeof(HealthzError), 503)]
        public IActionResult Healthz()
        {
            // healthcheck counts the document types

            try
            {
                HealthzSuccess res = new HealthzSuccess();

                logger.LogInformation("Healthz");

                HealthzSuccessDetails s = dal.GetHealthz();

                res.details.cosmosDb.details = s;

                // return 200 OK with payload
                return Ok(res);
            }

            catch (DocumentClientException dce)
            {
                // log and return 503
                logger.LogError("DocumentClientException:Healthz:{0}:{1}:{2}:{3}\r\n{4}", dce.StatusCode, dce.Error, dce.ActivityId, dce.Message, dce);

                HealthzError e = new HealthzError();
                e.details.cosmosDb.details.Error = dce.Message;

                return new ObjectResult(e)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable
                };
            }

            catch (Exception ex)
            {
                // log and return 500
                logger.LogError("Exception:Healthz\r\n{0}", ex);

                HealthzError e = new HealthzError();
                e.details.cosmosDb.details.Error = ex.Message;

                return new ObjectResult(e)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable
                };
            }
        }
    }
}
