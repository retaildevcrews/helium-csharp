using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Helium
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
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
            });

            // add healthcheck service
            services.AddHealthChecks().AddCosmosHealthCheck(CosmosHealthCheck.ServiceId);

            // configure Swagger
            services.ConfigureSwaggerGen(options =>
            {
                options.IncludeXmlComments(GetXmlCommentsPath());
            });

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(Constants.SwaggerName, new OpenApiInfo { Title = Constants.SwaggerTitle, Version = Constants.SwaggerVersion });
            });

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
        /// <param name="app"></param>
        /// <param name="env"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Required for DI")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // log 4xx and 5xx results to console
            // this should be first as it "wraps" all requests
            app.UseLogger(new LoggerOptions { TargetMs = 800, Log2xx = false, Log3xx = false });

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

            // use routing
            app.UseRouting();

            // map the controllers
            app.UseEndpoints(ep => { ep.MapControllers(); });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

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

        /// <summary>
        /// Get the path to the XML docs generated at compile time
        /// 
        /// Swagger uses this to annotate the documentation
        /// </summary>
        /// <returns>string - path to xml file</returns>
        private static string GetXmlCommentsPath()
        {
            var app = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application;
            return System.IO.Path.Combine(app.ApplicationBasePath, Constants.XmlCommentsPath);
        }
    }
}
