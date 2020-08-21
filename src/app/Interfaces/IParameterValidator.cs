using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CSE.Helium.Interfaces
{
    public interface IParameterValidator
    {
        IActionResult ValidateCommonParameters(IQueryCollection query, string q, int pageNumber, int pageSize, string method,
            ILogger logger);

        IActionResult ValidateMovieParameters(IQueryCollection query, string q, string genre, int year, double rating, string actorId, int pageNumber, int pageSize, string method, ILogger logger);

        IActionResult ValidateActorId(string actorId, string method, ILogger logger);

        IActionResult ValidateMovieId(string movieId, string method, ILogger logger);
    }
}