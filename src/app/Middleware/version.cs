using Microsoft.AspNetCore.Builder;

namespace Helium
{
    /// <summary>
    /// Registers aspnet middleware handler that handles /version
    /// </summary>
    public static class VersionMiddlewareExtensions
    {
        // cached response
        static byte[] _responseBytes = null;

        /// <summary>
        /// Middleware extension method to handle /version request
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder UseVersion(this IApplicationBuilder builder)
        {
            // implement the middleware
            builder.Use(async (context, next) =>
            {
                // matches /version
                if (context.Request.Path.Value.Equals("/version", System.StringComparison.OrdinalIgnoreCase))
                {
                    // cache the version info for performance
                    if (_responseBytes == null)
                    {
                        _responseBytes = System.Text.Encoding.UTF8.GetBytes(Version.AssemblyVersion);
                    }

                    // return the version info
                    context.Response.ContentType = "text/plain";
                    await context.Response.Body.WriteAsync(_responseBytes, 0, _responseBytes.Length).ConfigureAwait(false);
                }
                else
                {
                    // not a match, so call next middleware handler
                    await next().ConfigureAwait(false);
                }
            });

            return builder;
        }
    }
}
