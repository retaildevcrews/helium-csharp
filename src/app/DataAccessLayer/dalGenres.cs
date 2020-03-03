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
        const string _genresSelect = "select value m.genre from m where m.type = 'Genre' order by m.genre";

        /// <summary>
        /// Read the genres from CosmosDB
        /// </summary>
        /// <returns>List of strings</returns>
        public async Task<IEnumerable<string>> GetGenresAsync()
        {
            // get all genres as a list of strings
            // the "select value" converts m.genre to a string instead of a document

            List<string> results = new List<string>();

            var q = await InternalCosmosDBSqlQuery<string>(_genresSelect).ConfigureAwait(false);

            foreach (string g in q)
            {
                results.Add(g);
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
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            // we know the partition key is 0
            PartitionKey partitionKey = new PartitionKey("0");

            // read the genre from Cosmos
            // this will throw exception if not found
            var ir = await _cosmosDetails.Container.ReadItemAsync<IDictionary<string, string>>(key.ToLowerInvariant(), partitionKey).ConfigureAwait(false);

            // return the value
            return ir.Resource["genre"];
        }
    }
}