namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        public string GetHealthz()
        {
            // build the payload
            string res = GetCount("Movie") + "\r\n";
            res += GetCount("Actor") + "\r\n";
            res += GetCount("Genre") +"\r\n";
            res += "Instance: " + System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID") + "\r\n";

            return res;
        }

        /// <summary>
        /// Query CosmosDB for the count of a type (Movie, Actor, Genre)
        /// </summary>
        /// <param name="type">the type to count</param>
        /// <returns>string - count of documents of type</returns>
        private string GetCount(string type)
        {
            string sql = string.Format("select value count(1) from m where m.type = '{0}'", type);

            var query = QueryWorker(sql);

            foreach (var doc in query)
            {
                return string.Format("{0}s: {1}", type, doc);
            }

            return string.Empty;
        }
    }
}