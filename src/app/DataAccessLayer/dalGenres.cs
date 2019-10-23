using Microsoft.Azure.Documents;
using System.Linq;

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
        public IQueryable<string> GetGenres()
        {
            // get all genres as a list of strings
            // the "select value" converts m.genre to a string instead of a document

            return client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(genresSelect), feedOptions);
        }

        /// <summary>
        /// Look up the proper Genre by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>string.Empty or the Genre</returns>
        public string GetGenre(string key)
        {
            string sql = string.Format("select value m.genre from m where m.type = 'Genre' and m.id = '{0}'", key.Trim().ToLower());

            var q = client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql), feedOptions).ToList<string>();

            if (q != null && q.Count > 0)
            {
                return q[0];
            }

            return string.Empty;
        }
    }
}