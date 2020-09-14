using Helium;
using CSE.Helium.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;

namespace CSE.Helium.Validation
{
    public class ValidationProblemDetailsResult : IActionResult
    {
        private readonly ILogger logger;

        public ValidationProblemDetailsResult()
        {
            // todo: can this be done a different way so we don't need the ServiceActivator to get an instance of ILogger?
            using var serviceScope = ServiceActivator.GetScope();
            logger = serviceScope.ServiceProvider.GetService<ILogger<ValidationProblemDetailsResult>>();
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            context.HttpContext.Response.ContentType = "application/problem+json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.HttpContext.Response.WriteAsync(WriteJsonOutput(context, logger));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates JSON response using ValidationProblemDetails given inputs
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        private static string WriteJsonOutput(ActionContext context, ILogger logger)
        {
            // create problem details response
            var problemDetails = new ValidationProblemDetails(
                type: FormatProblemType(context),
                title: "Parameter validation error",
                detail: "One or more invalid parameters were specified.",
                status: (int)HttpStatusCode.BadRequest,
                instance: context.HttpContext.Request.GetEncodedPathAndQuery()
            );

            // collect all errors for iterative string/json representation
            var validationErrors = context.ModelState.Where(m => m.Value.Errors.Count > 0).ToArray();

            foreach (var validationError in validationErrors)
            {
                // skip empty validation error
                if (string.IsNullOrEmpty(validationError.Key))
                    continue;

                // log each validation error in the collection
                logger.LogWarning($"InvalidParameter|{context.HttpContext.Request.Path}|{validationError.Value.Errors[0].ErrorMessage}");

                var error = new ValidationError("InvalidValue", validationError.Key, validationError.Value.Errors[0].ErrorMessage);

                // add error object to problemDetails
                problemDetails.ValidationErrors.Add(error);
            }

            var jsonOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            };

            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.Converters.Add(new TimeSpanConverter());

            return JsonSerializer.Serialize(problemDetails, jsonOptions);
        }

        /// <summary>
        /// Determines the correct Type property to set in JSON response
        /// </summary>
        /// <param name="context"></param>
        private static string FormatProblemType(ActionContext context)
        {
            const string baseUri = "https://github.com/retaildevcrews/helium/blob/main/docs/ParameterValidation.md";

            var instance = context.HttpContext.Request.GetEncodedPathAndQuery();

            if (instance.Contains("?", StringComparison.InvariantCulture))
            {
                // query parameter
                if (instance.StartsWith("/api/movies", StringComparison.InvariantCulture))
                    return baseUri + "#movies";

                if (instance.StartsWith("/api/actors", StringComparison.InvariantCulture))
                    return baseUri + "#actors";
            }
            else
            {
                // direct read
                if (instance.StartsWith("/api/movies", StringComparison.InvariantCulture))
                    return baseUri + "#direct-read";

                if (instance.StartsWith("/api/actors", StringComparison.InvariantCulture))
                    return baseUri + "#direct-read-1";
            }

            // no match, return parameter validation main page
            return baseUri;
        }

    }
}
