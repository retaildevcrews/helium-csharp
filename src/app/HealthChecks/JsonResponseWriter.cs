using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Text.Json;
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
        public static Task JsonResponseWriter(HttpContext httpContext, HealthReport healthReport)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            // write the json
            httpContext.Response.ContentType = "application/json";
            return httpContext.Response.WriteAsync(JsonSerializer.Serialize(healthReport, jsonOptions));
        }
    }
}
