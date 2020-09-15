using CSE.Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
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
        /// Returns a JSON array of Actor objects based on query parameters
        /// </summary>
        /// <param name="actorQueryParameters"></param>
        [HttpGet]
        public async Task<IActionResult> GetActorsAsync([FromQuery] ActorQueryParameters actorQueryParameters)
        {
            _ = actorQueryParameters ?? throw new ArgumentNullException(nameof(actorQueryParameters));

            return await ResultHandler.Handle(dal.GetActorsAsync(
                    actorQueryParameters.Q,
                    actorQueryParameters.PageNumber * actorQueryParameters.PageSize,
                    actorQueryParameters.PageSize),
                    GetMethodText(actorQueryParameters),
                    Constants.ActorsControllerException,
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
            return await ResultHandler.Handle(dal.GetActorAsync(actorIdParameter.ActorId), method, "Actor Not Found", logger).ConfigureAwait(false);
        }

        /// <summary>
        /// Add parameters to the method name if specified in the query string
        /// </summary>
        /// <param name="actorQueryParameters"></param>
        /// <returns></returns>
        private string GetMethodText(ActorQueryParameters actorQueryParameters)
        {
            string method = "GetActorsAsync";

            if (HttpContext?.Request?.Query != null)
            {
                // add the query parameters to the method name if exists
                if (HttpContext.Request.Query.ContainsKey("q"))
                {
                    method = $"{method}:q:{actorQueryParameters.Q}";
                }
                if (HttpContext.Request.Query.ContainsKey("pageNumber"))
                {
                    method = $"{method}:pageNumber:{actorQueryParameters.PageNumber}";
                }
                if (HttpContext.Request.Query.ContainsKey("pageSize"))
                {
                    method = $"{method}:pageSize:{actorQueryParameters.PageSize}";
                }
            }

            return method;
        }
    }
}
