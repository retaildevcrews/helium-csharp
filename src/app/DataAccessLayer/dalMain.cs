using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;


namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class IDal : IDAL
    {
        // CosmosDB options
        private readonly FeedOptions feedOptions = new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = 2000 };

        private readonly Uri collectionLink = null;
        private readonly DocumentClient client = null;

        /// <summary>
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="cosmosUrl">CosmosDB URL</param>
        /// <param name="cosmosKey">CosmosDB connection key</param>
        /// <param name="cosmosDatabase">CosmosDB Database</param>
        /// <param name="cosmosCollection">CosmosDB Collection</param>
        public IDal(string cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            // create and open the CosmosDB client
            client = new DocumentClient(new Uri(cosmosUrl), cosmosKey);
            client.OpenAsync();

            // create the collection link
            collectionLink = UriFactory.CreateDocumentCollectionUri(cosmosDatabase, cosmosCollection);

            // run the health query to validate connection
            try
            {
                GetHealthz();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("GetHealthz failed", ex);
            }
        }

        /// <summary>
        /// Worker method that executes a query
        /// </summary>
        /// <param name="sql">the select statement to execute</param>
        /// <returns>results of the query</returns>
        public IQueryable<dynamic> QueryWorker(string sql)
        {
            return client.CreateDocumentQuery<dynamic>(collectionLink, sql, feedOptions);
        }

        /// <summary>
        /// Compute the partition key based on the movieId or actorId
        /// 
        /// For this sample, the partition key is always "0"
        /// 
        /// In a full implementation, you would have multiple partition for scaling
        /// </summary>
        /// <param name="id">document id</param>
        /// <returns>the partition key (always "0" in this implementation)</returns>
        public string GetPartitionKey(string id)
        {
            // validate id
            if (id.Length > 5 &&
                (id.StartsWith("tt") || id.StartsWith("nm")) &&
                Int32.TryParse(id.Substring(2), out int idInt))
            {
                return (idInt % 1).ToString();
            }

            throw new ArgumentException("Invalid id");
        }
    }
}