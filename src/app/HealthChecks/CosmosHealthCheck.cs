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
using Newtonsoft.Json;

namespace Helium
{
    public class CosmosHealthCheck : IHealthCheck
    {
        private readonly ILogger _logger;
        private readonly IDAL _dal;

        public CosmosHealthCheck(ILogger<CosmosHealthCheck> logger, IDAL dal)
        {
            _logger = logger;
            _dal = dal;
        }

        public string Name => "CosmosHealthCheck";
        public string Description => "Cosmos DB Health Check";

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            HealthzStatus stat = new HealthzStatus();

            var data = new Dictionary<string, object>()
            {
                { "HealthzStatus", stat }
            };

            try
            {
                if (App.config != null)
                {
                    stat.CosmosKey = App.config.GetValue<string>(Constants.CosmosKey).PadRight(5).Substring(0, 5).Trim() + "...";
                }

                stat.Results.Add(await GetGenresAsync());
                stat.Results.Add(await GetActorByIdAsync("nm0000173"));
                stat.Results.Add(await GetMovieByIdAsync("tt0133093"));
                stat.Results.Add(await SearchMoviesAsync("ring"));
                stat.Results.Add(await SearchActorsAsync("nicole"));
                stat.Results.Add(await GetTopRatedMoviesAsync());

                return new HealthCheckResult(HealthStatus.Healthy, Description, data: data);
            }

            catch (CosmosException ce)
            {
                // log and return 503
                _logger.LogError($"CosmosException:Healthz:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                stat.Message = ce.Message;

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, ce, data);
            }

            catch (System.AggregateException age)
            {
                var root = age.GetBaseException();

                if (root == null)
                {
                    root = age;
                }

                stat.Message = root.Message;

                // log and return 500
                _logger.LogError($"AggregateException|Healthz|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, age, data);
            }

            catch (Exception ex)
            {
                // log and return 500
                _logger.LogError($"Exception:Healthz\n{ex}");

                stat.Message = ex.Message;

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, ex, data);
            }
        }

        private async Task<HealthzResult> GetGenresAsync()
        {
            var result = new HealthzResult();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetGenresAsync()).ToList<string>();
            sw.Stop();
            result.Uri = "/api/genres";
            result.StatusCode = HealthzStatusCode.Healthy;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 200)
            {
                result.StatusCode = HealthzStatusCode.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        private async Task<HealthzResult> GetMovieByIdAsync(string movieId)
        {
            var result = new HealthzResult();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            await _dal.GetMovieAsync(movieId);
            sw.Stop();
            result.Uri = string.Format($"/api/movies/{movieId}");
            result.StatusCode = HealthzStatusCode.Healthy;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        private async Task<HealthzResult> SearchMoviesAsync(string query)
        {
            var result = new HealthzResult();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetMoviesByQueryAsync(query)).ToList<Movie>();
            sw.Stop();
            result.Uri = string.Format($"/api/movies?q={query}");
            result.StatusCode = HealthzStatusCode.Healthy;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        private async Task<HealthzResult> GetTopRatedMoviesAsync()
        {
            var result = new HealthzResult();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetMoviesByQueryAsync(string.Empty, toprated: true)).ToList<Movie>();
            sw.Stop();
            result.Uri = string.Format($"/api/movies?toprated=true");
            result.StatusCode = HealthzStatusCode.Healthy;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        private async Task<HealthzResult> GetActorByIdAsync(string actorId)
        {
            var result = new HealthzResult();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            await _dal.GetActorAsync(actorId);
            sw.Stop();
            result.Uri = string.Format($"/api/actors/{actorId}");
            result.StatusCode = HealthzStatusCode.Healthy;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        private async Task<HealthzResult> SearchActorsAsync(string query)
        {
            var result = new HealthzResult();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetActorsByQueryAsync(query)).ToList<Actor>();
            sw.Stop();
            result.Uri = string.Format($"/api/actors?q={query}");
            result.StatusCode = HealthzStatusCode.Healthy;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        public static Task CustomResponseWriter(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var hz = result.Entries["CosmosDB"].Data["HealthzStatus"] as Helium.Model.HealthzStatus;

            JsonSerializerSettings ser = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(hz, Formatting.Indented, ser);

            return httpContext.Response.WriteAsync(json);
        }
    }

    public static class CosmosHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddCosmosHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null)
        {
            // Register a check of type Cosmos
            builder.AddCheck<CosmosHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }
    }
}
