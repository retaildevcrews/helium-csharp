using Helium.Model;
using System.Threading.Tasks;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        const string healthzSelect = "select value count(1) from m where m.type = '{0}'";

        public async Task<HealthzSuccessDetails> GetHealthzAsync()
        {
            HealthzSuccessDetails d = new HealthzSuccessDetails();

            // get count of documents for each type
            d.Actors = await GetCountAsync("Actor");
            d.Movies = await GetCountAsync("Movie");
            d.Genres = await GetCountAsync("Genre");

            return d;
        }

        /// <summary>
        /// Query CosmosDB for the count of a type (Movie, Actor, Genre)
        /// </summary>
        /// <param name="type">the type to count</param>
        /// <returns>string - count of documents of type</returns>
        private async Task<long> GetCountAsync(string type)
        {
            string sql = string.Format(healthzSelect, type);

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