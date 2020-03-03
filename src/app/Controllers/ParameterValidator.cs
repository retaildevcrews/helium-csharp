using Microsoft.AspNetCore.Http;
using System;

namespace Helium.Controllers
{
    /// <summary>
    /// Validate the query string parameters
    /// </summary>
    public static class ParameterValidator
    {
        /// <summary>
        /// validate query string parameters common between Actors and Movies
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
                if (q == null || q.Length < 2 || q.Length > 20)
                {
                    message = "Invalid q (search) parameter";
                    return false;
                }
            }

            // validate page number
            if (query.ContainsKey("pageNumber"))
            {
                if (!int.TryParse(query["pageNumber"], out int val) || val != pageNumber || pageNumber < 1 || pageNumber > 10000)
                {
                    message = "Invalid PageNumber parameter";
                    return false;
                }
            }

            // validate page size
            if (query.ContainsKey("pageSize"))
            {
                if (!int.TryParse(query["pageSize"], out int val) || val != pageSize || pageSize < 1 || pageSize > 1000)
                {
                    message = "Invalid PageSize parameter";
                    return false;
                }
            }

            return true;
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
        /// <param name="message">error message</param>
        /// <returns></returns>
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
                if (genre == null || genre.Length < 3 || genre.Length > 20)
                {
                    message = "Invalid Genre parameter";
                    return false;
                }
            }

            // validate year
            if (query.ContainsKey("year"))
            {
                if (!int.TryParse(query["year"], out int val) || val != year || year < 1874 || year > DateTime.UtcNow.Year + 5)
                {
                    message = "Invalid Year parameter";
                    return false;
                }
            }

            // validate rating
            if (query.ContainsKey("rating"))
            {
                if (!double.TryParse(query["rating"], out double val) || val != rating || rating < 0 || rating > 10)
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

        /// <summary>
        /// validate actorId
        /// </summary>
        /// <param name="actorId">actorId</param>
        /// <param name="message">error message</param>
        /// <returns></returns>
        public static bool ActorId(string actorId, out string message)
        {
            message = string.Empty;

            // validate actorId
            if (actorId == null ||
                actorId.Length < 7 ||
                actorId.Length > 11 ||
                actorId.Substring(0, 2) != "nm" ||
                !int.TryParse(actorId.Substring(2), out int val) ||
                val <= 0)
            {
                message = "Invalid Actor ID parameter";
                return false;
            }

            return true;
        }

        /// <summary>
        /// validate movieId
        /// </summary>
        /// <param name="movieId">movieId</param>
        /// <param name="message">error message</param>
        /// <returns></returns>
        public static bool MovieId(string movieId, out string message)
        {
            message = string.Empty;

            // validate movieId
            if (movieId == null ||
                movieId.Length < 7 ||
                movieId.Length > 11 ||
                movieId.Substring(0, 2) != "tt" ||
                !int.TryParse(movieId.Substring(2), out int val) ||
                val <= 0)
            {
                message = "Invalid Movie ID parameter";
                return false;
            }

            return true;
        }
    }
}
