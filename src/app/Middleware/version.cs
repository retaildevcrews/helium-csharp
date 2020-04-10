using Microsoft.AspNetCore.Builder;
using System;
using System.Reflection;

namespace Middleware
{
    /// <summary>
    /// Registers aspnet middleware handler that handles /version
    /// </summary>
    public static class VersionExtensions
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
                        _responseBytes = System.Text.Encoding.UTF8.GetBytes(Middleware.VersionExtensions.Version);
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

        // cache version info as it doesn't change
        static string _version = string.Empty;

        /// <summary>
        /// Get the app version
        /// </summary>
        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                {
                    if (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute v)
                    {
                        _version = v.InformationalVersion;
                    }
                }

                return _version;
            }
        }
    }
}
