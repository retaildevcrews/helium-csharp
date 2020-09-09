using CSE.Helium.Model;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CSE.Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        // select template for Movies
        const string movieSelect = "select m.id, m.partitionKey, m.movieId, m.type, m.textSearch, m.title, m.year, m.runtime, m.rating, m.votes, m.totalScore, m.genres, m.roles from m where m.type = 'Movie' ";
        const string movieOrderBy = " order by m.textSearch ASC, m.movieId ASC";
        const string movieOffset = " offset {0} limit {1}";

        /// <summary>
        /// Retrieve a single Movie from CosmosDB by movieId
        /// 
        /// Uses the CosmosDB single document read API which is 1 RU if less than 1K doc size
        /// 
        /// Throws an exception if not found
        /// </summary>
        /// <param name="movieId">Movie ID</param>
        /// <returns>Movie object</returns>
        public async Task<Movie> GetMovieAsync(string movieId)
        {
            // get the partition key for the movie ID
            // note: if the key cannot be determined from the ID, ReadDocumentAsync cannot be used.
            // ComputePartitionKey will throw an ArgumentException if the movieId isn't valid
            // get a movie by ID
            return await cosmosDetails.Container.ReadItemAsync<Movie>(movieId, new PartitionKey(Movie.ComputePartitionKey(movieId))).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of Movies by search and/or filter terms
        /// </summary>
        /// <param name="q">search term</param>
        /// <param name="genre">get movies by genre</param>
        /// <param name="year">get movies by year</param>
        /// <param name="rating">get movies rated >= rating</param>
        /// <param name="actorId">get movies by actorId</param>
        /// <param name="offset">zero based offset for paging</param>
        /// <param name="limit">number of documents for paging</param>
        /// <returns>List of Movies or an empty list</returns>
        public async Task<IEnumerable<Movie>> GetMoviesAsync(string q, string genre = "", int year = 0, double rating = 0, string actorId = "", int offset = 0, int limit = Constants.DefaultPageSize)
        {

            string sql = movieSelect;

            if (limit < 1)
            {
                limit = Constants.DefaultPageSize;
            }
            else if (limit > Constants.MaxPageSize)
            {
                limit = Constants.MaxPageSize;
            }

            string offsetLimit = string.Format(CultureInfo.InvariantCulture, movieOffset, offset, limit);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                sql += " and contains(m.title, @q, true) ";
            }

            if (year > 0)
            {
                sql += " and m.year = @year ";
            }

            if (rating > 0)
            {
                sql += " and m.rating >= @rating ";
            }

            if (!string.IsNullOrWhiteSpace(actorId))
            {
                // convert to lower
                actorId = actorId.Trim().ToLowerInvariant();
                sql += " and array_contains(m.roles, { actorId: @actorId }, true) ";
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                genre = genre.Trim();
                sql += " and contains(m.genreSearch, @genre, true) ";
            }

            sql += movieOrderBy + offsetLimit;

            // Parameterize fields
            QueryDefinition queryDefinition = new QueryDefinition(sql);

            if (!string.IsNullOrWhiteSpace(q))
            {
                queryDefinition.WithParameter("@q", q);
            }
            if (!string.IsNullOrWhiteSpace(actorId))
            {
                queryDefinition.WithParameter("@actorId", actorId);
            }
            if (!string.IsNullOrWhiteSpace(genre))
            {
                // genreSearch is stored delimited with :
                queryDefinition.WithParameter("@genre", "|" + genre + "|");
            }
            if (year > 0)
            {
                queryDefinition.WithParameter("@year", year);
            }
            if (rating > 0)
            {
                queryDefinition.WithParameter("@rating", rating);
            }

            return await InternalCosmosDBSqlQuery<Movie>(queryDefinition).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the featured movie list from Cosmos
        /// </summary>
        /// <returns>List</returns>
        public async Task<List<string>> GetFeaturedMovieListAsync()
        {
            List<string> list = new List<string>();

            string sql = "select m.movieId, m.weight from m where m.type = 'Featured'";

            try
            {
                var query = await InternalCosmosDBSqlQuery<FeaturedMovie>(sql).ConfigureAwait(false);

                foreach (FeaturedMovie f in query)
                {
                    // apply weighting
                    for (int i = 0; i < f.Weight; i++)
                    {
                        list.Add(f.MovieId);
                    }
                }
            }
            // ignore error and return default
            catch { }

            // default to The Matrix
            if (list.Count == 0)
            {
                list.Add("tt0133093");
            }

            return list;
        }

    }

}