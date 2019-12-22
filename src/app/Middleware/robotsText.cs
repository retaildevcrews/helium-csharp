using Microsoft.AspNetCore.Builder;

namespace Helium
{
    public static class RobotsMiddlewareExtensions
    {
        // response that prevents all indexing
        static readonly byte[] _responseBytes = System.Text.Encoding.UTF8.GetBytes("# Prevent indexing\r\nUser-agent: *\r\nDisallow: /\r\n");

        /// <summary>
        /// Middleware extension method to handle /robots*.txt request
        /// App Service sends a warmup request of /robots8327.txt (where 8327 is a random number)
        /// this causes a 404 error which appears in Azure Monitor as a false issue
        /// this also handles a real /robots.txt request to prevent indexing
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRobots(this IApplicationBuilder builder)
        {
            // create the middleware
            builder.Use(async (context, next) =>
            {
                // convert to lower and remove the leading /
                string path = context.Request.Path.Value.ToLower().Substring(1);

                // matches /robots*.txt (/robots.txt /robots123.txt etc)
                // does not match /robots/robots.txt
                if (!path.Contains("/") && path.StartsWith("robots") && path.EndsWith(".txt"))
                {
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
