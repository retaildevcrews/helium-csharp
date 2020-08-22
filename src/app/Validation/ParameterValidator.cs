using System;
using System.Net;
using System.Text;
using CSE.Helium.Enumerations;
using CSE.Helium.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CSE.Helium.Validation
{
    /// <summary>
    /// Validate the query string parameters
    /// </summary>
    public class ParameterValidator : IParameterValidator
    {
        /// <summary>
        /// validate query string parameters common between Actors and Movies
        /// </summary>
        /// <param name="query">IQueryCollection</param>
        /// <param name="q">search</param>
        /// <param name="pageNumber">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public IActionResult ValidateCommonParameters(IQueryCollection query, string q, int pageNumber, int pageSize, string method, ILogger logger)
        {
            // no query string
            if (query == null)
            {
                return null;
            }

            // validate q (search)
            if (query.ContainsKey("q"))
            {
                if (q == null || q.Length < 2 || q.Length > 20)
                {
                    return ValidationProcessor.GetAndLogInvalidSearchParameter(method, logger);
                }
            }

            // validate page number
            if (query.ContainsKey("pageNumber"))
            {
                if (!int.TryParse(query["pageNumber"], out int val) || val != pageNumber || pageNumber < 1 || pageNumber > 10000)
                {
                    return ValidationProcessor.GetAndLogInvalidPageNumberParameter(method, logger);
                }
            }

            // validate page size
            if (query.ContainsKey("pageSize"))
            {
                if (!int.TryParse(query["pageSize"], out int val) || val != pageSize || pageSize < 1 || pageSize > 1000)
                {
                    return ValidationProcessor.GetAndLogInvalidPageSizeParameter(method, logger);
                }
            }

            return null;
        }
        
        /// <summary>
        /// validate query string parameters for Movies
        /// </summary>
        /// <param name="query">IQueryCollection</param>
        /// <param name="q">search</param>
        /// <param name="genre">genre</param>
        /// <param name="year">year</param>
        /// <param name="rating">rating</param>
        /// <param name="actorId">actorId</param>
        /// <param name="pageNumber">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public IActionResult ValidateMovieParameters(IQueryCollection query, string q, string genre, int year, double rating, string actorId, int pageNumber, int pageSize, string method, ILogger logger)
        {
            // no query string
            if (query == null)
            {
                return null;
            }

            // validate q, page number and page size
            var result = ValidateCommonParameters(query, q, pageNumber, pageSize, method, logger);
            if (result != null)
            {
                return result;
            }

            // validate movie genre
            if (query.ContainsKey("genre"))
            {
                if (genre == null || genre.Length < 3 || genre.Length > 20)
                {
                    return ValidationProcessor.GetAndLogInvalidMovieGenreParameter(method, logger);
                }
            }

            // validate movie year
            if (query.ContainsKey("year"))
            {
                if (!int.TryParse(query["year"], out int val) || val != year || year < 1874 || year > DateTime.UtcNow.Year + 5)
                {
                    return ValidationProcessor.GetAndLogInvalidMovieYearParameter(method, logger);
                }
            }

            // validate movie rating
            if (query.ContainsKey("rating"))
            {
                if (!double.TryParse(query["rating"], out double val) || val != rating || rating < 0 || rating > 10)
                {
                    return ValidationProcessor.GetAndLogInvalidMovieRatingParameter(method, logger);
                }
            }

            // validate actorId
            if (query.ContainsKey("actorId"))
            {
                result = ValidateActorId(actorId, method, logger);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// validate actorId
        /// </summary>
        /// <param name="actorId">actorId</param>
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public IActionResult ValidateActorId(string actorId, string method, ILogger logger)
        {
            // validate actorId
            if (actorId == null ||
                actorId.Length < 5 ||
                actorId.Length > 7 ||
                actorId.Substring(0, 2) != "nm" ||
                !int.TryParse(actorId.Substring(2), out int val) ||
                val <= 0)
            {
                return ValidationProcessor.GetAndLogInvalidActorIdParameter(method, logger);
            }

            return null;
        }

        /// <summary>
        /// validate movieId
        /// </summary>
        /// <param name="movieId">movieId</param>
        /// <param name="method">current method</param>
        /// <param name="logger">ILogger</param>
        /// <returns></returns>
        public IActionResult ValidateMovieId(string movieId, string method, ILogger logger)
        {
            // validate movieId
            if (movieId == null ||
                movieId.Length < 7 ||
                movieId.Length > 11 ||
                movieId.Substring(0, 2) != "tt" ||
                !int.TryParse(movieId.Substring(2), out int val) ||
                val <= 0)
            {
                return ValidationProcessor.GetAndLogInvalidMovieIdParameter(method, logger);
            }

            return null;
        }
    }
}
