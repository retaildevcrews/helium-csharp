using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Helium
{
    public partial class CosmosHealthCheck : IHealthCheck
    {
        public static readonly string Name = "cosmosHealthCheck";
        public static readonly string Description = "Cosmos DB Health Check";

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // dictionary
            var data = new Dictionary<string, object>();

            try
            {
                // add instance and version
                data.Add("Instance", System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID") ?? "unknown");
                data.Add("Version", Helium.Version.AssemblyVersion);


                // Run each health check
                data.Add("getGenresAsync", await GetGenresAsync().ConfigureAwait(false));
                data.Add("getActorByIdAsync", await GetActorByIdAsync("nm0000173").ConfigureAwait(false));
                data.Add("getMovieByIdAsync", await GetMovieByIdAsync("tt0133093").ConfigureAwait(false));
                data.Add("searchMoviesAsync", await SearchMoviesAsync("ring").ConfigureAwait(false));
                data.Add("searchActorsAsync", await SearchActorsAsync("nicole").ConfigureAwait(false));
                data.Add("getTopRatedMoviesAsync", await GetTopRatedMoviesAsync().ConfigureAwait(false));

                HealthStatus status = HealthStatus.Healthy;

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


// TODO - remove or uncomment

                // display any non-healthy checks
                //if (status != HealthStatus.Healthy)
                //{
                //    foreach (var d in data.Values)
                //    {
                //        if (d is HealthzCheck h && h.Status != HealthStatus.Healthy)
                //        {
                //            Console.WriteLine($"{h.Status}\t{(long)h.Duration.TotalMilliseconds,6:0}\tHealtz({(int)h.TargetDuration.TotalMilliseconds})\t{h.Endpoint}");
                //        }
                //    }
                //}

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
