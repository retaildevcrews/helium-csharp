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
        public static readonly string Description = "Cosmos DB Health Check";
        public static readonly string Key = "HealthzStatus";

        // TODO - /healthz doesn't appear in swagger
        private readonly ILogger _logger;
        private readonly IDAL _dal;

        // ignore nulls in json
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

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
            // status object
            HealthzStatus stat = new HealthzStatus();

            // dictionary
            var data = new Dictionary<string, object>()
            {
                { Key, stat }
            };

            try
            {
                if (App.config != null)
                {
                    // get the current Cosmos key
                    stat.CosmosKey = App.config.GetValue<string>(Constants.CosmosKey).PadRight(5).Substring(0, 5).Trim() + "...";
                }

                // Run each individual health check
                stat.Results.Add(await GetGenresAsync());
                stat.Results.Add(await GetActorByIdAsync("nm0000173"));
                stat.Results.Add(await GetMovieByIdAsync("tt0133093"));
                stat.Results.Add(await SearchMoviesAsync("ring"));
                stat.Results.Add(await SearchActorsAsync("nicole"));
                stat.Results.Add(await GetTopRatedMoviesAsync());

                // TODO - return stat.status - need to convert to enum first
                return new HealthCheckResult(HealthStatus.Healthy, Description, data: data);
            }

            catch (CosmosException ce)
            {
                // log and return Unhealthy
                _logger.LogError($"CosmosException:Healthz:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                stat.Message = ce.Message;

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, ce, data);
            }

            catch (System.AggregateException age)
            {
                var root = age.GetBaseException() ?? age;

                stat.Message = root.Message;

                // log and return unhealthy
                _logger.LogError($"AggregateException|Healthz|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, root, data);
            }

            catch (Exception ex)
            {
                // log and return unhealthy
                _logger.LogError($"Exception:Healthz\n{ex}");

                stat.Message = ex.Message;

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, ex, data);
            }
        }

        /// <summary>
        /// Run the Get Genres Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
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

        /// <summary>
        /// Run the Get Movie by Id Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
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

        /// <summary>
        /// Run the Search Movies Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
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

        /// <summary>
        /// Run the Get Top Rated Movies Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
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

        /// <summary>
        /// Run the Get Actor By Id Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
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

        /// <summary>
        /// Run the Search Actors Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
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

        /// <summary>
        /// Write the health check results as json
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="healthReport">HealthReport</param>
        /// <returns></returns>
        public static Task CustomResponseWriter(HttpContext httpContext, HealthReport healthReport)
        {
            httpContext.Response.ContentType = "application/json";

            // TODO - convert to use system.json with camel casing
            // write the json
            return httpContext.Response.WriteAsync(
                JsonConvert.SerializeObject(
                    healthReport?.Entries?[Constants.CosmosHealthCheck].Data?[Key], 
                    jsonSettings));
        }
    }
}
