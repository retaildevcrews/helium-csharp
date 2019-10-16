using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using System;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle all of the /api/actors requests
    /// </summary>
    [Route("api/[controller]")]
    public class ActorsController : Controller
    {
        private readonly ILogger logger;
        private readonly IDAL dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public ActorsController(ILogger<ActorsController> logger, IDAL dal)
        {
            // save to local for use in handlers
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a JSON array of Actor objects</remarks>
        /// <param name="q">(optional) The term used to search Actor name</param>
        /// <response code="200">json array of Actor objects or empty array if not found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Actor[]), 200)]
        public IActionResult GetActors([FromQuery] string q)
        {
            // check the query string
            if (q == null)
            {
                q = string.Empty;
            }

            q = q.Trim();

            string method = string.IsNullOrEmpty(q) ? "GetActors" : string.Format("SearchActors:{0}", q);

            logger.LogInformation(method, q);

            try
            {
                return Ok(dal.GetActorsByQuery(q));
            }

            catch (DocumentClientException dce)
            {
                // log and return 500
                logger.LogError("DocumentClientException:" + method + ":{0}:{1}:{2}:{3}\r\n{4}", dce.StatusCode, dce.Error, dce.ActivityId, dce.Message, dce);

                return new ObjectResult(Constants.ActorsControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception ex)
            {
                logger.LogError("Exception:" + method + "\r\n{0}", ex);

                return new ObjectResult(Constants.ActorsControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a single Actor object by actorId</remarks>
        /// <param name="actorId">The actorId</param>
        /// <response code="404">actorId not found</response>
        [HttpGet("{actorId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Actor), 200)]
        [ProducesResponseType(typeof(void), 404)]
        public async System.Threading.Tasks.Task<IActionResult> GetActorByIdAsync(string actorId)
        {
            logger.LogInformation("GetActorByIdAsync {0}", actorId);

            try
            {
                // get a single actor
                return Ok(await dal.GetActorAsync(actorId));
            }

            // actorId isn't well formed
            catch (ArgumentException)
            {
                logger.LogInformation("NotFound:GetActorByIdAsync:{0}", actorId);

                // return a 404
                return NotFound();
            }

            catch (DocumentClientException dce)
            {
                // CosmosDB API will throw an exception on an actorId not found
                if (dce.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogInformation("NotFound:GetActorByIdAsync:{0}", actorId);

                    // return a 404
                    return NotFound();
                }
                else
                {
                    // log and return 500
                    logger.LogError("DocumentClientException:GetActorByIdAsync:{0}:{1}:{2}:{3}\r\n{4}", dce.StatusCode, dce.Error, dce.ActivityId, dce.Message, dce);

                    return new ObjectResult(Constants.ActorsControllerException)
                    {
                        StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                    };
                }
            }

            // log and return 500
            catch (Exception e)
            {
                logger.LogError("Exception:GetActorByIdAsync:{0}\r\n{1}", e.Message, e);

                return new ObjectResult(Constants.ActorsControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
