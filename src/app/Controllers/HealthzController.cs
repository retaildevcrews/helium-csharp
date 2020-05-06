using CSE.Helium.DataAccessLayer;
using CSE.Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    /// <summary>
    /// Handle the /healthz* requests
    ///
    /// Cache results to prevent monitoring from overloading service
    /// </summary>
    [Route("[controller]")]
    [ResponseCache(Duration = Constants.HealthzCacheDuration)]
    public class HealthzController : Controller
    {
        private readonly ILogger logger;
        private readonly ILogger<CosmosHealthCheck> hcLogger;
        private readonly IDAL dal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="dal">data access layer</param>
        /// <param name="hcLogger">HealthCheck logger</param>
        public HealthzController(ILogger<HealthzController> logger, IDAL dal, ILogger<CosmosHealthCheck> hcLogger)
        {
            this.logger = logger;
            this.hcLogger = hcLogger;
            this.dal = dal;
        }

        /// <summary>
        /// Returns a plain text health status (Healthy, Degraded or Unhealthy)
        /// </summary>
        [HttpGet]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> RunHealthzAsync()
        {
            // get list of genres as list of string
            logger.LogInformation(nameof(RunHealthzAsync));

            HealthCheckResult res = await RunCosmosHealthCheck().ConfigureAwait(false);

            HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            return new ContentResult
            {
                Content = IetfCheck.ToIetfStatus(res.Status),
                StatusCode = res.Status == HealthStatus.Unhealthy ? (int)System.Net.HttpStatusCode.ServiceUnavailable : (int)System.Net.HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Returns an IETF (draft) health+json representation of the full Health Check
        /// </summary>
        [HttpGet("ietf")]
        [Produces("application/health+json")]
        [ProducesResponseType(typeof(CosmosHealthCheck), 200)]
        public async System.Threading.Tasks.Task RunIetfAsync()
        {
            logger.LogInformation(nameof(RunHealthzAsync));

            DateTime dt = DateTime.UtcNow;

            HealthCheckResult res = await RunCosmosHealthCheck().ConfigureAwait(false);

            HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            await CosmosHealthCheck.IetfResponseWriter(HttpContext, res, DateTime.UtcNow.Subtract(dt)).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a JSON representation of the full Health Check
        /// </summary>
        [HttpGet("dotnet")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CosmosHealthCheck), 200)]
        public async System.Threading.Tasks.Task<IActionResult> RunHealthzJsonAsync()
        {
            logger.LogInformation(nameof(RunHealthzAsync));

            HealthCheckResult res = await RunCosmosHealthCheck().ConfigureAwait(false);

            HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            return new ObjectResult(res)
            {
                StatusCode = res.Status == HealthStatus.Unhealthy ? (int)System.Net.HttpStatusCode.ServiceUnavailable : (int)System.Net.HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Run the health check
        /// </summary>
        /// <returns>HealthCheckResult</returns>
        private async Task<HealthCheckResult> RunCosmosHealthCheck()
        {
            CosmosHealthCheck chk = new CosmosHealthCheck(hcLogger, dal);

            return await chk.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
        }
    }
}
