using Helium.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Helium
{
    public partial class CosmosHealthCheck : IHealthCheck
    {
        // TODO - refactor - a lot of code can be shared

        /// <summary>
        /// Get Genres Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
        private async Task<HealthzCheck> GetGenresAsync()
        {
            var result = new HealthzCheck();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetGenresAsync()).ToList<string>();
            sw.Stop();
            result.Uri = "/api/genres";
            result.Status = HealthStatus.Healthy;
            result.Duration = sw.Elapsed;

            if (sw.ElapsedMilliseconds > 200)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        /// <summary>
        /// Get Movie by Id Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
        private async Task<HealthzCheck> GetMovieByIdAsync(string movieId)
        {
            var result = new HealthzCheck();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            await _dal.GetMovieAsync(movieId);
            sw.Stop();
            result.Uri = string.Format($"/api/movies/{movieId}");
            result.Status = HealthStatus.Healthy;
            result.Duration = sw.Elapsed;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        /// <summary>
        /// Search Movies Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
        private async Task<HealthzCheck> SearchMoviesAsync(string query)
        {
            var result = new HealthzCheck();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetMoviesByQueryAsync(query)).ToList<Movie>();
            sw.Stop();
            result.Uri = string.Format($"/api/movies?q={query}");
            result.Status = HealthStatus.Healthy;
            result.Duration = sw.Elapsed;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        /// <summary>
        /// Get Top Rated Movies Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
        private async Task<HealthzCheck> GetTopRatedMoviesAsync()
        {
            var result = new HealthzCheck();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetMoviesByQueryAsync(string.Empty, toprated: true)).ToList<Movie>();
            sw.Stop();
            result.Uri = string.Format($"/api/movies?toprated=true");
            result.Status = HealthStatus.Healthy;
            result.Duration = sw.Elapsed;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        /// <summary>
        /// Get Actor By Id Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
        private async Task<HealthzCheck> GetActorByIdAsync(string actorId)
        {
            var result = new HealthzCheck();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            await _dal.GetActorAsync(actorId);
            sw.Stop();
            result.Uri = string.Format($"/api/actors/{actorId}");
            result.Status = HealthStatus.Healthy;
            result.Duration = sw.Elapsed;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        /// <summary>
        /// Search Actors Healthcheck
        /// </summary>
        /// <returns>HealthzResult</returns>
        private async Task<HealthzCheck> SearchActorsAsync(string query)
        {
            var result = new HealthzCheck();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetActorsByQueryAsync(query)).ToList<Actor>();
            sw.Stop();
            result.Uri = string.Format($"/api/actors?q={query}");
            result.Status = HealthStatus.Healthy;
            result.Duration = sw.Elapsed;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }
    }
}
