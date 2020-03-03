using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle the single /api/genres requests
    /// </summary>
    [Route("api/[controller]")]
    public class GenresController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDAL _dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public GenresController(ILogger<GenresController> logger, IDAL dal)
        {
            _logger = logger;
            _dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a JSON string array of Genre</remarks>
        /// <response code="200">JSON array of strings or empty array if not found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), 200)]
        public async Task<IActionResult> GetGenresAsync()
        {
            // get list of genres as list of string
            _logger.LogInformation(nameof(GetGenresAsync));

            try
            {
                return Ok(await _dal.GetGenresAsync().ConfigureAwait(false));
            }

            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                _logger.LogError($"CosmosException:GetGenres:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                return new ContentResult
                {
                    Content = Constants.GenresControllerException,
                    StatusCode = (int)ce.StatusCode
                };
            }

            catch (System.AggregateException age)
            {
                var root = age.GetBaseException();

                if (root == null)
                {
                    root = age;
                }

                // log and return 500
                _logger.LogError($"AggregateException|GetGenres|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new ContentResult
                {
                    Content = Constants.GenresControllerException,
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception ex)
            {
                // log and return 500
                _logger.LogError($"Exception:GetGenres\n{ex}");

                return new ContentResult
                {
                    Content = Constants.GenresControllerException,
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
