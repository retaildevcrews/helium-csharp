using Microsoft.Azure.Cosmos;
using System;

namespace Helium.DataAccessLayer
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
        private QueryRequestOptions _queryRequestOptions = null;
        private CosmosClientOptions _cosmosClientOptions = null;

        // CosmosDB query request options
        public QueryRequestOptions QueryRequestOptions
        {
            get
            {
                if (_queryRequestOptions == null)
                {
                    _queryRequestOptions = new QueryRequestOptions { MaxItemCount = MaxRows };
                }

                return _queryRequestOptions;
            }
        }

        // default protocol is tcp, default connection mode is direct
        public CosmosClientOptions CosmosClientOptions
        {
            get
            {
                if (_cosmosClientOptions == null)
                {
                    _cosmosClientOptions = new CosmosClientOptions { RequestTimeout = TimeSpan.FromSeconds(Timeout), MaxRetryAttemptsOnRateLimitedRequests = Retries, MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(Timeout) };
                }

                return _cosmosClientOptions;
            }
        }
    }
}