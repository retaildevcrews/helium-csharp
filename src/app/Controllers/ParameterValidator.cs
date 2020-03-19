using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Validate the query string parameters
    /// </summary>
    public static class ParameterValidator
    {
        /// <summary>
        /// validate query string parameters common between Actors and Movies
        /// </summary>
        /// <param name="query">IQueryCollection</param>
        /// <param name="q">search</param>
        /// <param name="pageNumber">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="message">error message</param>
        /// <returns></returns>
        public static bool Common(IQueryCollection query, string q, int pageNumber, int pageSize, string method, ILogger logger, out ContentResult result)
        {
            result = null;

            // no query string
            if (query == null)
            {
                return true;
            }

            // validate q (search)
            if (query.ContainsKey("q"))
            {
                if (q == null || q.Length < 2 || q.Length > 20)
                {
                    result = GetAndLogBadParam("Invalid q (search) parameter", method, logger);
                    return false;
                }
            }

            // validate page number
            if (query.ContainsKey("pageNumber"))
            {
                if (!int.TryParse(query["pageNumber"], out int val) || val != pageNumber || pageNumber < 1 || pageNumber > 10000)
                {
                    result = GetAndLogBadParam("Invalid PageNumber parameter", method, logger);
                    return false;
                }
            }

            // validate page size
            if (query.ContainsKey("pageSize"))
            {
                if (!int.TryParse(query["pageSize"], out int val) || val != pageSize || pageSize < 1 || pageSize > 1000)
                {
                    result = GetAndLogBadParam("Invalid PageSize parameter", method, logger);
                    return false;
                }
            }

            return true;
        }

        public static ContentResult GetAndLogBadParam(string message, string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|{message}");

            return new ContentResult
            {
                Content = message,
                StatusCode = (int)System.Net.HttpStatusCode.BadRequest
            };
        }

        /// <summary>
        /// validate query string parameters for Movies
        /// </summary>
        /// <param name="query">IQueryCollection</param>
        /// <param name="q">search</param>
        /// <param name="genre">genre</param>
        /// <param name="year">year</param>
        /// <param name="rating">rating</param>
        /// <param name="actorId">actorId</param>
        /// <param name="pageNumber">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="message">error message</param>
        /// <returns></returns>
        public static bool Movies(IQueryCollection query, string q, string genre, int year, double rating, string actorId, int pageNumber, int pageSize, string method, ILogger logger, out ContentResult result)
        {
            result = null;

            // no query string
            if (query == null)
            {
                return true;
            }

            // validate q, page number and page size
            if (!Common(query, q, pageNumber, pageSize, method, logger, out result))
            {
                return false;
            }

            // validate genre
            if (query.ContainsKey("genre"))
            {
                if (genre == null || genre.Length < 3 || genre.Length > 20)
                {
                    result = GetAndLogBadParam("Invalid Genre parameter", method, logger);
                    return false;
                }
            }

            // validate year
            if (query.ContainsKey("year"))
            {
                if (!int.TryParse(query["year"], out int val) || val != year || year < 1874 || year > DateTime.UtcNow.Year + 5)
                {
                    result = GetAndLogBadParam("Invalid Year parameter", method, logger);
                    return false;
                }
            }

            // validate rating
            if (query.ContainsKey("rating"))
            {
                if (!double.TryParse(query["rating"], out double val) || val != rating || rating < 0 || rating > 10)
                {
                    result = GetAndLogBadParam("Invalid Rating parameter", method, logger);
                    return false;
                }
            }

            // validate actorId
            if (query.ContainsKey("actorId"))
            {
                if (!ActorId(actorId, method, logger, out result))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// validate actorId
        /// </summary>
        /// <param name="actorId">actorId</param>
        /// <param name="message">error message</param>
        /// <returns></returns>
        public static bool ActorId(string actorId, string method, ILogger logger, out ContentResult result)
        {
            result = null;

            // validate actorId
            if (actorId == null ||
                actorId.Length < 7 ||
                actorId.Length > 11 ||
                actorId.Substring(0, 2) != "nm" ||
                !int.TryParse(actorId.Substring(2), out int val) ||
                val <= 0)
            {
                result = GetAndLogBadParam("Invalid Actor ID parameter", method, logger);
                return false;
            }

            return true;
        }

        /// <summary>
        /// validate movieId
        /// </summary>
        /// <param name="movieId">movieId</param>
        /// <param name="message">error message</param>
        /// <returns></returns>
        public static bool MovieId(string movieId, string method, ILogger logger, out ContentResult result)
        {
            // validate movieId
            if (movieId == null ||
                movieId.Length < 7 ||
                movieId.Length > 11 ||
                movieId.Substring(0, 2) != "tt" ||
                !int.TryParse(movieId.Substring(2), out int val) ||
                val <= 0)
            {
                result = GetAndLogBadParam("Invalid Movie ID parameter", method, logger);
                return false;
            }

            result = null;
            return true;
        }
    }

    public static class ResultHandler
    {
        public static async Task<IActionResult> Handle<T>(Task<T> task, string method, string errorMessage, ILogger logger)
        {
            logger.LogInformation(method);

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
                return new OkObjectResult(await task.ConfigureAwait(false));
            }

            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                logger.LogError($"CosmosException:{method}:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                return new ContentResult
                {
                    Content = errorMessage,
                    StatusCode = (int)ce.StatusCode
                };
            }

            catch (Exception ex)
            {
                // log and return exception
                logger.LogError($"Exception:{method}\n{ex}");

                return new ContentResult
                {
                    Content = errorMessage,
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

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
