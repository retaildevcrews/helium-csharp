using Microsoft.Azure.Cosmos;
using System;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL : IDAL
    {
        // CosmosDB query request options
        private readonly QueryRequestOptions queryRequestOptions = new QueryRequestOptions { MaxItemCount = 2000 };

        // default protocol is tcp, default connection mode is direct
        private readonly CosmosClientOptions cosmosClientOptions = new CosmosClientOptions { RequestTimeout = TimeSpan.FromSeconds(60), MaxRetryAttemptsOnRateLimitedRequests = 9, MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60) };
        private CosmosClient client = null;
        private Container container = null;

        /// <summary>
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="cosmosUrl">CosmosDB URL</param>
        /// <param name="cosmosKey">CosmosDB connection key</param>
        /// <param name="cosmosDatabase">CosmosDB Database</param>
        /// <param name="cosmosCollection">CosmosDB Collection</param>
        public DAL(string cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            // create the CosmosDB client and container
            client = new CosmosClient(cosmosUrl, cosmosKey, cosmosClientOptions);

            container = client.GetContainer(cosmosDatabase, cosmosCollection);
        }

        /// <summary>
        /// Worker method that executes a query
        /// </summary>
        /// <param name="sql">the select statement to execute</param>
        /// <returns>results of the query</returns>
        public FeedIterator<dynamic> QueryWorker(string sql)
        {
            return container.GetItemQueryIterator<dynamic>(sql, requestOptions: queryRequestOptions);
        }

        public void Reconnect(string cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            var c = new CosmosClient(cosmosUrl, cosmosKey, cosmosClientOptions);
            client = c;
            container = client.GetContainer(cosmosDatabase, cosmosCollection);
        }

        /// <summary>
        /// Compute the partition key based on the movieId or actorId
        /// 
        /// For this sample, the partitionkey is the id mod 10
        /// 
        /// In a full implementation, you would update the logic to determine the partition key
        /// </summary>
        /// <param name="id">document id</param>
        /// <returns>the partition key</returns>
        public static string GetPartitionKey(string id)
        {
            // validate id
            if (id.Length > 5 &&
                (id.StartsWith("tt") || id.StartsWith("nm")) &&
                Int32.TryParse(id.Substring(2), out int idInt))
            {
                return (idInt % 10).ToString();
            }

            throw new ArgumentException("GetPartitionKey");
        }
    }
}