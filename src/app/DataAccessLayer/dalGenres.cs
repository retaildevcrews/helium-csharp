using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSE.Helium.DataAccessLayer
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

            List<string> results = new List<string>();

            var q = await InternalCosmosDBSqlQuery<string>(genresSelect).ConfigureAwait(false);

            foreach (string g in q)
            {
                results.Add(g);
            }

            return results;
        }
    }
}