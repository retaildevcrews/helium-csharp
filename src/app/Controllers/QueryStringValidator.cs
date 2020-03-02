using Helium.DataAccessLayer;
using Helium.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Helium.Controllers
{
    /// <summary>
    /// Validate the query string parameters
    /// </summary>
    public static class QueryStringValidator
    {
        /// <summary>
        /// Validate parameters common between Actors and Movies
        /// </summary>
        /// <param name="query">IQueryCollection</param>
        /// <param name="q">search</param>
        /// <param name="pageNumber">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="message">error message</param>
        /// <returns></returns>
        public static bool Common(IQueryCollection query, string q, int pageNumber, int pageSize, out string message)
        {
            message = string.Empty;

            // no query string
            if (query == null)
            {
                return true;
            }

            // validate q (search)
            if (query.ContainsKey("q"))
            {
                if (q == null || q.Length < 3 || q.Length > 50)
                {
                    message = "Invalid q parameter";
                    return false;
                }
            }

            // validate page number
            if (query.ContainsKey("pageNumber"))
            {
                if (pageNumber < 1 || pageNumber > 10000)
                {
                    message = "Invalid PageNumber parameter";
                    return false;
                }
            }

            // validate page size
            if (query.ContainsKey("pageSize"))
            {
                if (pageSize < 1 || pageSize > 1000)
                {
                    message = "Invalid PageSize parameter";
                    return false;
                }
            }

            return true;
        }

        public static bool Movies(IQueryCollection query, string q, string genre, int year, double rating, string actorId, int pageNumber, int pageSize, out string message)
        {
            message = string.Empty;

            // no query string
            if (query == null)
            {
                return true;
            }

            // validate q, page number and page size
            if (!Common(query, q, pageNumber, pageSize, out message))
            {
                return false;
            }

            // validate genre
            if (query.ContainsKey("genre"))
            {
                if (genre == null || genre.Length < 3 || genre.Length > 50)
                {
                    message = "Invalid Genre parameter";
                    return false;
                }
            }

            // validate year
            if (query.ContainsKey("year"))
            {
                if (year < 1874 || year > DateTime.UtcNow.Year + 5)
                {
                    message = "Invalid Year parameter";
                    return false;
                }
            }

            // validate rating
            if (query.ContainsKey("rating"))
            {
                if (rating < 0 || rating > 10)
                {
                    message = "Invalid Rating parameter";
                    return false;
                }
            }

            // validate actorId
            if (query.ContainsKey("actorId"))
            {
                if (!ActorId(actorId, out message))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ActorId(string actorId, out string message)
        {
            message = string.Empty;

            // validate actorId
            if (actorId == null ||
                actorId.Length < 7 ||
                actorId.Length > 11 ||
                actorId.Substring(0, 2) != "nm" ||
                !int.TryParse(actorId.Substring(2), out int _))
            {
                message = "Invalid Actor ID parameter";
                return false;
            }

            return true;
        }

        public static bool MovieId(string movieId, out string message)
        {
            message = string.Empty;

            // validate movieId
            if (movieId == null ||
                movieId.Length < 7 ||
                movieId.Length > 11 ||
                movieId.Substring(0, 2) != "tt" ||
                !int.TryParse(movieId.Substring(2), out int _))
            {
                message = "Invalid Movie ID parameter";
                return false;
            }

            return true;
        }
    }
}
