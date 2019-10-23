using Helium.Model;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        const string healthzSelect = "select value count(1) from m where m.type = '{0}'";

        public HealthzSuccessDetails GetHealthz()
        {

            HealthzSuccessDetails d = new HealthzSuccessDetails();

            // get count of documents for each type
            d.Actors = GetCount("Actor");
            d.Movies = GetCount("Movie");
            d.Genres = GetCount("Genre");

            return d;
        }

        /// <summary>
        /// Query CosmosDB for the count of a type (Movie, Actor, Genre)
        /// </summary>
        /// <param name="type">the type to count</param>
        /// <returns>string - count of documents of type</returns>
        private long GetCount(string type)
        {
            string sql = string.Format(healthzSelect, type);

            var query = QueryWorker(sql);

            foreach (var doc in query)
            {
                return doc;
            }

            return -1;
        }
    }
}