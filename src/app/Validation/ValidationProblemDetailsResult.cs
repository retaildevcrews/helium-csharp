using System;
using System.Net;
using System.Text.Json;
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

            var problemDetails = new ValidationProblemDetails(context.ModelState);

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));

            //context.HttpContext.Response.WriteAsync(ValidationProcessor.ProcessBadParameter(context, context.HttpContext.Request.Path, logger));

            return Task.CompletedTask;
        }
    }
}
