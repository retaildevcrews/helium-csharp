using Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using System;

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
        public IActionResult GetGenres()
        {
            // get list of genres as list of string
            logger.LogInformation("GetGenres");

            try
            {
                return Ok(dal.GetGenres());
            }

            catch (DocumentClientException dce)
            {
                // log and return 500
                logger.LogError("DocumentClientException:GetGenres:{0}:{1}:{2}:{3}\r\n{4}", dce.StatusCode, dce.Error, dce.ActivityId, dce.Message, dce);

                return new ObjectResult(Constants.GenresControllerException)
                {
                    StatusCode = Constants.ServerError
                };
            }

            catch (Exception ex)
            {
                // log and return 500
                logger.LogError("Exception:GetGenres\r\n{0}", ex);

                return new ObjectResult(Constants.GenresControllerException)
                {
                    StatusCode = Constants.ServerError
                };
            }
        }
    }
}
