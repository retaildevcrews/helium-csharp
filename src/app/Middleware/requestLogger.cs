using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
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
        private readonly LoggerOptions _options = new LoggerOptions();

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
            long duration = (long)DateTime.Now.Subtract(dtStart).TotalMilliseconds;

            // log degraded requests if targetMs > 0
            if (_options.TargetMs > 0 && duration > _options.TargetMs)
            {
                // write the degraded message to the console

                // TODO - temporarily disable this for debugging healthz degraded correlation
                //Console.WriteLine($"Degraded\t{duration,6:0}\t{context.Request.Headers[_ipHeader]}\t{GetPathAndQuerystring(context.Request)}");
            }

            // TODO - remove this block - debugging healthz correlation
            if (duration > 250 && context.Request.Path.StartsWithSegments("/healthz", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"{context.Response.StatusCode}\t{duration,6:0}\t{context.Request.Headers[_ipHeader]}\t{GetPathAndQuerystring(context.Request)}");
                return;
            }


            // don't log favicon.ico 404s
            if (context.Request.Path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // check for logging by response level
            if (context.Response.StatusCode < 300)
            {
                if (!_options.Log2xx)
                {
                    return;
                }
            }
            else if (context.Response.StatusCode < 400)
            {
                if (!_options.Log3xx)
                {
                    return;
                }
            }
            else if (context.Response.StatusCode < 500)
            {
                if (!_options.Log4xx)
                {
                    return;
                }
            }
            else
            {
                if (!_options.Log5xx)
                {
                    return;
                }
            }

            // write the results to the console
            Console.WriteLine($"{context.Response.StatusCode}\t{duration,6:0}\t{context.Request.Headers[_ipHeader]}\t{GetPathAndQuerystring(context.Request)}");
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
