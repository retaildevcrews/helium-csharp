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
            var query = _cosmosDetails.Container.GetItemQueryIterator<string>(new QueryDefinition(_genresSelect), requestOptions: _cosmosDetails.QueryRequestOptions);

            List<string> results = new List<string>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync().ConfigureAwait(false))
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "key has to be lower case")]
        public async Task<string> GetGenreAsync(string key)
        {
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