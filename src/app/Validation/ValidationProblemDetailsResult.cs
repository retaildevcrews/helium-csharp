using System;
using System.Collections.Generic;
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

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            //context.HttpContext.Response.WriteAsync(ValidationProcessor.WriteJsonUsingObjects(context, logger));
            
            context.HttpContext.Response.WriteAsync(ValidationProcessor.WriteJsonWithStringFormat(context, logger));

            //context.HttpContext.Response.WriteAsync(ValidationProcessor.WriteJsonWithStringBuilder(context, logger));

            return Task.CompletedTask;
        }
    }
}
