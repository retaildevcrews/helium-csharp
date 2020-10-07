using Microsoft.AspNetCore.Builder;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace CSE.Middleware
{
    /// <summary>
    /// Registers aspnet middleware handler that handles /version
    /// </summary>
    public static class VersionExtensions
    {
        // cached response
        private static byte[] responseBytes;

        // cache version info as it doesn't change
        private static string version = string.Empty;

        /// <summary>
        /// Gets the app version
        /// </summary>
        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    if (Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute v)
                    {
                        version = v.InformationalVersion;
                    }
                }

                return version;
            }
        }

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
                    if (responseBytes == null)
                    {
                        string swaggerVersion;

                        try
                        {
                            // read swagger version from swagger.json
                            using JsonDocument sw = JsonDocument.Parse(File.ReadAllText("wwwroot/swagger/helium.json"));
                            swaggerVersion = sw.RootElement.GetProperty("info").GetProperty("version").ToString();
                        }
                        catch (Exception ex)
                        {
                            // log and default to 1.0
                            Console.WriteLine($"UseVersion:{ex.Message}");
                            swaggerVersion = "1.0";
                        }

                        // build and cache the json string
                        string json = $"{{ \"apiVersion\": \"{swaggerVersion}\", \"appVersion\": \"{Middleware.VersionExtensions.Version}\", \"language\": \"C#\" }}";
                        responseBytes = System.Text.Encoding.UTF8.GetBytes(json);
                    }

                    // return the version info
                    context.Response.ContentType = "application/json";
                    await context.Response.Body.WriteAsync(responseBytes).ConfigureAwait(false);
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
