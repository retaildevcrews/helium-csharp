using Helium.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Helium
{
    /// <summary>
    /// Logger options used to configure DI
    /// </summary>
    public class LoggerOptions
    {
        public bool Log2xx { get; set; } = true;
        public bool Log3xx { get; set; } = true;
        public bool Log4xx { get; set; } = true;
        public bool Log5xx { get; set; } = true;
        public double TargetMs { get; set; } = 250;
    }

    /// <summary>
    /// Middleware extension to make registering Logger easy
    /// 
    /// Note: Logger should be on of the first things registered in DI
    /// </summary>
    public static class LoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogger(this IApplicationBuilder builder, LoggerOptions options = null)
        {
            // extension - use app.UseMyLogger();

            if (options == null)
            {
                options = new LoggerOptions();
            }

            return builder.UseMiddleware<Logger>(Options.Create<LoggerOptions>(options));
        }
    }

    /// <summary>
    /// Simple aspnet core middleware that logs requests to the console
    /// </summary>
    public class Logger
    {

        // next action to Invoke
        private readonly RequestDelegate _next;
        private readonly LoggerOptions _options;

        private const string _ipHeader = "X-Client-IP";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">RequestDelegate</param>
        /// <param name="options">LoggerOptions</param>
        public Logger(RequestDelegate next, IOptions<LoggerOptions> options)
        {
            // save for later
            _next = next;
            _options = options?.Value;

            if (_options == null)
            {
                // use default
                _options = new LoggerOptions();
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

            // Invoke next handler
            if (_next != null)
            {
                await _next.Invoke(context).ConfigureAwait(false);
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
                Console.WriteLine($"{context.Response.StatusCode}\t{duration,6:0}\t{context.Request.Headers[_ipHeader]}\t{GetPathAndQuerystring(context.Request)}");
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
                if (!_options.Log2xx)
                {
                    return false;
                }
            }
            else if (response.StatusCode < 400)
            {
                if (!_options.Log3xx)
                {
                    return false;
                }
            }
            else if (response.StatusCode < 500)
            {
                if (!_options.Log4xx)
                {
                    return false;
                }
            }
            else
            {
                if (!_options.Log5xx)
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
                // TODO - log the exception if not null

                var hcr = (HealthCheckResult)context.Items[typeof(HealthCheckResult).ToString()];


                // log not healthy requests
                if (hcr.Status != HealthStatus.Healthy)
                {
                    string log = string.Empty;

                    // build the log message
                    log += string.Format($"{hcr.Status}\t{(long)duration,6}\t{context.Request.Headers[_ipHeader]}\t{GetPathAndQuerystring(context.Request)}\n", CultureInfo.InvariantCulture);


                    // add each not healthy check to the log message
                    foreach (var d in hcr.Data.Values)
                    {
                        if (d is HealthzCheck h && h.Status != HealthStatus.Healthy)
                        {
                            log += string.Format($"{h.Status}\t{(long)h.Duration.TotalMilliseconds,6}\t{context.Request.Headers[_ipHeader]}\t{h.Endpoint}\t({h.TargetDuration.TotalMilliseconds,1:0})\n", CultureInfo.InvariantCulture);
                        }
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
