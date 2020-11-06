// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace CSE.Helium.DataAccessLayer
{
    /// <summary>
    /// Extension to allow services.AddDal(url, key, db, coll)
    /// </summary>
    public static class DataAccessLayerExtension
    {
        /// <summary>
        /// Extension to allow services.AddDal(url, key, db, coll)
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="cosmosUrl">Cosmos URL</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <param name="cosmosDatabase">Cosmos Database</param>
        /// <param name="cosmosCollection">Cosmos Collection</param>
        /// <param name="timeout">Cosmos client timeout</param>
        /// <param name="retry">Cosmos client retry</param>
        /// <returns>ServiceCollection</returns>
        public static IServiceCollection AddDal(this IServiceCollection services, Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, int timeout, int retry)
        {
            if (cosmosUrl == null)
            {
                throw new ApplicationException("cosmosUrl cannot be null");
            }

            CosmosClientOptions options = new CosmosClientOptions
            {
                RequestTimeout = TimeSpan.FromSeconds(timeout),
                MaxRetryAttemptsOnRateLimitedRequests = retry,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(timeout),
            };

            services.AddSingleton<CosmosClient>(new CosmosClient(cosmosUrl.AbsoluteUri, cosmosKey, options));

            // add the data access layer as a singleton
            services.AddSingleton<IDAL>(new DAL(
                cosmosUrl,
                cosmosKey,
                cosmosDatabase,
                cosmosCollection,
                services.BuildServiceProvider().GetService<CosmosClient>()));

            return services;
        }
    }
}
