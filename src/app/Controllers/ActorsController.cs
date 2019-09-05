using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

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
        public IEnumerable<Actor> GetActors([FromQuery] string q)
        {
            // get actors

            // check the query string for search
            if (!string.IsNullOrEmpty(q))
            {
                logger.LogInformation("GetActorsByQuery {0}", q);

                return dal.GetActorsByQuery(q);
            }

            // get all actors
            logger.LogInformation("GetActors");

            return dal.GetActors();
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
                // CosmosDB API will throw an exception on a bad actorId
                Actor a = await dal.GetActorAsync(actorId);

                return Ok(a);
            }
            catch (Exception e)
            {
                if (e.Message == "Invalid id")
                {
                    logger.LogError("Not Found: GetActorByIdAsync {0}", actorId);

                    // return a 404
                    return NotFound();
                }
                else
                {
                    logger.LogError("GetActorByIdAsync Exception: {0}", e);

                    return new ObjectResult("ActorController exception")
                    {
                        StatusCode = Constants.ServerError
                    };
                }
            }
        }
    }
}
