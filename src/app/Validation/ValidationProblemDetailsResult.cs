using Helium;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CSE.Helium.Validation
{
    public class ValidationProblemDetailsResult : IActionResult
    {
        private readonly ILogger logger;

        public ValidationProblemDetailsResult()
        {
            using var serviceScope = ServiceActivator.GetScope();
            logger = serviceScope.ServiceProvider.GetService<ILogger<ValidationProblemDetailsResult>>();
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            context.HttpContext.Response.WriteAsync(ValidationProcessor.WriteJsonUsingObjects(context, logger));
            
            //context.HttpContext.Response.WriteAsync(ValidationProcessor.WriteJsonWithStringBuilder(context, logger));

            return Task.CompletedTask;
        }
    }
}
