using Microsoft.AspNetCore.Builder;
using System.Globalization;

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
                    // get assembly version
                    var aVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                    // get file creation date time
                    string file = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    System.DateTime dt = System.IO.File.GetCreationTime(file);

                    // build semver from assembly version and file creation date
                    _version = string.Format(CultureInfo.InvariantCulture, $"{aVer.Major}.{aVer.Minor}.{aVer.Build}+{dt.ToString("MMdd.HHmm", CultureInfo.InvariantCulture)}");
                }

                return _version;
            }
        }
    }
}
