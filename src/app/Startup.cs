// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Helium.Validation;
using CSE.Middleware;
using Helium;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSE.Helium
{
    /// <summary>
    /// WebHostBuilder Startup
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">the configuration for WebHost</param>
        public Startup(IConfiguration configuration)
        {
            // keep a local reference
            Configuration = configuration;
        }

        /// <summary>
        /// Service configuration
        /// </summary>
        /// <param name="services">The services in the web host</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // set json serialization defaults and api behavior
            services.AddControllers()
                .ConfigureApiBehaviorOptions(o =>
                {
                    o.InvalidModelStateResponseFactory = ctx => new ValidationProblemDetailsResult();
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
                });

            // add healthcheck service
            services.AddHealthChecks().AddCosmosHealthCheck(CosmosHealthCheck.ServiceId);

            // add App Insights if key set
            string appInsightsKey = Configuration.GetValue<string>(Constants.AppInsightsKey);

            if (!string.IsNullOrEmpty(appInsightsKey))
            {
                services.AddApplicationInsightsTelemetry(appInsightsKey);
            }
        }

        /// <summary>
        /// Configure the application builder
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <param name="env">IWebHostEnvironment</param>
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));

            ServiceActivator.Configure(app.ApplicationServices);

            // log http responses to the console
            // this should be first as it "wraps" all requests
            if (App.AppLogLevel != LogLevel.None)
            {
                app.UseLogger(new LoggerOptions
                {
                    Log2xx = App.AppLogLevel <= LogLevel.Information,
                    Log3xx = App.AppLogLevel <= LogLevel.Information,
                    Log4xx = App.AppLogLevel <= LogLevel.Warning,
                    Log5xx = true,
                });
            }

            // differences based on dev or prod
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseResponseCaching();

            app.UseStaticFiles();

            // use routing
            app.UseRouting();

            // map the controllers
            app.UseEndpoints(ep => { ep.MapControllers(); });

            // rewrite root to /index.html
            app.UseSwaggerRoot();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(Constants.SwaggerPath, Constants.SwaggerTitle);
                c.RoutePrefix = string.Empty;
            });

            // use the robots middleware to handle /robots*.txt requests
            app.UseRobots();

            // use the version middleware to handle /version
            app.UseVersion();
        }
    }
}
