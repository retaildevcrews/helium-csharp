using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace Helium
{
    public partial class CosmosHealthCheck : IHealthCheck
    {
        public static readonly string Description = "Cosmos DB Health Check";

        private static JsonSerializerOptions jsonOptions = null;

        // TODO - /healthz doesn't appear in swagger

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
                if (App.config != null)
                {
                    // get the current Cosmos key
                    data.Add("CosmosKey", App.config.GetValue<string>(Constants.CosmosKey).PadRight(5).Substring(0, 5).Trim() + "...");
                }

                // add instance and version
                data.Add("Instance", System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID") ?? "unknown");
                data.Add("Version", Helium.Version.AssemblyVersion);

                // Run each individual health check
                data.Add("GetGenresAsync", await GetGenresAsync());
                data.Add("GetActorByIdAsync", await GetActorByIdAsync("nm0000173"));
                data.Add("GetMovieByIdAsync", await GetMovieByIdAsync("tt0133093"));
                data.Add("SearchMoviesAsync", await SearchMoviesAsync("ring"));
                data.Add("SearchActorsAsync", await SearchActorsAsync("nicole"));
                data.Add("GetTopRatedMoviesAsync", await GetTopRatedMoviesAsync());

                HealthStatus status = HealthStatus.Healthy;

                // overall health is the worst status
                foreach (var d in data.Values)
                {
                    if (status != HealthStatus.Unhealthy)
                    {
                        if (d is HealthzResult h && h.Status != HealthStatus.Healthy)
                        {
                            status = h.Status;
                        }
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

            catch (System.AggregateException age)
            {
                var root = age.GetBaseException() ?? age;

                data.Add("AggregateException", root.Message);

                // log and return unhealthy
                _logger.LogError($"AggregateException|Healthz|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, root, data);
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
