using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            GenreObject g = await _cosmosDetails.Container.ReadItemAsync<GenreObject>(key.ToLowerInvariant(), new PartitionKey("0")).ConfigureAwait(false);

            return g.Genre;
        }

        /// <summary>
        /// Internal class for reading Genre json
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Late bound")]
        internal class GenreObject
        {
            public string Genre { get; set; }
        }
    }
}