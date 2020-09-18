using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSE.Helium;
using CSE.Helium.DataAccessLayer;
using CSE.KeyVault;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace CSE.Helium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : Controller
    {
        private readonly IDAL dal;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        protected AsyncRetryPolicy RetryCosmosPolicy { get; private set; }

        public BaseController(ILogger logger, IDAL dal, IConfiguration configuration )
        {
            this.dal = dal;
            this.configuration = configuration;
            this.logger = logger;
            RetryCosmosPolicy = GetCosmosRetryPolicy();
        }

        /// <summary>
        /// Handle an IActionResult request from a controller
        /// </summary>
        /// <typeparam name="T">type of result</typeparam>
        /// <param name="task">async task (usually the Cosmos query)</param>
        /// <param name="method">method name for logging</param>
        /// <param name="errorMessage">error message to log on error</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public static async Task<IActionResult> Handle<T>(Task<T> task, string method, string errorMessage, ILogger logger)
        {
            // log the request
            logger.LogInformation(method);

            // return exception if task is null
            if (task == null)
            {
                logger.LogError($"Exception:{method} task is null");

                return CreateResult(errorMessage, HttpStatusCode.InternalServerError);
            }

            try
            {
                // return an OK object result
                return new OkObjectResult(await task.ConfigureAwait(false));
            }

            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning($"CosmosNotFound:{method}");
                }
                else
                {
                    logger.LogError($"{ce}\nCosmosException:{method}:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}");
                }

                return CreateResult(errorMessage, ce.StatusCode);
            }

            catch (Exception ex)
            {
                // log and return exception
                logger.LogError($"{ex}\nException:{method}:{ex.Message}");

                // return 500 error
                return CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// ContentResult factory
        /// </summary>
        /// <param name="message">string</param>
        /// <param name="statusCode">int</param>
        /// <returns></returns>
        public static JsonResult CreateResult(string message, HttpStatusCode statusCode)
        {
            return new JsonResult(new ErrorResult { Error = statusCode, Message = message })
            {
                StatusCode = (int)statusCode
            };
        }


        private AsyncRetryPolicy GetCosmosRetryPolicy()
        {
            return Policy.Handle<CosmosException>(e => e.StatusCode == HttpStatusCode.Unauthorized)
                .WaitAndRetryAsync(1, retryAttempt =>
                 {
                     var timeToWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                     logger.LogInformation("Reloading configurations to check if Key was rotated.");
                     
                     //Reload the configurations.
                     (configuration as IConfigurationRoot).Reload();

                     logger.LogInformation("Refresh cosmos connection with upadated secret.");
                     dal.Reconnect(new Uri(configuration[Constants.CosmosUrl]), configuration[Constants.CosmosKey], configuration[Constants.CosmosDatabase], configuration[Constants.CosmosCollection]).ConfigureAwait(false);

                     logger.LogInformation($"Waiting {timeToWait.TotalSeconds} seconds");
                     return timeToWait;
                 }
                );
        }
    }
}
