// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using CSE.Helium.DataAccessLayer;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Extension to allow services.AddCosmosClient(url, key, db, coll)
    /// </summary>
    public static class CosmosClientExtension
    {
        /// <summary>
        /// Extension to allow services.AddDal(url, key, db, coll)
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="cosmosUrl">Cosmos URL</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <returns>ServiceCollection</returns>
        public static IServiceCollection AddCosmosClient(this IServiceCollection services, Uri cosmosUrl, string cosmosKey)
        {
            if (cosmosUrl == null)
            {
                throw new ApplicationException("cosmosUrl cannot be null");
            }

            CosmosConfig config = new CosmosConfig();

            CosmosClientOptions options = new CosmosClientOptions
            {
                RequestTimeout = TimeSpan.FromSeconds(config.Timeout),
                MaxRetryAttemptsOnRateLimitedRequests = config.Retries,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(config.Timeout),
            };

            CosmosClient client = new CosmosClient(cosmosUrl.AbsoluteUri, cosmosKey, options);

            services.AddSingleton<CosmosClient>(client);

            return services;
        }
    }
}
