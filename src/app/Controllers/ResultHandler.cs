using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Helium.Controllers
{
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

            // return exception of task is null
            if (task == null)
            {
                logger.LogError($"Exception:{method}\ntask is null");

                return new ContentResult
                {
                    Content = errorMessage,
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
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
                    logger.LogError($"CosmosException:{method}:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");
                }

                return CreateResult(errorMessage, (int)ce.StatusCode);
            }

            catch (Exception ex)
            {
                // log and return exception
                logger.LogError($"Exception:{method}\n{ex}");

                // return 500 error
                return CreateResult("Internal Server Error", (int)System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// ContentResult factory
        /// </summary>
        /// <param name="message">string</param>
        /// <param name="statusCode">int</param>
        /// <returns></returns>
        public static ContentResult CreateResult(string message, int statusCode)
        {
            return new ContentResult
            {
                Content = message,
                StatusCode = statusCode
            };
        }
    }
}
