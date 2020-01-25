using Helium.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
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
        /// <returns>Task</returns>
        public static Task IetfResponseWriter(HttpContext httpContext, HealthReport healthReport)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (healthReport == null)
            {
                throw new ArgumentNullException(nameof(healthReport));
            }

            // create the dictionaries
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, object> checks = new Dictionary<string, object>();

            // add header values
            result.Add("status", IetfCheck.ToIetfStatus(healthReport.Status));
            result.Add("serviceId", CosmosHealthCheck.ServiceId);
            result.Add("description", CosmosHealthCheck.Description);

            // add all the entries
            foreach (var e in healthReport.Entries.Values)
            {
                // add all the data elements
                foreach (var d in e.Data)
                {
                    // transform HealthzCheck into IetfCheck
                    if (d.Value is HealthzCheck r)
                    {
                        // add to checks dictionary
                        checks.Add(ToCamelCase(d.Key), new IetfCheck(r));
                    }
                    else
                    {
                        // add to the main dictionary
                        result.Add(ToCamelCase(d.Key), d.Value);
                    }
                }
            }

            // add the checks to the dictionary
            result.Add("checks", checks);

            // write the json
            httpContext.Response.ContentType = "application/health+json";
            httpContext.Response.StatusCode = healthReport.Status == HealthStatus.Unhealthy ? (int)System.Net.HttpStatusCode.ServiceUnavailable : (int)System.Net.HttpStatusCode.OK;
            return httpContext.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
        }

        /// <summary>
        /// Write the Health Check results as json
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="res">HealthCheckResult</param>
        /// <param name="totalTime">TimeSpan</param>
        /// <returns></returns>
        public static Task IetfResponseWriter(HttpContext httpContext, HealthCheckResult res, TimeSpan totalTime)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // Convert the HealthCheckResult to a HealthReport
            HealthReport rpt = new HealthReport(
                new Dictionary<string, HealthReportEntry> { { CosmosHealthCheck.ServiceId, new HealthReportEntry(res.Status, res.Description, totalTime, res.Exception, res.Data) } },
                totalTime);

            // call the response writer
            return IetfResponseWriter(httpContext, rpt);

        }

        /// <summary>
        /// Convert string to camelCase
        /// </summary>
        /// <param name="src">string to convert</param>
        /// <returns>string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "to lower is correct")]
        public static string ToCamelCase(string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                return src;
            }

            string camel = src.Substring(0, 1).ToLowerInvariant();

            if (src.Length > 1)
            {
                camel += src.Substring(1);
            }

            return camel;
        }
    }
}
