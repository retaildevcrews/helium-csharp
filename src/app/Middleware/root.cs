using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Cosmos.Linq;
using System;

namespace Middleware
{
    /// <summary>
    /// Register swagger root middleware
    /// </summary>
    public static class SwaggerRootExtensions
    {
        /// <summary>
        /// aspnet middleware extension method to handle / request
        /// rewrite / to /index.html (the swagger UI)
        ///
        /// This has to be before UseSwaggerUI() in startup
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwaggerRoot(this IApplicationBuilder builder)
        {
            // implement the middleware
            builder.Use(async (context, next) =>
            {
                // rewrite / path
                if (context.Request.Path.Value == "/")
                {
                    context.Request.Path = new Microsoft.AspNetCore.Http.PathString("/index.html");
                }

                // call next middleware handler
                await next().ConfigureAwait(false);
            });

            return builder;
        }
    }
}
