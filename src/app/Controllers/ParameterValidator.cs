using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text;
using CSE.Helium.Enumerations;

namespace CSE.Helium.Controllers
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
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public static IActionResult Common(IQueryCollection query, string q, int pageNumber, int pageSize, string method, ILogger logger)
        {
            // no query string
            if (query == null)
            {
                return null;
            }

            // validate q (search)
            if (query.ContainsKey("q"))
            {
                if (q == null || q.Length < 2 || q.Length > 20)
                {
                    return GetAndLogInvalidSearchParameter(method, logger);
                }
            }

            // validate page number
            if (query.ContainsKey("pageNumber"))
            {
                if (!int.TryParse(query["pageNumber"], out int val) || val != pageNumber || pageNumber < 1 || pageNumber > 10000)
                {
                    return GetAndLogInvalidPageNumberParameter(method, logger);
                }
            }

            // validate page size
            if (query.ContainsKey("pageSize"))
            {
                if (!int.TryParse(query["pageSize"], out int val) || val != pageSize || pageSize < 1 || pageSize > 1000)
                {
                    return GetAndLogInvalidPageSizeParameter(method, logger);
                }
            }

            return null;
        }

        /// <summary>
        /// validate search parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IActionResult GetAndLogInvalidSearchParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid q (search) parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid q (search) parameter", "q",
                InnerErrorType.InvalidSearchParameter, 2, 20);
        }

        /// <summary>
        /// validate PageSize parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IActionResult GetAndLogInvalidPageSizeParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid PageSize parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid PageSize parameter", "pageSize",
                InnerErrorType.InvalidPageSizeParameter, 1, 1000);
        }

        /// <summary>
        /// validate PageNumber parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IActionResult GetAndLogInvalidPageNumberParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid PageNumber parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid PageNumber parameter", "pageNumber", 
                InnerErrorType.InvalidPageNumberParameter, 1, 10000);
        }

        /// <summary>
        /// validate actorId parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IActionResult GetAndLogInvalidActorIdParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid Actor ID parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid Actor ID parameter", "actorId",
                InnerErrorType.InvalidActorIDParameter, null, null);
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
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public static IActionResult Movies(IQueryCollection query, string q, string genre, int year, double rating, string actorId, int pageNumber, int pageSize, string method, ILogger logger)
        {
            // no query string
            if (query == null)
            {
                return null;
            }

            // validate q, page number and page size
            var result = Common(query, q, pageNumber, pageSize, method, logger);
            if (result != null)
            {
                return result;
            }

            // validate genre
            if (query.ContainsKey("genre"))
            {
                if (genre == null || genre.Length < 3 || genre.Length > 20)
                {
                    return GetAndLogInvalidGenreParameter(method, logger);
                }
            }

            // validate year
            if (query.ContainsKey("year"))
            {
                if (!int.TryParse(query["year"], out int val) || val != year || year < 1874 || year > DateTime.UtcNow.Year + 5)
                {
                    return GetAndLogInvalidYearParameter(method, logger);
                }
            }

            // validate rating
            if (query.ContainsKey("rating"))
            {
                if (!double.TryParse(query["rating"], out double val) || val != rating || rating < 0 || rating > 10)
                {
                    return GetAndLogInvalidRatingParameter(method, logger);
                }
            }

            // validate actorId
            if (query.ContainsKey("actorId"))
            {
                result = ActorId(actorId, method, logger);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// validate genre parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IActionResult GetAndLogInvalidGenreParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid Genre parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid Genre parameter", "genre",
                InnerErrorType.InvalidGenreParameter, 3, 20);
        }

        /// <summary>
        /// validate year parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IActionResult GetAndLogInvalidYearParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid Year parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid Year parameter", "year",
                InnerErrorType.InvalidYearParameter, 1874, 2025);
        }

        /// <summary>
        /// validate movie rating parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IActionResult GetAndLogInvalidRatingParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid Rating parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid Rating parameter", "rating",
                InnerErrorType.InvalidRatingParameter, 0, 10);
        }

        /// <summary>
        /// validate movie ID parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IActionResult GetAndLogInvalidMovieIdParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid Movie ID parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid Movie ID parameter", "movieId",
                InnerErrorType.InvalidMovieIDParameter, null, null);
        }

        /// <summary>
        /// validate actorId
        /// </summary>
        /// <param name="actorId">actorId</param>
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public static IActionResult ActorId(string actorId, string method, ILogger logger)
        {
            // validate actorId
            if (actorId == null ||
                actorId.Length < 7 ||
                actorId.Length > 11 ||
                actorId.Substring(0, 2) != "nm" ||
                !int.TryParse(actorId.Substring(2), out int val) ||
                val <= 0)
            {
                return GetAndLogInvalidActorIdParameter(method, logger);
            }

            return null;
        }

        /// <summary>
        /// validate movieId
        /// </summary>
        /// <param name="movieId">movieId</param>
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public static IActionResult MovieId(string movieId, string method, ILogger logger)
        {
            // validate movieId
            if (movieId == null ||
                movieId.Length < 7 ||
                movieId.Length > 11 ||
                movieId.Substring(0, 2) != "tt" ||
                !int.TryParse(movieId.Substring(2), out int val) ||
                val <= 0)
            {
                return GetAndLogInvalidMovieIdParameter(method, logger);
            }

            return null;
        }

        /// <summary>
        /// creates JSON response using StringBuilder given inputs
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <param name="target"></param>
        /// <param name="innerErrorType"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        private static IActionResult BuildJsonResponse(HttpStatusCode statusCode, string errorCode, string errorMessage, string target,
            InnerErrorType innerErrorType, object minValue, object maxValue)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("\"error\": {\n");
            sb.Append($"\"code\": \"{errorCode}\",\n");
            sb.Append($"\"message\": \"{errorMessage}\",\n");
            sb.Append($"\"statusCode\": {(int)statusCode},\n");
            sb.Append($"\"target\": \"{target}\",\n");
            sb.Append("\"innererror\": {\n");
            sb.Append($"\"code\": \"{innerErrorType}\",\n");

            switch (innerErrorType)
            {
                case InnerErrorType.InvalidSearchParameter:
                    sb.Append($"\"minLength\": {minValue},\n");
                    sb.Append($"\"maxLength\": {maxValue},\n");
                    sb.Append("\"characterTypes\": [\n");
                    sb.Append("\"lowerCase\",\n");
                    sb.Append("\"upperCase\",\n");
                    sb.Append("\"number\",\n");
                    sb.Append("\"symbol\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidPageSizeParameter:
                    sb.Append($"\"minValue\": {minValue},\n");
                    sb.Append($"\"maxValue\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"integer\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidPageNumberParameter:
                    sb.Append($"\"minValue\": {minValue},\n");
                    sb.Append($"\"maxValue\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"integer\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidGenreParameter:
                    sb.Append($"\"minLength\": {minValue},\n");
                    sb.Append($"\"maxLength\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"integer\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidYearParameter:
                    sb.Append($"\"minValue\": {minValue},\n");
                    sb.Append($"\"maxValue\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"integer\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidRatingParameter:
                    sb.Append($"\"minValue\": {minValue},\n");
                    sb.Append($"\"maxValue\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"double\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidActorIDParameter:
                    break;
                case InnerErrorType.InvalidMovieIDParameter:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(innerErrorType), innerErrorType, null);
            }

            sb.Append("}\n");
            sb.Append("}\n");
            sb.Append("}");

            return new ContentResult()
            {
                StatusCode = (int)statusCode,
                ContentType = "application/json",
                Content = sb.ToString()
            };
        }
    }
}
