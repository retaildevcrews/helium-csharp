using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
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
        /// <response code="200">json array of Actor objects or empty array if not found</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Actor[]), 200)]
        public async Task<IActionResult> GetActorsAsync([FromQuery] string q)
        {
            // check the query string
            if (q == null)
            {
                q = string.Empty;
            }

            q = q.Trim();

            string method = string.IsNullOrEmpty(q) ? "GetActorsAsync" : string.Format($"SearchActors:{q}");

            _logger.LogInformation(method, q);

            try
            {
                return Ok(await _dal.GetActorsByQueryAsync(q));
            }

            catch (CosmosException ce)
            {
                // log and return 500
                _logger.LogError($"CosmosException:{method}:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                return new ObjectResult(Constants.ActorsControllerException)
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
                _logger.LogError($"AggregateException|{method}|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new ObjectResult(Constants.ActorsControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            catch (Exception ex)
            {
                _logger.LogError($"Exception:{method}\n{ex}");

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
        public async Task<IActionResult> GetActorByIdAsync(string actorId)
        {
            _logger.LogInformation($"GetActorByIdAsync {actorId}");

            try
            {
                // get a single actor
                return Ok(await _dal.GetActorAsync(actorId));
            }

            // actorId isn't well formed
            catch (ArgumentException)
            {
                _logger.LogInformation($"NotFound:GetActorByIdAsync:{actorId}");

                // return a 404
                return NotFound();
            }

            catch (CosmosException ce)
            {
                // CosmosDB API will throw an exception on an actorId not found
                if (ce.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation($"NotFound:GetActorByIdAsync:{actorId}");

                    // return a 404
                    return NotFound();
                }
                else
                {
                    // log and return 500
                    _logger.LogError($"CosmosException:GetActorByIdAsync:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}\n{ce}");

                    return new ObjectResult(Constants.ActorsControllerException)
                    {
                        StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                    };
                }
            }

            catch (System.AggregateException age)
            {
                var root = age.GetBaseException();

                if (root == null)
                {
                    root = age;
                }

                // log and return 500
                _logger.LogError($"AggregateException|GetActorByIdAsync|{root.GetType()}|{root.Message}|{root.Source}|{root.TargetSite}");

                return new ObjectResult(Constants.ActorsControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }

            // log and return 500
            catch (Exception e)
            {
                _logger.LogError($"Exception:GetActorByIdAsync:{e.Message}\n{e}");

                return new ObjectResult(Constants.ActorsControllerException)
                {
                    StatusCode = (int)System.Net.HttpStatusCode.InternalServerError
                };
            }
        }
    }
}
