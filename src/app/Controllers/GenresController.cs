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
        private readonly ILogger logger;
        private readonly IDAL dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public GenresController(ILogger<GenresController> logger, IDAL dal)
        {
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a JSON string array of Genre</remarks>
        /// <response code="200">json array of strings or empty array if not found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), 200)]
        public async Task<IActionResult> GetGenresAsync()
        {
            // get list of genres as list of string
            logger.LogInformation("GetGenres");

            try
            {
                return Ok(await dal.GetGenresAsync());
            }

            catch (CosmosException ce)
            {
                // log and return 500
                logger.LogError("CosmosException:GetGenres:{0}:{1}:{2}:{3}\r\n{4}", ce.StatusCode, ce.ActivityId, ce.Message, ce.ToString());

                return new ObjectResult(Constants.GenresControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
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
                logger.LogError($"AggregateException|GetGenres|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new ObjectResult(Constants.GenresControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception ex)
            {
                // log and return 500
                logger.LogError("Exception:GetGenres\r\n{0}", ex);

                return new ObjectResult(Constants.GenresControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
