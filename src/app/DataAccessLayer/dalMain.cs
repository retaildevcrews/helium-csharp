// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CSE.Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL : IDAL
    {
        private CosmosConfig cosmosDetails;

        public int DefaultPageSize { get; set; } = 100;
        public int MaxPageSize { get; set; } = 1000;
        public int CosmosTimeout { get; set; } = 60;
        public int CosmosMaxRetries { get; set; } = 10;

        /// <summary>
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="cosmosUrl">CosmosDB Url</param>
        /// <param name="cosmosKey">CosmosDB connection key</param>
        /// <param name="cosmosDatabase">CosmosDB Database</param>
        /// <param name="cosmosCollection">CosmosDB Collection</param>
        public DAL(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            if (cosmosUrl == null)
            {
                throw new ArgumentNullException(nameof(cosmosUrl));
            }

            cosmosDetails = new CosmosConfig
            {
                MaxRows = MaxPageSize,

                Timeout = CosmosTimeout,
                CosmosCollection = cosmosCollection,
                CosmosDatabase = cosmosDatabase,
                CosmosKey = cosmosKey,
                CosmosUrl = cosmosUrl.AbsoluteUri,
            };

            // create the CosmosDB client and container
            cosmosDetails.Client = OpenAndTestCosmosClient(cosmosUrl, cosmosKey, cosmosDatabase, cosmosCollection).GetAwaiter().GetResult();
            cosmosDetails.Container = cosmosDetails.Client.GetContainer(cosmosDatabase, cosmosCollection);
        }

        /// <summary>
        /// Recreate the Cosmos Client / Container (after a key rotation)
        /// </summary>
        /// <param name="cosmosUrl">Cosmos URL</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <param name="cosmosDatabase">Cosmos Database</param>
        /// <param name="cosmosCollection">Cosmos Collection</param>
        /// <param name="force">force reconnection even if no params changed</param>
        /// <returns>Task</returns>
        public async Task Reconnect(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false)
        {
            if (cosmosUrl == null)
            {
                throw new ArgumentNullException(nameof(cosmosUrl));
            }

            if (force ||
                cosmosDetails.CosmosCollection != cosmosCollection ||
                cosmosDetails.CosmosDatabase != cosmosDatabase ||
                cosmosDetails.CosmosKey != cosmosKey ||
                cosmosDetails.CosmosUrl != cosmosUrl.AbsoluteUri)
            {
                CosmosConfig d = new CosmosConfig
                {
                    CosmosCollection = cosmosCollection,
                    CosmosDatabase = cosmosDatabase,
                    CosmosKey = cosmosKey,
                    CosmosUrl = cosmosUrl.AbsoluteUri,
                };

                // open and test a new client / container
                d.Client = await OpenAndTestCosmosClient(cosmosUrl, cosmosKey, cosmosDatabase, cosmosCollection).ConfigureAwait(false);
                d.Container = d.Client.GetContainer(cosmosDatabase, cosmosCollection);

                // set the current CosmosDetail
                cosmosDetails = d;
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
        private async Task<CosmosClient> OpenAndTestCosmosClient(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            // validate required parameters
            if (cosmosUrl == null)
            {
                throw new ArgumentNullException(nameof(cosmosUrl));
            }

            if (string.IsNullOrEmpty(cosmosKey))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosKey not set correctly {cosmosKey}"));
            }

            if (string.IsNullOrEmpty(cosmosDatabase))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosDatabase not set correctly {cosmosDatabase}"));
            }

            if (string.IsNullOrEmpty(cosmosCollection))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosCollection not set correctly {cosmosCollection}"));
            }

            // open and test a new client / container
            CosmosClient c = new CosmosClient(cosmosUrl.AbsoluteUri, cosmosKey, cosmosDetails.CosmosClientOptions);
            Container con = c.GetContainer(cosmosDatabase, cosmosCollection);
            await con.ReadItemAsync<dynamic>("action", new PartitionKey("0")).ConfigureAwait(false);

            return c;
        }

        /// <summary>
        /// Generic function to be used by subclasses to execute arbitrary queries and return type T.
        /// </summary>
        /// <typeparam name="T">POCO type to which results are serialized and returned.</typeparam>
        /// <param name="queryDefinition">Query to be executed.</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        private async Task<IEnumerable<T>> InternalCosmosDBSqlQuery<T>(QueryDefinition queryDefinition)
        {
            // run query
            FeedIterator<T> query = cosmosDetails.Container.GetItemQueryIterator<T>(queryDefinition, requestOptions: cosmosDetails.QueryRequestOptions);

            // return results
            return await InternalCosmosDbResults<T>(query).ConfigureAwait(false);
        }

        /// <summary>
        /// Generic function to be used by subclasses to execute arbitrary queries and return type T.
        /// </summary>
        /// <typeparam name="T">POCO type to which results are serialized and returned.</typeparam>
        /// <param name="sql">Query to be executed.</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        private async Task<IEnumerable<T>> InternalCosmosDBSqlQuery<T>(string sql)
        {
            // run query
            FeedIterator<T> query = cosmosDetails.Container.GetItemQueryIterator<T>(sql, requestOptions: cosmosDetails.QueryRequestOptions);

            // return results
            return await InternalCosmosDbResults<T>(query).ConfigureAwait(false);
        }

        /// <summary>
        /// Returnt the generic Cosmos DB results
        /// </summary>
        /// <typeparam name="T">generic type</typeparam>
        /// <param name="query">Cosmos Query</param>
        /// <returns>IEnumerable T</returns>
        private static async Task<IEnumerable<T>> InternalCosmosDbResults<T>(FeedIterator<T> query)
        {
            List<T> results = new List<T>();

            while (query.HasMoreResults)
            {
                foreach (T doc in await query.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(doc);
                }
            }

            return results;
        }
    }
}