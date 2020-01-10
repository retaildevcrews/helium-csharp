using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle the /healthz* requests
    /// </summary>
    [Route("[controller]")]
    public class HealthzController : Controller
    {
        private readonly ILogger _logger;
        private readonly ILogger<CosmosHealthCheck> _hcLogger;
        private readonly IDAL _dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="dal">data access layer</param>
        /// <param name="hcLogger">HealthCheck logger</param>
        public HealthzController(ILogger<HealthzController> logger, IDAL dal, ILogger<CosmosHealthCheck> hcLogger)
        {
            _logger = logger;
            _hcLogger = hcLogger;
            _dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a plain text health status (Healthy, Degraded or Unhealthy)</remarks>
        [HttpGet]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), 200)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Method Name")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Method logs and handles all exceptions")]
        public async Task<IActionResult> RunHealthzAsync()
        {
            // get list of genres as list of string
            _logger.LogInformation(nameof(RunHealthzAsync));

            HealthCheckResult res = await RunCosmosHealthCheck().ConfigureAwait(false);

            HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            return new ObjectResult(res.Status.ToString())
            {
                StatusCode = res.Status == HealthStatus.Unhealthy ? (int)System.Net.HttpStatusCode.ServiceUnavailable : (int)System.Net.HttpStatusCode.OK
            };
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns an IETF (draft) health+json representation of the full Health Check</remarks>
        [HttpGet("ietf")]
        [Produces("application/health+json")]
        [ProducesResponseType(typeof(CosmosHealthCheck), 200)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Method logs and handles all exceptions")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "log messages")]
        public async System.Threading.Tasks.Task RunIetfAsync()
        {
            _logger.LogInformation(nameof(RunHealthzAsync));

            DateTime dt = DateTime.UtcNow;

            HealthCheckResult res = await RunCosmosHealthCheck().ConfigureAwait(false);

            HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            await CosmosHealthCheck.IetfResponseWriter(HttpContext, res, DateTime.UtcNow.Subtract(dt)).ConfigureAwait(false);
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a JSON representation of the full Health Check</remarks>
        [HttpGet("dotnet")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CosmosHealthCheck), 200)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Method logs and handles all exceptions")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "log messages")]
        public async System.Threading.Tasks.Task<IActionResult> RunHealthzJsonAsync()
        {
            _logger.LogInformation(nameof(RunHealthzAsync));

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
            CosmosHealthCheck chk = new CosmosHealthCheck(_hcLogger, _dal);

            return await chk.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
        }
    }
}
