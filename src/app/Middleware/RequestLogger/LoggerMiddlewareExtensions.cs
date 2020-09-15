using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace CSE.Middleware
{
    /// <summary>
    /// Middleware extension to make registering Logger easy
    /// 
    /// Note: Logger should be one of the first things registered in DI
    /// </summary>
    public static class LoggerExtensions
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
}
