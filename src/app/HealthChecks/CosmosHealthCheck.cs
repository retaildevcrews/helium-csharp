using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Helium
{
    public partial class CosmosHealthCheck : IHealthCheck
    {
        public static readonly string ServiceId = "helium-csharp";
        public static readonly string Description = "Helium C# Health Check";

        private static JsonSerializerOptions jsonOptions = null;

        private readonly ILogger _logger;
        private readonly IDAL _dal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="dal">IDAL</param>
        public CosmosHealthCheck(ILogger<CosmosHealthCheck> logger, IDAL dal)
        {
            // save to member vars
            _logger = logger;
            _dal = dal;

            // setup serialization options
            if (jsonOptions == null)
            {
                // ignore nulls in json
                jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                };

                // serialize enums as strings
                jsonOptions.Converters.Add(new JsonStringEnumConverter());

                // serialize TimeSpan as 00:00:00.1234567
                jsonOptions.Converters.Add(new TimeSpanConverter());
            }
        }

        /// <summary>
        /// Run the health check (IHealthCheck)
        /// </summary>
        /// <param name="context">HealthCheckContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // dictionary
            var data = new Dictionary<string, object>();

            try
            {
                HealthStatus status = HealthStatus.Healthy;

                // add instance and version
                data.Add("Instance", System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID") ?? "unknown");
                data.Add("Version", Helium.Version.AssemblyVersion);

                // Run each health check
                await GetGenresAsync(data).ConfigureAwait(false);
                await GetActorByIdAsync("nm0000173", data).ConfigureAwait(false);
                await GetMovieByIdAsync("tt0133093", data).ConfigureAwait(false);
                await SearchMoviesAsync("ring", data).ConfigureAwait(false);
                await SearchActorsAsync("nicole", data).ConfigureAwait(false);

                // overall health is the worst status
                foreach (var d in data.Values)
                {
                    if (d is HealthzCheck h && h.Status != HealthStatus.Healthy)
                    {
                        status = h.Status;
                    }

                    if (status == HealthStatus.Unhealthy)
                    {
                        break;
                    }
                }

                // return the result
                return new HealthCheckResult(status, Description, data: data);
            }

            catch (CosmosException ce)
            {
                // log and return Unhealthy
                _logger.LogError($"CosmosException:Healthz:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                data.Add("CosmosException", ce.Message);

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, ce, data);
            }

            catch (Exception ex)
            {
                // log and return unhealthy
                _logger.LogError($"Exception:Healthz\n{ex}");

                data.Add("Exception", ex.Message);

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, ex, data);
            }
        }
    }
}
