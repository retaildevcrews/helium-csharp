using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSE.Helium.Validation
{
    public class ValidationProblemDetailsResult : IActionResult
    {
        private readonly ILogger logger;

        public ValidationProblemDetailsResult(IServiceCollection serviceCollection)
        {
            var services = serviceCollection.BuildServiceProvider();
            logger = services.GetRequiredService<ILogger<ValidationProblemDetailsResult>>();
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.WriteAsync(
                ValidationProcessor.ProcessBadParameter(context, "Testing", logger));

            return Task.CompletedTask;
        }
    }
}
