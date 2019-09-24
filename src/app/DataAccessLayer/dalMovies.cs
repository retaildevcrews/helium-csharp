using Helium.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System.Threading.Tasks;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        // select template for movies
        const string movieSelect = "select m.id, m.partitionKey, m.movieId, m.type, m.textSearch, m.title, m.year, m.runtime, m.rating, m.votes, m.totalScore, m.genres, m.roles from m ";

        /// <summary>
        /// Get a single movie by movieId
        /// 
        /// CosmosDB throws an exception if movieId not found
        /// </summary>
        /// <param name="movieId">movie ID to retrieve</param>
        /// <returns>Movie object</returns>
        public async Task<Movie> GetMovieAsync(string movieId)
        {
            // get the partition key for the movie ID
            // note: if the key cannot be determined from the ID, ReadDocumentAsync cannot be used.
            RequestOptions requestOptions = new RequestOptions { PartitionKey = new PartitionKey(GetPartitionKey(movieId)) };

            // get a movie by ID
            return await client.ReadDocumentAsync<Movie>(collectionLink.ToString() + "/docs/" + movieId, requestOptions);
        }

        /// <summary>
        /// Get all movies
        /// </summary>
        /// <returns>List of all Movies</returns>
        public IQueryable<Movie> GetMovies()
        {
            // get all movies
            string sql = movieSelect + "where m.type = 'Movie' order by m.movieId";
            return QueryMovieWorker(sql);
        }

        /// <summary>
        /// Get a list of movies by searching the title
        /// </summary>
        /// <param name="q">search term</param>
        /// <returns>List of Movies or an empty list</returns>
        public IQueryable<Movie> GetMoviesByQuery(string q)
        {
            // get movies by a "like" search on title
            string sql = string.Format("{0} where contains(m.textSearch, '{1}') order by m.movieId", movieSelect, q.ToLower());
            return QueryMovieWorker(sql);
        }

        /// <summary>
        /// Movie worker query
        /// </summary>
        /// <param name="sql">select statement to execute</param>
        /// <returns>List of Movies or empty list</returns>
        public IQueryable<Movie> QueryMovieWorker(string sql)
        {
            // run query
            return client.CreateDocumentQuery<Movie>(collectionLink, sql, feedOptions);
        }
    }
}