using Helium.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
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
        public static Task IetfResponseWriter(HttpContext httpContext, HealthReport healthReport)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            Dictionary<string, object> checks = new Dictionary<string, object>();

            res.Add("status", ToIetfStatus(healthReport.Status));

            foreach (var e in healthReport.Entries.Values)
            {
                foreach (var d in e.Data)
                {
                    if (d.Value is HealthzCheck r)
                    {
                        IetfCheck ietf = new IetfCheck()
                        {
                            Status = ToIetfStatus(r.Status),
                            ComponentType = "CosmosDB",
                            ObservedValue = Math.Round(r.Duration.TotalMilliseconds, 2),
                            ObservedUnit = "ms",
                            Time = r.Time,
                            Uri = r.Uri,
                            Message = r.Message
                        };

                        checks.Add(d.Key, ietf);
                    }
                    else
                    {
                        res.Add(d.Key, d.Value);
                    }
                }
            }

            res.Add("checks", checks);

            // write the json
            httpContext.Response.ContentType = "application/health+json";
            return httpContext.Response.WriteAsync(JsonSerializer.Serialize(res, jsonOptions));
        }

        /// <summary>
        /// Convert the dotnet HealthStatus to the IETF Status
        /// </summary>
        /// <param name="status">HealthStatus (dotnet)</param>
        /// <returns>string</returns>
        public static string ToIetfStatus(HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Healthy => "up",
                HealthStatus.Degraded => "warn",
                _ => "down"
            };
        }
    }
}
