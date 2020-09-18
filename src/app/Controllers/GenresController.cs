using CSE.Helium.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CSE.Helium.Controllers
{
    /// <summary>
    /// Handle the single /api/genres requests
    /// </summary>
    [Route("api/[controller]")]
    public class GenresController : BaseController
    {
        private readonly ILogger logger;
        private readonly IDAL dal;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">log instance</param>
        /// <param name="dal">data access layer instance</param>
        public GenresController(ILogger<GenresController> logger, IDAL dal, IConfiguration configuration) : base(logger, dal, configuration)
        {
            this.logger = logger;
            this.dal = dal;
        }

        /// <summary>
        /// Returns a JSON string array of Genre
        /// </summary>
        /// <response code="200">JSON array of strings or empty array if not found</response>
        [HttpGet]
        public async Task<IActionResult> GetGenresAsync()
        {
            // get list of genres as list of string
            return await Handle(RetryCosmosPolicy.ExecuteAsync(()=> dal.GetGenresAsync()), nameof(GetGenresAsync), Constants.GenresControllerException, logger).ConfigureAwait(false);
        }
    }
}
