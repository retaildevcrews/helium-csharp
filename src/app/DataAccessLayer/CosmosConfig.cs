using Microsoft.Azure.Cosmos;
using System;

namespace CSE.Helium.DataAccessLayer
{
    /// <summary>
    /// Internal class for Cosmos config
    /// </summary>
    internal class CosmosConfig
    {
        public CosmosClient Client = null;
        public Container Container = null;

        // default values for Cosmos Options
        public int MaxRows = 1000;
        public int Timeout = 60;
        public int Retries = 10;

        // Cosmos connection fields
        public string CosmosUrl;
        public string CosmosKey;
        public string CosmosDatabase;
        public string CosmosCollection;

        // member variables
        private QueryRequestOptions queryRequestOptions = null;
        private CosmosClientOptions cosmosClientOptions = null;

        // CosmosDB query request options
        public QueryRequestOptions QueryRequestOptions
        {
            get
            {
                if (queryRequestOptions == null)
                {
                    queryRequestOptions = new QueryRequestOptions { MaxItemCount = MaxRows };
                }

                return queryRequestOptions;
            }
        }

        // default protocol is tcp, default connection mode is direct
        public CosmosClientOptions CosmosClientOptions
        {
            get
            {
                if (cosmosClientOptions == null)
                {
                    cosmosClientOptions = new CosmosClientOptions { RequestTimeout = TimeSpan.FromSeconds(Timeout), MaxRetryAttemptsOnRateLimitedRequests = Retries, MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(Timeout) };
                }

                return cosmosClientOptions;
            }
        }
    }
}