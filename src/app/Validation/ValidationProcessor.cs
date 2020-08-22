using CSE.Helium.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text;

namespace CSE.Helium.Validation
{
    public static class ValidationProcessor
    {
        /// <summary>
        /// validate search parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal static IActionResult GetAndLogInvalidSearchParameter(string method, ILogger logger)
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
        internal static IActionResult GetAndLogInvalidPageSizeParameter(string method, ILogger logger)
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
        internal static IActionResult GetAndLogInvalidPageNumberParameter(string method, ILogger logger)
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
        internal static IActionResult GetAndLogInvalidActorIdParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid Actor ID parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid Actor ID parameter", "actorId",
                InnerErrorType.InvalidActorIDParameter, null, null);
        }

        /// <summary>
        /// validate genre parameter
        /// </summary>
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal static IActionResult GetAndLogInvalidMovieGenreParameter(string method, ILogger logger)
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
        internal static IActionResult GetAndLogInvalidMovieYearParameter(string method, ILogger logger)
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
        internal static IActionResult GetAndLogInvalidMovieRatingParameter(string method, ILogger logger)
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
        internal static IActionResult GetAndLogInvalidMovieIdParameter(string method, ILogger logger)
        {
            logger.LogWarning($"InvalidParameter|{method}|Invalid Movie ID parameter");

            return BuildJsonResponse(HttpStatusCode.BadRequest, "BadArgument", "Invalid Movie ID parameter", "movieId",
                InnerErrorType.InvalidMovieIDParameter, null, null);
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

            switch (innerErrorType)
            {
                case InnerErrorType.InvalidSearchParameter:
                    sb.Append($"\"code\": \"{innerErrorType}\",\n");
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
                    sb.Append($"\"code\": \"{innerErrorType}\",\n");
                    sb.Append($"\"minValue\": {minValue},\n");
                    sb.Append($"\"maxValue\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"integer\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidPageNumberParameter:
                    sb.Append($"\"code\": \"{innerErrorType}\",\n");
                    sb.Append($"\"minValue\": {minValue},\n");
                    sb.Append($"\"maxValue\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"integer\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidGenreParameter:
                    sb.Append($"\"code\": \"{innerErrorType}\",\n");
                    sb.Append($"\"minLength\": {minValue},\n");
                    sb.Append($"\"maxLength\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"integer\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidYearParameter:
                    sb.Append($"\"code\": \"{innerErrorType}\",\n");
                    sb.Append($"\"minValue\": {minValue},\n");
                    sb.Append($"\"maxValue\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"integer\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidRatingParameter:
                    sb.Append($"\"code\": \"{innerErrorType}\",\n");
                    sb.Append($"\"minValue\": {minValue},\n");
                    sb.Append($"\"maxValue\": {maxValue},\n");
                    sb.Append("\"valueTypes\": [\n");
                    sb.Append("\"double\",\n");
                    sb.Append("]\n");
                    break;
                case InnerErrorType.InvalidActorIDParameter:
                    sb.Append($"\"code\": \"{innerErrorType}\"\n");
                    break;
                case InnerErrorType.InvalidMovieIDParameter:
                    sb.Append($"\"code\": \"{innerErrorType}\"\n");
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
