using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Handle all of the /api/actors requests
    /// </summary>
    [Route("api/[controller]")]
    public class ActorsController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDAL _dal;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public ActorsController(ILogger<ActorsController> logger, IDAL dal)
        {
            // save to local for use in handlers
            _logger = logger;
            _dal = dal;
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a JSON array of Actor objects</remarks>
        /// <param name="q">(optional) The term used to search Actor name</param>
        /// <param name="pageNumber">1 based page index</param>
        /// <param name="pageSize">page size (1000 max)</param>
        /// <response code="200">JSON array of Actor objects or empty array if not found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Actor[]), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> GetActorsAsync([FromQuery] string q, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = Constants.DefaultPageSize)
        {
            string method = GetMethodText(q, pageNumber, pageSize);

            // validate query string parameters
            if (!ParameterValidator.Common(HttpContext?.Request?.Query, q, pageNumber, pageSize, method, _logger, out ContentResult result))
            {
                return result;
            }

            // convert to zero based index
            pageNumber = pageNumber > 1 ? pageNumber - 1 : 0;

            return await ResultHandler.Handle(_dal.GetActorsByQueryAsync(q, pageNumber * pageSize, pageSize), method, Constants.ActorsControllerException, _logger).ConfigureAwait(false);
        }

        /// <summary>
        /// </summary>
        /// <remarks>Returns a single JSON Actor by actorId</remarks>
        /// <param name="actorId">The actorId</param>
        /// <response code="404">actorId not found</response>
        [HttpGet("{actorId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Actor), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> GetActorByIdAsync(string actorId)
        {
            string method = "GetActorByIdAsync " + actorId;

            // validate actorId
            if (!ParameterValidator.ActorId(actorId, method, _logger, out ContentResult result))
            {
                return result;
            }

            // return result
            return await ResultHandler.Handle(_dal.GetActorAsync(actorId), method, Constants.ActorsControllerException, _logger).ConfigureAwait(false);
        }

        /// <summary>
        /// Add parameters to the method name if specified in the query string
        /// </summary>
        /// <param name="q"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private string GetMethodText(string q, int pageNumber, int pageSize)
        {
            string method = "GetActorsAsync";

            if (HttpContext != null && HttpContext.Request != null && HttpContext.Request.Query != null)
            {
                // add the query parameters to the method name if exists
                if (HttpContext.Request.Query.ContainsKey("q"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:q:{q}");
                }
                if (HttpContext.Request.Query.ContainsKey("pageNumber"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:pageNumber:{pageNumber}");
                }
                if (HttpContext.Request.Query.ContainsKey("pageSize"))
                {
                    method = string.Format(CultureInfo.InvariantCulture, $"{method}:pageSize:{pageSize}");
                }
            }

            return method;
        }
    }
}
