using Helium.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Helium
{
    public partial class CosmosHealthCheck : IHealthCheck
    {
        private readonly Stopwatch stopwatch = new Stopwatch();


        /// <summary>
        /// Build the response
        /// </summary>
        /// <param name="uri">string</param>
        /// <param name="targetDurationMs">double (ms)</param>
        /// <param name="ex">Exception (default = null)</param>
        /// <param name="data">Dictionary(string, object)</param>
        /// <returns>HealthzCheck</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
        private HealthzCheck BuildHealthzCheck(string uri, double targetDurationMs, Exception ex = null, Dictionary<string, object> data = null, string testName = null)
        {
            stopwatch.Stop();

            // create the result
            var result = new HealthzCheck
            {
                Endpoint = uri,
                Status = HealthStatus.Healthy,
                Duration = stopwatch.Elapsed,
                TargetDuration = new System.TimeSpan(0, 0, 0, 0, (int)targetDurationMs),
                ComponentType = "CosmosDB"
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
                data.Add(testName, result);
            }

            return result;
        }

        /// <summary>
        /// Get Genres Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetGenresAsync(Dictionary<string, object> data = null)
        {
            const int maxMilliseconds = 400;
            const string path = "/api/genres";

            stopwatch.Restart();

            try
            {
                (await _dal.GetGenresAsync().ConfigureAwait(false)).ToList<string>();

                return BuildHealthzCheck(path, maxMilliseconds, null, data, nameof(GetGenresAsync));
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, maxMilliseconds, ex, data, nameof(GetGenresAsync));

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
            const int maxMilliseconds = 400;
            string path = "/api/movies/" + movieId;

            stopwatch.Restart();

            try
            {
                await _dal.GetMovieAsync(movieId).ConfigureAwait(false);

                return BuildHealthzCheck(path, maxMilliseconds, null, data, nameof(GetMovieByIdAsync));
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, maxMilliseconds, ex, data, nameof(GetMovieByIdAsync));

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
            const int maxMilliseconds = 400;
            string path = "/api/movies?q=" + query;

            stopwatch.Restart();

            try
            {
                (await _dal.GetMoviesByQueryAsync(query).ConfigureAwait(false)).ToList<Movie>();

                return BuildHealthzCheck(path, maxMilliseconds, null, data, nameof(SearchMoviesAsync));
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, maxMilliseconds, ex, data, nameof(SearchMoviesAsync));

                // throw the exception so that HealthCheck logs
                throw;
            }
        }

        /// <summary>
        /// Get Top Rated Movies Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetTopRatedMoviesAsync(Dictionary<string, object> data = null)
        {
            const int maxMilliseconds = 400;
            const string path = "/api/movies?torated=true";

            stopwatch.Restart();

            try
            {
                (await _dal.GetMoviesByQueryAsync(string.Empty, toprated: true).ConfigureAwait(false)).ToList<Movie>();

                return BuildHealthzCheck(path, maxMilliseconds, null, data, nameof(GetTopRatedMoviesAsync));
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, maxMilliseconds, ex, data, nameof(GetTopRatedMoviesAsync));

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
            const int maxMilliseconds = 250;
            string path = "/api/actors/" + actorId;

            stopwatch.Restart();

            try
            {
                await _dal.GetActorAsync(actorId).ConfigureAwait(false);
                return BuildHealthzCheck(path, maxMilliseconds, null, data, nameof(GetActorByIdAsync));
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, maxMilliseconds, ex, data, nameof(GetActorByIdAsync));

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
            const int maxMilliseconds = 400;
            string path = "/api/actors?q=" + query;

            stopwatch.Restart();

            try
            {
                (await _dal.GetActorsByQueryAsync(query).ConfigureAwait(false)).ToList<Actor>();

                return BuildHealthzCheck(path, maxMilliseconds, null, data, nameof(SearchActorsAsync));
            }
            catch (Exception ex)
            {
                BuildHealthzCheck(path, maxMilliseconds, ex, data, nameof(SearchActorsAsync));

                // throw the exception so that HealthCheck logs
                throw;
            }
        }
    }
}
