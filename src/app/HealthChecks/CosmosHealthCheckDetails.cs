using Helium.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
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
        /// <param name="duration">TimeSpan</param>
        /// <param name="targetDurationMs">double (ms)</param>
        /// <returns>HealthzCheck</returns>
        private HealthzCheck BuildHealthzCheck(string uri, TimeSpan duration, double targetDurationMs)
        {
            // create the result
            var result = new HealthzCheck
            {
                Uri = uri,
                Status = HealthStatus.Healthy,
                Duration = duration
            };

            // check duration
            if (duration.TotalMilliseconds > targetDurationMs)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        /// <summary>
        /// Get Genres Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetGenresAsync()
        {
            stopwatch.Restart();
            (await _dal.GetGenresAsync()).ToList<string>();
            stopwatch.Stop();

            return BuildHealthzCheck("/api/genres", stopwatch.Elapsed, 200);
        }

        /// <summary>
        /// Get Movie by Id Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetMovieByIdAsync(string movieId)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            await _dal.GetMovieAsync(movieId);
            sw.Stop();

            return BuildHealthzCheck($"/api/movies/{movieId}", sw.Elapsed, 200);
        }

        /// <summary>
        /// Search Movies Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> SearchMoviesAsync(string query)
        {
            stopwatch.Restart();
            (await _dal.GetMoviesByQueryAsync(query)).ToList<Movie>();
            stopwatch.Stop();

            return BuildHealthzCheck($"/api/movies?q={query}", stopwatch.Elapsed, 200);
        }

        /// <summary>
        /// Get Top Rated Movies Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetTopRatedMoviesAsync()
        {
            stopwatch.Restart();
            (await _dal.GetMoviesByQueryAsync(string.Empty, toprated: true)).ToList<Movie>();
            stopwatch.Stop();

            return BuildHealthzCheck("/api/movies?toprated=true", stopwatch.Elapsed, 200);
        }

        /// <summary>
        /// Get Actor By Id Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetActorByIdAsync(string actorId)
        {
            stopwatch.Restart();
            await _dal.GetActorAsync(actorId);
            stopwatch.Stop();

            return BuildHealthzCheck($"/api/actors/{actorId}", stopwatch.Elapsed, 200);
        }

        /// <summary>
        /// Search Actors Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> SearchActorsAsync(string query)
        {
            stopwatch.Restart();
            (await _dal.GetActorsByQueryAsync(query)).ToList<Actor>();
            stopwatch.Stop();

            return BuildHealthzCheck($"/api/actors?q={query}", stopwatch.Elapsed, 200);
        }
    }
}
