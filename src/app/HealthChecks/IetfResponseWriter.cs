using Helium.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, object> checks = new Dictionary<string, object>();

            result.Add("status", IetfCheck.ToIetfStatus(healthReport.Status));
            result.Add("serviceId", "helium-csharp");
            result.Add("description", "Helium Health Check");

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
                        checks.Add(d.Key, new IetfCheck(r));
                    }
                    else
                    {
                        // add to the main dictionary
                        result.Add(d.Key, d.Value);
                    }
                }
            }

            // add the checks to the dictionary
            result.Add("checks", checks);

            // write the json
            httpContext.Response.ContentType = "application/health+json";
            return httpContext.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
        }

        public static Dictionary<string, object> IetfResponseWriter(HttpContext httpContext, HealthCheckResult res)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, object> checks = new Dictionary<string, object>();

            result.Add("status", IetfCheck.ToIetfStatus(res.Status));
            result.Add("serviceId", "helium-csharp");
            result.Add("description", "Helium Health Check");

            // add all the entries
            // add all the data elements
            foreach (var d in res.Data)
            {
                // transform HealthzCheck into IetfCheck
                if (d.Value is HealthzCheck r)
                {
                    // add to checks dictionary
                    checks.Add(d.Key, new IetfCheck(r));
                }
                else
                {
                    // add to the main dictionary
                    result.Add(d.Key, d.Value);
                }
            }

            // add the checks to the dictionary
            result.Add("checks", checks);

            // write the json
            httpContext.Response.ContentType = "application/health+json";

            return result;
        }
    }
}
