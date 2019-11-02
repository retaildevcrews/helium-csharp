using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle the /healthz request
    /// </summary>
    [Route("[controller]")]
    public class HealthzController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDAL _dal;
        private readonly IConfiguration _config;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        /// <param name="config">IConfiguration</param>
        public HealthzController(ILogger<HealthzController> logger, IDAL dal, IConfiguration config)
        {
            _logger = logger;
            _dal = dal;
            _config = config;
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
        public async Task<IActionResult> HealthzAsync()
        {
            // healthcheck counts the document types

            try
            {
                HealthzSuccess res = new HealthzSuccess();

                _logger.LogInformation("Healthz");

                HealthzSuccessDetails s = await _dal.GetHealthzAsync();

                if (_config != null)
                {
                    s.CosmosKey = _config.GetValue<string>(Constants.CosmosKey).PadRight(5).Substring(0, 5).Trim() + "...";
                }

                res.details.cosmosDb.details = s;

                // return 200 OK with payload
                return Ok(res);
            }

            catch (CosmosException ce)
            {
                // log and return 503
                _logger.LogError($"CosmosException:Healthz:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                HealthzError e = new HealthzError();
                e.details.cosmosDb.details.Error = ce.Message;

                return new ObjectResult(e)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable
                };
            }

            catch (System.AggregateException age)
            {
                var root = age.GetBaseException();

                if (root == null)
                {
                    root = age;
                }

                HealthzError e = new HealthzError();
                e.details.cosmosDb.details.Error = root.Message;

                // log and return 500
                _logger.LogError($"AggregateException|Healthz|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new ObjectResult(e)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception ex)
            {
                // log and return 500
                _logger.LogError($"Exception:Healthz\n{ex}");

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
