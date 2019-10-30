using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        const string genresSelect = "select value m.genre from m where m.type = 'Genre' order by m.genre";

        /// <summary>
        /// Read the genres from CosmosDB
        /// </summary>
        /// <returns>List of strings</returns>
        public async Task<IEnumerable<string>> GetGenresAsync()
        {
            // get all genres as a list of strings
            // the "select value" converts m.genre to a string instead of a document
            var query = container.GetItemQueryIterator<string>(new QueryDefinition(genresSelect), requestOptions: queryRequestOptions);

            List<string> results = new List<string>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync())
                {
                    results.Add(doc);
                }
            }
            return results;
        }

        /// <summary>
        /// Look up the proper Genre by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>string.Empty or the Genre</returns>
        public async Task<string> GetGenreAsync(string key)
        {
            string sql = string.Format("select value m.genre from m where m.type = 'Genre' and m.id = '{0}'", key.Trim().ToLower());

            var query = container.GetItemQueryIterator<string>(new QueryDefinition(sql), requestOptions: queryRequestOptions);

            List<string> results = new List<string>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync())
                {
                    results.Add(doc);
                }
            }

            if (results != null && results.Count > 0)
            {
                return results[0];
            }

            return string.Empty;
        }
    }
}