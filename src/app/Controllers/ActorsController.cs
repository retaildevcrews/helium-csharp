using CSE.Helium.DataAccessLayer;
using Helium.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    /// <summary>
    /// Handle all of the /api/actors requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ActorsController : BaseController
    {
        private readonly ILogger logger;
        private readonly IDAL dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public ActorsController(ILogger<ActorsController> logger, IDAL dal, IConfiguration configuration) : base(logger, dal, configuration)
        {
            // save to local for use in handlers
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// Returns a JSON array of Actor objects based on query parameters
        /// </summary>
        /// <param name="actorQueryParameters"></param>
        [HttpGet]
        public async Task<IActionResult> GetActorsAsync([FromQuery] ActorQueryParameters actorQueryParameters)
        {
            _ = actorQueryParameters ?? throw new ArgumentNullException(nameof(actorQueryParameters));

            return await Handle(
                   RetryCosmosPolicy.ExecuteAsync(()=> dal.GetActorsAsync(actorQueryParameters)), actorQueryParameters.GetMethodText(HttpContext), Constants.ActorsControllerException,
                    logger)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a single JSON Actor by actorId
        /// </summary>
        /// <param name="actorIdParameter">The actorId</param>
        /// <response code="404">actorId not found</response>
        [HttpGet("{actorId}")]
        public async Task<IActionResult> GetActorByIdAsync([FromRoute] ActorIdParameter actorIdParameter)
        {
            _ = actorIdParameter ?? throw new ArgumentNullException(nameof(actorIdParameter));

            string method = nameof(GetActorByIdAsync) + actorIdParameter.ActorId;

            // return result
            return await Handle(
                RetryCosmosPolicy.ExecuteAsync(() => dal.GetActorAsync(actorIdParameter.ActorId)), method, "Actor Not Found", logger)
                .ConfigureAwait(false);
        }
    }
}
