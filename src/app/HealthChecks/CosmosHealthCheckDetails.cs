using CSE.Helium.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CSE.Helium
{
    public partial class CosmosHealthCheck : IHealthCheck
    {
        private const int MaxResponseTime = 200;
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// Build the response
        /// </summary>
        /// <param name="uri">string</param>
        /// <param name="targetDurationMs">double (ms)</param>
        /// <param name="ex">Exception (default = null)</param>
        /// <param name="data">Dictionary(string, object)</param>
        /// <param name="testName">Test Name</param>
        /// <returns>HealthzCheck</returns>
        private HealthzCheck BuildHealthzCheck(string uri, double targetDurationMs, Exception ex = null, Dictionary<string, object> data = null, string testName = null)
        {
            stopwatch.Stop();

            // create the result
            HealthzCheck result = new HealthzCheck
            {
                Endpoint = uri,
                Status = HealthStatus.Healthy,
                Duration = stopwatch.Elapsed,
                TargetDuration = new System.TimeSpan(0, 0, 0, 0, (int)targetDurationMs),
                ComponentId = testName,
                ComponentType = "datastore",
            };

            // check duration
            if (result.Duration.TotalMilliseconds > targetDurationMs)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = HealthzCheck.TimeoutMessage;
            }

            // add the exception
            if (ex != null)
            {
                result.Status = HealthStatus.Unhealthy;
                result.Message = ex.Message;
            }

            // add the results to the dictionary
            if (data != null && !string.IsNullOrEmpty(testName))
            {
                data.Add(testName + ":responseTime", result);
            }

            return result;
        }

        /// <summary>
        /// Get Genres Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetGenresAsync(Dictionary<string, object> data = null)
        {
            const string name = "getGenres";
            const string path = "/api/genres";

            stopwatch.Restart();

            try
            {
                _ = (await dal.GetGenresAsync().ConfigureAwait(false)).ToList<string>();

                return BuildHealthzCheck(path, MaxResponseTime, null, data, name);
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, MaxResponseTime, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }

        /// <summary>
        /// Get Movie by Id Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetMovieByIdAsync(string movieId, Dictionary<string, object> data = null)
        {
            const string name = "getMovieById";
            string path = "/api/movies/" + movieId;

            stopwatch.Restart();

            try
            {
                await dal.GetMovieAsync(movieId).ConfigureAwait(false);

                return BuildHealthzCheck(path, MaxResponseTime / 2, null, data, name);
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, MaxResponseTime / 2, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }

        /// <summary>
        /// Search Movies Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> SearchMoviesAsync(string query, Dictionary<string, object> data = null)
        {
            const string name = "searchMovies";

            MovieQueryParameters movieQuery = new MovieQueryParameters { Q = query };

            string path = "/api/movies?q=" + movieQuery.Q;

            stopwatch.Restart();

            try
            {
                _ = (await dal.GetMoviesAsync(movieQuery).ConfigureAwait(false)).ToList<Movie>();

                return BuildHealthzCheck(path, MaxResponseTime, null, data, name);
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, MaxResponseTime, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }

        /// <summary>
        /// Get Actor By Id Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetActorByIdAsync(string actorId, Dictionary<string, object> data = null)
        {
            const string name = "getActorById";
            string path = "/api/actors/" + actorId;

            stopwatch.Restart();

            try
            {
                await dal.GetActorAsync(actorId).ConfigureAwait(false);
                return BuildHealthzCheck(path, MaxResponseTime / 2, null, data, name);
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, MaxResponseTime / 2, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }

        /// <summary>
        /// Search Actors Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> SearchActorsAsync(string query, Dictionary<string, object> data = null)
        {
            const string name = "searchActors";

            ActorQueryParameters actorQuery = new ActorQueryParameters { Q = query };

            string path = "/api/actors?q=" + actorQuery.Q;

            stopwatch.Restart();

            try
            {
                _ = (await dal.GetActorsAsync(actorQuery).ConfigureAwait(false)).ToList<Actor>();

                return BuildHealthzCheck(path, MaxResponseTime, null, data, name);
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, MaxResponseTime, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }
    }
}
