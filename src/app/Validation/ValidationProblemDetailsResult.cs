// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Helium;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSE.Helium.Validation
{
    public class ValidationProblemDetailsResult : IActionResult
    {
        private readonly ILogger logger;

        public ValidationProblemDetailsResult()
        {
            using IServiceScope serviceScope = ServiceActivator.GetScope();
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
        /// <param name="context">ActionContext</param>
        /// <param name="logger">ILogger</param>
        private static string WriteJsonOutput(ActionContext context, ILogger logger)
        {
            // create problem details response
            ValidationProblemDetails problemDetails = new ValidationProblemDetails(
                type: FormatProblemType(context),
                title: "Parameter validation error",
                detail: "One or more invalid parameters were specified.",
                status: (int)HttpStatusCode.BadRequest,
                instance: context.HttpContext.Request.GetEncodedPathAndQuery());

            // collect all errors for iterative string/json representation
            System.Collections.Generic.KeyValuePair<string, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry>[] validationErrors = context.ModelState.Where(m => m.Value.Errors.Count > 0).ToArray();

            foreach (System.Collections.Generic.KeyValuePair<string, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry> validationError in validationErrors)
            {
                // skip empty validation error
                if (string.IsNullOrEmpty(validationError.Key))
                {
                    continue;
                }

                // log each validation error in the collection
                logger.LogWarning($"InvalidParameter|{context.HttpContext.Request.Path}|{validationError.Value.Errors[0].ErrorMessage}");

                // add error object to problemDetails
                problemDetails.ValidationErrors.Add(CreateValidationError(validationError.Key));
            }

            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };

            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.Converters.Add(new TimeSpanConverter());

            return JsonSerializer.Serialize(problemDetails, jsonOptions);
        }

        /// <summary>
        /// Create a standard validation error
        /// </summary>
        /// <param name="key">validation error key</param>
        /// <returns>standardized ValidationError</returns>
        private static ValidationError CreateValidationError(string key)
        {
            return new ValidationError("InvalidValue", PascalToCamelCase(key), GetErrorMessage(key));
        }

        /// <summary>
        /// Convert Pascal case to camel case
        ///   value must be a valid Pascal case word
        ///   no validation / changes are made other than lower case the first letter
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <returns>string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "tolower is correct")]
        private static string PascalToCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length == 1)
            {
                return value.ToLowerInvariant();
            }

            return value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
        }

        /// <summary>
        /// Create standard error messages
        /// </summary>
        /// <param name="key">validation error key</param>
        /// <returns>string</returns>
        private static string GetErrorMessage(string key)
        {
            return key switch
            {
                "ActorId" => "The parameter 'actorId' should start with 'nm' and be between 7 and 11 characters in total.",
                "Genre" => "The parameter 'genre' should be between 3 and 20 characters.",
                "MovieId" => "The parameter 'movieId' should start with 'tt' and be between 7 and 11 characters in total.",
                "PageNumber" => "The parameter 'pageNumber' should be between 1 and 10000.",
                "PageSize" => "The parameter 'pageSize' should be between 1 and 1000.",
                "Q" => "The parameter 'q' should be between 2 and 20 characters.",
                "Rating" => "The parameter 'rating' should be between 0.0 and 10.0.",
                "Year" => "The parameter 'year' should be between 1874 and 2025.",
                _ => $"Unknown key: {key}",
            };
        }

        /// <summary>
        /// Determines the correct Type property to set in JSON response
        /// </summary>
        /// <param name="context">ActionContext</param>
        private static string FormatProblemType(ActionContext context)
        {
            const string baseUri = "https://github.com/retaildevcrews/helium/blob/main/docs/ParameterValidation.md";

            string instance = context.HttpContext.Request.GetEncodedPathAndQuery();

            if (instance.Contains("?", StringComparison.InvariantCulture))
            {
                // query parameter
                if (instance.StartsWith("/api/movies", StringComparison.InvariantCulture))
                {
                    return baseUri + "#movies";
                }

                if (instance.StartsWith("/api/actors", StringComparison.InvariantCulture))
                {
                    return baseUri + "#actors";
                }
            }
            else
            {
                // direct read
                if (instance.StartsWith("/api/movies", StringComparison.InvariantCulture))
                {
                    return baseUri + "#direct-read";
                }

                if (instance.StartsWith("/api/actors", StringComparison.InvariantCulture))
                {
                    return baseUri + "#direct-read-1";
                }
            }

            // no match, return parameter validation main page
            return baseUri;
        }
    }
}
