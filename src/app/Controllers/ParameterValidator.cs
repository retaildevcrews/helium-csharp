using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using Helium.ErrorHandler;
using System.Net;

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
        public static JsonResult Common(IQueryCollection query, string q, int pageNumber, int pageSize, string method, ILogger logger)
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

        private static JsonResult GetAndLogBadParam(string message, string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|{message}");
            
            return new JsonResult(new ErrorResult { Error = System.Net.HttpStatusCode.BadRequest, Message = message })
            {
                StatusCode = (int)System.Net.HttpStatusCode.BadRequest
            };
        }

        private static JsonResult GetAndLogInvalidSearchParameter(string method, ILogger logger)
        {
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const InnerErrorType innerErrorType = InnerErrorType.SearchParameter;
            const string message = "Invalid q (search) parameter";
            const string errorCode = "BadArgument";
            const string target = "q";

            return CreateAndFormatError(logger, method, innerErrorType, message, statusCode, errorCode, target);
        }

        private static JsonResult GetAndLogInvalidPageSizeParameter(string method, ILogger logger)
        {
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const InnerErrorType innerErrorType = InnerErrorType.PageSizeParameter;
            const string message = "Invalid PageSize parameter";
            const string errorCode = "BadArgument";
            const string target = "pageSize";

            return CreateAndFormatError(logger, method, innerErrorType, message, statusCode, errorCode, target);
        }

        private static JsonResult GetAndLogInvalidPageNumberParameter(string method, ILogger logger)
        {
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const InnerErrorType innerErrorType = InnerErrorType.PageNumberParameter;
            const string message = "Invalid PageNumber parameter";
            const string errorCode = "BadArgument";
            const string target = "pageNumber";

            return CreateAndFormatError(logger, method, innerErrorType, message, statusCode, errorCode, target);
        }

        private static JsonResult GetAndLogInvalidActorIdParameter(string method, ILogger logger)
        {
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const InnerErrorType innerErrorType = InnerErrorType.ActorIdParameter;
            const string message = "Invalid Actor ID parameter";
            const string errorCode = "BadArgument";
            const string target = "actorId";

            return CreateAndFormatError(logger, method, innerErrorType, message, statusCode, errorCode, target);
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
        public static JsonResult Movies(IQueryCollection query, string q, string genre, int year, double rating, string actorId, int pageNumber, int pageSize, string method, ILogger logger)
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

        private static JsonResult GetAndLogInvalidGenreParameter(string method, ILogger logger)
        {
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const InnerErrorType innerErrorType = InnerErrorType.ActorIdParameter;
            const string message = "Invalid Genre parameter";
            const string errorCode = "BadArgument";
            const string target = "genre";

            return CreateAndFormatError(logger, method, innerErrorType, message, statusCode, errorCode, target);
        }

        private static JsonResult GetAndLogInvalidYearParameter(string method, ILogger logger)
        {
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const InnerErrorType innerErrorType = InnerErrorType.YearParameter;
            const string message = "Invalid Year parameter";
            const string errorCode = "BadArgument";
            const string target = "year";

            return CreateAndFormatError(logger, method, innerErrorType, message, statusCode, errorCode, target);
        }

        private static JsonResult GetAndLogInvalidRatingParameter(string method, ILogger logger)
        {
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const InnerErrorType innerErrorType = InnerErrorType.RatingParameter;
            const string message = "Invalid Rating parameter";
            const string errorCode = "BadArgument";
            const string target = "rating";

            return CreateAndFormatError(logger, method, innerErrorType, message, statusCode, errorCode, target);
        }

        private static JsonResult GetAndLogInvalidMovieIdParameter(string method, ILogger logger)
        {
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const InnerErrorType innerErrorType = InnerErrorType.RatingParameter;
            const string message = "Invalid Movie ID parameter";
            const string errorCode = "BadArgument";
            const string target = "movieId";

            return CreateAndFormatError(logger, method, innerErrorType, message, statusCode, errorCode, target);
        }

        /// <summary>
        /// validate actorId
        /// </summary>
        /// <param name="actorId">actorId</param>
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public static JsonResult ActorId(string actorId, string method, ILogger logger)
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
        public static JsonResult MovieId(string movieId, string method, ILogger logger)
        {
            // validate movieId
            if (movieId == null ||
                movieId.Length < 7 ||
                movieId.Length > 11 ||
                movieId.Substring(0, 2) != "tt" ||
                !int.TryParse(movieId.Substring(2), out int val) ||
                val <= 0)
            {
                return GetAndLogBadParam("Invalid Movie ID parameter", method, logger);
            }

            return null;
        }

        private static JsonResult CreateAndFormatError(ILogger logger, string method, InnerErrorType innerErrorType, string message,
            HttpStatusCode httpStatusCode, string errorCode, string target)
        {
            logger.LogWarning($"InvalidParameter|{method}|{message}");

            var httpInnerError = new InnerError(innerErrorType);
            var httpError = new HttpErrorType(errorCode, httpInnerError, message, (int)httpStatusCode, target);
            var errorResponse = new ErrorResponse(httpError);

            return new JsonResult(errorResponse)
            {
                StatusCode = (int)httpStatusCode
            };
        }
    }
}
