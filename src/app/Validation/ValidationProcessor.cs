using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CSE.Helium.Validation
{
    internal static class ValidationProcessor
    {
        /// <summary>
        /// creates JSON response using StringBuilder given inputs
        /// </summary>
        /// <param name="context"></param>
        /// <param name="target"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static string WriteJsonWithStringBuilder(ActionContext context, ILogger logger)
        {
            var modelStateEntries = context.ModelState.Where(
                e => e.Value.Errors.Count > 0).ToArray();
            var last = modelStateEntries.Last();

            var sb = new StringBuilder();

            sb.Append($"{{\"type\": \"http://www.example.com/validation-error\",\n");
            sb.Append($"\"title\": \"Your request parameters did not validate.\",\n");
            sb.Append($"\"detail\": \"One or more invalid parameters were specified.\",\n");
            sb.Append($"\"status\": 400,");
            sb.Append($"\"instance\": \"{context.HttpContext.Request.Path}\",\n");
            sb.Append($"\"validationErrors\": [\n");
            
            // write and log details collection
            foreach (var state in modelStateEntries)
            {
                logger.LogWarning($"InvalidParameter|{context.HttpContext.Request.Path}|{state.Value.Errors[0].ErrorMessage}");

                sb.Append("{\n");
                sb.Append($"\"code\": \"InvalidValue\",\n");
                sb.Append($"\"target\": \"{state.Key}\",\n");
                sb.Append($"\"message\": \"{state.Value.Errors[0].ErrorMessage}\"\n");

                if (state.Equals(last))
                {
                    sb.Append("}\n");
                    sb.Append("]");
                }
                else
                {
                    // more results
                    sb.Append("},\n");
                }
            }

            sb.Append("}\n");

            return sb.ToString();
        }
        
        /// <summary>
        /// creates JSON response using string.Format given inputs
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static string WriteJsonWithStringFormat(ActionContext context, ILogger logger)
        {
            // collect all errors for iterative string/json representation
            var validationErrors = context.ModelState.Where(m => m.Value.Errors.Count > 0).ToArray();

            // need last error for proper json formatting of a collection
            var lastValidationError = validationErrors.Last();

            var validationErrorsJson = "";

            foreach (var validationError in validationErrors)
            {
                // log each validation error in the collection
                logger.LogWarning($"InvalidParameter|{context.HttpContext.Request.Path}|{validationError.Value.Errors[0].ErrorMessage}");

                // create a validationError
                validationErrorsJson += string.Format(
                    CultureInfo.InvariantCulture,
                    "{{`code`: `InvalidValue`, `target`: `{0}`, `message`: `{1}`}}", validationError.Key, validationError.Value.Errors[0].ErrorMessage).Replace("`", "\"", StringComparison.InvariantCulture);

                // implementation of proper json formatting of a collection
                if (!validationError.Equals(lastValidationError))
                    validationErrorsJson += ",";
            }

            // format final response including error collection
            var response = string.Format(
                CultureInfo.InvariantCulture,
                "{{`type`: `http://www.example.com/validation-error`, `title`: `Your request parameters did not validate.`, `detail`: `One or more invalid parameters were specified.`, `status`: {0}, `instance`: `{1}`, `validationErrors`: [{2}]}}",
                (int)HttpStatusCode.BadRequest, context.HttpContext.Request.Path, validationErrorsJson).Replace("`", "\"", StringComparison.InvariantCulture);

            return response;
        }

        public static string WriteJsonUsingObjects(ActionContext context, ILogger logger)
        {
            var problemDetails = new ValidationProblemDetails
            {
                Type = "http://www.example.com/validation-error",
                Title = "Your request parameters did not validate.",
                Detail = "One or more invalid parameters were specified.",
                Status = 400,
                Instance = context.HttpContext.Request.Path,
            };

            // collect all errors for iterative string/json representation
            var validationErrors = context.ModelState.Where(m => m.Value.Errors.Count > 0).ToArray();

            foreach (var validationError in validationErrors)
            {
                // log each validation error in the collection
                logger.LogWarning($"InvalidParameter|{context.HttpContext.Request.Path}|{validationError.Value.Errors[0].ErrorMessage}");

                var error = new ValidationError("InvalidValue", validationError.Key, validationError.Value.Errors[0].ErrorMessage);
                
                // add error object to problemDetails
                problemDetails.ValidationErrors.Add(error);
            }

            return JsonSerializer.Serialize(problemDetails);
        }
    }
}
