using Microsoft.Azure.Documents;
using System.Linq;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        /// <summary>
        /// Read the genres from CosmosDB
        /// </summary>
        /// <returns>List of strings</returns>
        public IQueryable<string> GetGenres()
        {
            // get all genres as a list of strings
            // the "select value" converts m.genre to a string instead of a document

            string sql = "select value m.genre from m where m.type = 'Genre' order by m.genre";
            return client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql), feedOptions);
        }

   }
}