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

namespace Helium.Controllers
{
    /// <summary>
    /// Handle the /healthz request
    /// </summary>
    [Route("[controller]")]
    public class HealthzController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDAL _dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public HealthzController(ILogger<HealthzController> logger, IDAL dal)
        {
            _logger = logger;
            _dal = dal;
        }

        private async Task<HealthzResult> GetGenresAsync()
        {
            var result = new HealthzResult();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            (await _dal.GetGenresAsync()).ToList<string>();
            sw.Stop();
            result.Uri = "/api/genres";
            result.StatusCode = HealthzStatusCode.Up;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 200)
            {
                result.StatusCode = HealthzStatusCode.Warn;
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
            result.StatusCode = HealthzStatusCode.Up;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Warn;
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
            result.StatusCode = HealthzStatusCode.Up;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Warn;
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
            result.StatusCode = HealthzStatusCode.Up;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Warn;
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
            result.StatusCode = HealthzStatusCode.Up;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Warn;
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
            result.StatusCode = HealthzStatusCode.Up;
            result.TotalMilliseconds = sw.ElapsedMilliseconds;

            if (sw.ElapsedMilliseconds > 100)
            {
                result.StatusCode = HealthzStatusCode.Warn;
                result.Message = "Request exceeded expected duration";
            }

            return result;
        }

        /// <summary>
        /// The health check end point
        /// </summary>
        /// <remarks>
        /// Returns a HealthzSuccess or HealthzError as application/json
        /// </remarks>
        /// <returns>200 OK or 503 Error</returns>
        /// <response code="200">Returns a HealthzSuccess as application/json</response>
        /// <response code="503">Returns a HealthzError as application/json due to unexpected results</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(HealthzStatus), 200)]
        [ProducesResponseType(typeof(HealthzStatus), 503)]
        public async Task<IActionResult> HealthzAsync()
        {
            HealthzStatus stat = new HealthzStatus();

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

                return Ok(stat);
            }

            catch (CosmosException ce)
            {
                // log and return 503
                _logger.LogError($"CosmosException:Healthz:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                stat.Message = ce.Message;

                return new ObjectResult(stat)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable
                };
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

                return new ObjectResult(stat)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception ex)
            {
                // log and return 500
                _logger.LogError($"Exception:Healthz\n{ex}");

                stat.Message = ex.Message;

                return new ObjectResult(stat)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.ServiceUnavailable
                };
            }
        }
    }
}
