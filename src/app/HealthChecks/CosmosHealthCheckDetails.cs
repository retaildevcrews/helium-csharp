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
        /// <param name="targetDurationMs">double (ms)</param>
        /// <returns>HealthzCheck</returns>
        private HealthzCheck BuildHealthzCheck(string uri, double targetDurationMs)
        {
            stopwatch.Stop();

            // create the result
            var result = new HealthzCheck
            {
                Endpoint = uri,
                Status = HealthStatus.Healthy,
                Duration = stopwatch.Elapsed
            };

            // check duration
            if (result.Duration.TotalMilliseconds > targetDurationMs)
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

            return BuildHealthzCheck("/api/genres", 200);
        }

        /// <summary>
        /// Get Movie by Id Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetMovieByIdAsync(string movieId)
        {
            stopwatch.Restart();
            await _dal.GetMovieAsync(movieId);

            return BuildHealthzCheck($"/api/movies/{movieId}", 200);
        }

        /// <summary>
        /// Search Movies Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> SearchMoviesAsync(string query)
        {
            stopwatch.Restart();
            (await _dal.GetMoviesByQueryAsync(query)).ToList<Movie>();

            return BuildHealthzCheck($"/api/movies?q={query}", 200);
        }

        /// <summary>
        /// Get Top Rated Movies Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetTopRatedMoviesAsync()
        {
            stopwatch.Restart();
            (await _dal.GetMoviesByQueryAsync(string.Empty, toprated: true)).ToList<Movie>();

            return BuildHealthzCheck("/api/movies?toprated=true", 200);
        }

        /// <summary>
        /// Get Actor By Id Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> GetActorByIdAsync(string actorId)
        {
            stopwatch.Restart();
            await _dal.GetActorAsync(actorId);

            return BuildHealthzCheck($"/api/actors/{actorId}", 200);
        }

        /// <summary>
        /// Search Actors Healthcheck
        /// </summary>
        /// <returns>HealthzCheck</returns>
        private async Task<HealthzCheck> SearchActorsAsync(string query)
        {
            stopwatch.Restart();
            (await _dal.GetActorsByQueryAsync(query)).ToList<Actor>();

            return BuildHealthzCheck($"/api/actors?q={query}", 200);
        }
    }
}
