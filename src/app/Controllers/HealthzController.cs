using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle the single /api/genres requests
    /// </summary>
    [Route("[controller]")]
    public class HealthzController : Controller
    {
        private readonly ILogger _logger;
        private readonly ILogger<Helium.CosmosHealthCheck> _hcLogger;
        private readonly IDAL _dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        /// <param name="hcLogger"></param>
        public HealthzController(ILogger<GenresController> logger, IDAL dal, ILogger<Helium.CosmosHealthCheck> hcLogger)
        {
            _logger = logger;
            _hcLogger = hcLogger;
            _dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a JSON string array of Genre</remarks>
        /// <response code="200">JSON array of strings or empty array if not found</response>
        [HttpGet]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string[]), 200)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Method Name")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Method logs and handles all exceptions")]
        public async Task<IActionResult> RunHealthzAsync()
        {
            // get list of genres as list of string
            _logger.LogInformation(nameof(RunHealthzAsync));

            try
            {
                HealthCheckResult res = await RunCosmosHealthCheck().ConfigureAwait(false);

                return Ok(res.Status.ToString());
            }

            catch (Exception ex)
            {
                // log and return 500
                _logger.LogError($"Exception:RunHealthz\n{ex}");

                return new ObjectResult(Constants.HealthzControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a single JSON Movie by movieId</remarks>
        /// <param name="type"></param>
        /// <response code="404">movieId not found</response>
        [HttpGet("{type}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Helium.CosmosHealthCheck), 200)]
        [ProducesResponseType(typeof(void), 404)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Method logs and handles all exceptions")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "log messages")]
        public async System.Threading.Tasks.Task<IActionResult> RunHealthz(string type)
        {
            _logger.LogInformation(nameof(RunHealthzAsync));

            if (type != null &&
                (type.Equals("json", StringComparison.OrdinalIgnoreCase) ||
                type.Equals("ietf", StringComparison.OrdinalIgnoreCase)))
            {
                DateTime dt = DateTime.UtcNow;

                HealthCheckResult res = await RunCosmosHealthCheck().ConfigureAwait(false);


                var ietf = CosmosHealthCheck.IetfResponseWriter(HttpContext, res);

                return Ok(ietf);
            }
            else
            {
                return NotFound();
            }
        }


        private async Task<HealthCheckResult> RunCosmosHealthCheck()
        {
            CosmosHealthCheck chk = new CosmosHealthCheck(_hcLogger, _dal);

            return await chk.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
        }
    }
}
