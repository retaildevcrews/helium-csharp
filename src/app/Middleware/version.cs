using Microsoft.AspNetCore.Builder;

namespace Helium
{
    public static class VersionMiddlewareExtensions
    {
        // response that returns the current version
        static byte[] _responseBytes = null;

        /// <summary>
        /// Middleware extension method to handle /version request
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <returns></returns>
        public static IApplicationBuilder UseVersion(this IApplicationBuilder builder)
        {
            // create the middleware
            builder.Use(async (context, next) =>
            {
                // matches /version
                if (context.Request.Path.Value.Equals("/version", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (_responseBytes == null)
                    {
                        _responseBytes = System.Text.Encoding.UTF8.GetBytes(Version.AssemblyVersion);
                    }

                    string t = Version.AssemblyVersion + System.Environment.NewLine;

                    foreach (var h in context.Request.Headers)
                    {
                        t += string.Format($"{h.Key}\t{context.Request.Headers[h.Key]}\n");
                    }

                    _responseBytes = System.Text.Encoding.UTF8.GetBytes(t);

                    // return the content
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
