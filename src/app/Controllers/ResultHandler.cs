using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    public class ErrorResult
    {
        public int Status => (int)Error;
        public string Message { get; set; }
        public HttpStatusCode Error { get; set; }
    }
    /// <summary>
    /// Handles query requests from the controllers
    /// </summary>
    public static class ResultHandler
    {
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
                if (ce.StatusCode == System.Net.HttpStatusCode.NotFound)
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
    }
}
