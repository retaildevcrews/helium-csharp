using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helium
{
    public partial class CosmosHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Write the health check results as json
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="healthReport">HealthReport</param>
        /// <returns></returns>
        public static Task CustomResponseWriter(HttpContext httpContext, HealthReport healthReport)
        {
            httpContext.Response.ContentType = "application/json";

            // setup serialization options
            if (jsonOptions == null)
            {
                // ignore nulls in json
                jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                };

                // serialize enums as strings
                jsonOptions.Converters.Add(new JsonStringEnumConverter());

                // serialize TimeSpan as 00:00:00.1234567
                jsonOptions.Converters.Add(new TimeSpanConverter());
            }

            // write the json
            return httpContext.Response.WriteAsync(JsonSerializer.Serialize(healthReport, jsonOptions));
        }
    }
}
