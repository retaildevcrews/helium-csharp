using CSE.Helium.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace CSE.Middleware
{
    /// <summary>
    /// Simple aspnet core middleware that logs requests to the console
    /// </summary>
    public class Logger
    {

        // next action to Invoke
        private readonly RequestDelegate next;
        private readonly LoggerOptions options;

        private const string ipHeader = "X-Client-IP";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">RequestDelegate</param>
        /// <param name="options">LoggerOptions</param>
        public Logger(RequestDelegate next, IOptions<LoggerOptions> options)
        {
            // save for later
            this.next = next;
            this.options = options?.Value;

            if (this.options == null)
            {
                // use default
                this.options = new LoggerOptions();
            }
        }

        /// <summary>
        /// Called by aspnet pipeline
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>Task (void)</returns>
        public async Task Invoke(HttpContext context)
        {
            // set start time
            DateTime dtStart = DateTime.Now;

            if (context == null)
            {
                return;
            }

            // Invoke next handler
            if (next != null)
            {
                await next.Invoke(context).ConfigureAwait(false);
            }

            // compute request duration
            var duration = DateTime.Now.Subtract(dtStart).TotalMilliseconds;

            // don't log favicon.ico 404s
            if (context.Request.Path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // handle healthz composite logging
            if (LogHealthzHandled(context, duration))
            {
                return;
            }

            // write the results to the console
            if (ShouldLogRequest(context.Response))
            {
                Console.WriteLine($"{context.Response.StatusCode}\t{duration,6:0}\t{context.Request.Headers[ipHeader]}\t{GetPathAndQuerystring(context.Request)}");
            }
        }

        /// <summary>
        /// Check log level to determine if request should be logged
        /// </summary>
        /// <param name="response">HttpResponse</param>
        /// <returns></returns>
        private bool ShouldLogRequest(HttpResponse response)
        {

            // check for logging by response level
            if (response.StatusCode < 300)
            {
                if (!options.Log2xx)
                {
                    return false;
                }
            }
            else if (response.StatusCode < 400)
            {
                if (!options.Log3xx)
                {
                    return false;
                }
            }
            else if (response.StatusCode < 500)
            {
                if (!options.Log4xx)
                {
                    return false;
                }
            }
            else
            {
                if (!options.Log5xx)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Log the healthz results for degraded and unhealthy
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="duration">double</param>
        /// <returns></returns>
        private static bool LogHealthzHandled(HttpContext context, double duration)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // check if there is a HealthCheckResult item
            if (context.Items.Count > 0 && context.Items.ContainsKey(typeof(HealthCheckResult).ToString()))
            {
                var hcr = (HealthCheckResult)context.Items[typeof(HealthCheckResult).ToString()];


                // log not healthy requests
                if (hcr.Status != HealthStatus.Healthy)
                {
                    string log = string.Empty;

                    // build the log message
                    log += string.Format(CultureInfo.InvariantCulture, $"{IetfCheck.ToIetfStatus(hcr.Status)}\t{duration,6:0}\t{context.Request.Headers[ipHeader]}\t{GetPathAndQuerystring(context.Request)}\n");


                    // add each not healthy check to the log message
                    foreach (var d in hcr.Data.Values)
                    {
                        if (d is HealthzCheck h && h.Status != HealthStatus.Healthy)
                        {
                            log += string.Format(CultureInfo.InvariantCulture, $"{IetfCheck.ToIetfStatus(h.Status)}\t{(long)h.Duration.TotalMilliseconds,6}\t{context.Request.Headers[ipHeader]}\t{h.Endpoint}\t({h.TargetDuration.TotalMilliseconds,1:0})\n");
                        }
                    }

                    if (hcr.Exception != null)
                    {
                        log += "HealthCheckException\n";
                        log += hcr.Exception.ToString() + "\n";
                    }

                    // write the log message
                    Console.Write(log);

                    // done logging this request
                    return true;
                }
            }

            // keep processing
            return false;
        }

        /// <summary>
        /// Return the path and query string if it exists
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>string</returns>
        private static string GetPathAndQuerystring(HttpRequest request)
        {
            return request?.Path.ToString() + request?.QueryString.ToString();
        }
    }
}
