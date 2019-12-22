using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL : IDAL
    {
        private CosmosDetails _cosmosDetails = null;

        /// <summary>
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="cosmosAddress">CosmosDB Address</param>
        /// <param name="cosmosKey">CosmosDB connection key</param>
        /// <param name="cosmosDatabase">CosmosDB Database</param>
        /// <param name="cosmosCollection">CosmosDB Collection</param>
        public DAL(string cosmosAddress, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            _cosmosDetails = new CosmosDetails
            {
                CosmosCollection = cosmosCollection,
                CosmosDatabase = cosmosDatabase,
                CosmosKey = cosmosKey,
                CosmosUrl = cosmosAddress
            };

            // create the CosmosDB client and container
            _cosmosDetails.Client = OpenAndTestCosmosClient(cosmosAddress, cosmosKey, cosmosDatabase, cosmosCollection).GetAwaiter().GetResult();
            _cosmosDetails.Container = _cosmosDetails.Client.GetContainer(cosmosDatabase, cosmosCollection);
        }

        /// <summary>
        /// Worker method that executes a query
        /// </summary>
        /// <param name="sql">the select statement to execute</param>
        /// <returns>results of the query</returns>
        public FeedIterator<dynamic> QueryWorker(string sql)
        {
            return _cosmosDetails.Container.GetItemQueryIterator<dynamic>(sql, requestOptions: _cosmosDetails.QueryRequestOptions);
        }

        /// <summary>
        /// Recreate the Cosmos Client / Container (after a key rotation)
        /// </summary>
        /// <param name="cosmosAddress">Cosmos address</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <param name="cosmosDatabase">Cosmos Database</param>
        /// <param name="cosmosCollection">Cosmos Collection</param>
        /// <param name="force">force reconnection even if no params changed</param>
        /// <returns>Task</returns>
        public async Task Reconnect(string cosmosAddress, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false)
        {
            if (force ||
                _cosmosDetails.CosmosCollection != cosmosCollection ||
                _cosmosDetails.CosmosDatabase != cosmosDatabase ||
                _cosmosDetails.CosmosKey != cosmosKey ||
                _cosmosDetails.CosmosUrl != cosmosAddress)
            {
                CosmosDetails d = new CosmosDetails
                {
                    CosmosCollection = cosmosCollection,
                    CosmosDatabase = cosmosDatabase,
                    CosmosKey = cosmosKey,
                    CosmosUrl = cosmosAddress
                };

                // open and test a new client / container
                d.Client = await OpenAndTestCosmosClient(cosmosAddress, cosmosKey, cosmosDatabase, cosmosCollection).ConfigureAwait(false);
                d.Container = d.Client.GetContainer(cosmosDatabase, cosmosCollection);

                // set the current CosmosDetail
                _cosmosDetails = d;
            }
        }

        /// <summary>
        /// Open and test the Cosmos Client / Container / Query
        /// </summary>
        /// <param name="cosmosUrl">Cosmos URL</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <param name="cosmosDatabase">Cosmos Database</param>
        /// <param name="cosmosCollection">Cosmos Collection</param>
        /// <returns>An open and validated CosmosClient</returns>
        private async Task<CosmosClient> OpenAndTestCosmosClient(string cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            // validate required parameters
            if (string.IsNullOrEmpty(cosmosUrl))
            {
                throw new ArgumentException(string.Format($"CosmosUrl not set correctly {cosmosUrl}"));
            }

            if (string.IsNullOrEmpty(cosmosKey))
            {
                throw new ArgumentException(string.Format($"CosmosKey not set correctly {cosmosKey}"));
            }

            if (string.IsNullOrEmpty(cosmosDatabase))
            {
                throw new ArgumentException(string.Format($"CosmosDatabase not set correctly {cosmosDatabase}"));
            }

            if (string.IsNullOrEmpty(cosmosCollection))
            {
                throw new ArgumentException(string.Format($"CosmosCollection not set correctly {cosmosCollection}"));
            }

            // open and test a new client / container
            var c = new CosmosClient(cosmosUrl, cosmosKey, _cosmosDetails.CosmosClientOptions);
            var con = c.GetContainer(cosmosDatabase, cosmosCollection);
            await con.ReadItemAsync<dynamic>("action", new PartitionKey("0").ConfigureAwait(false));

            return c;
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

    internal class CosmosDetails
    {
        public CosmosClient Client = null;
        public Container Container = null;

        public string CosmosUrl;
        public string CosmosKey;
        public string CosmosDatabase;
        public string CosmosCollection;

        // CosmosDB query request options
        public readonly QueryRequestOptions QueryRequestOptions = new QueryRequestOptions { MaxItemCount = 600 };

        // default protocol is tcp, default connection mode is direct
        public readonly CosmosClientOptions CosmosClientOptions = new CosmosClientOptions { RequestTimeout = TimeSpan.FromSeconds(60), MaxRetryAttemptsOnRateLimitedRequests = 9, MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60) };
    }
}