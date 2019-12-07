using Helium.Model;
using System.Threading.Tasks;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        // TODO - should be able to delete this file

        const string _healthzSelect = "select value count(1) from m where m.type = '{0}'";

        /// <summary>
        /// Query CosmosDB for the count of a type (Movie, Actor, Genre)
        /// </summary>
        /// <param name="type">the type to count</param>
        /// <returns>string - count of documents of type</returns>
        private async Task<long> GetCountAsync(string type)
        {
            string sql = string.Format(_healthzSelect, type);

            var query = QueryWorker(sql);

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync())
                {
                    return doc;
                }
            }

            return -1;
        }
    }
}