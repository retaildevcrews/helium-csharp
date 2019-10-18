using Helium.Model;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        public HealthzSuccessDetails GetHealthz()
        {

            HealthzSuccessDetails d = new HealthzSuccessDetails();

            d.Actors = GetCount("Actor");
            d.Movies = GetCount("Movie");
            d.Genres = GetCount("Genre");

            // build the payload
            return d;
        }

        /// <summary>
        /// Query CosmosDB for the count of a type (Movie, Actor, Genre)
        /// </summary>
        /// <param name="type">the type to count</param>
        /// <returns>string - count of documents of type</returns>
        private long GetCount(string type)
        {
            string sql = string.Format("select value count(1) from m where m.type = '{0}'", type);

            var query = QueryWorker(sql);

            foreach (var doc in query)
            {
                return doc;
            }

            return -1;
        }
    }
}