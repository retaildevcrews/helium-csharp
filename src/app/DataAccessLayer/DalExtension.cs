using Microsoft.Extensions.DependencyInjection;
using System;

namespace Helium.DataAccessLayer
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
        /// <returns></returns>
        public static IServiceCollection AddDal(this IServiceCollection services, Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            // add the data access layer as a singleton
            services.AddSingleton<IDAL>(new DAL(
                cosmosUrl,
                cosmosKey,
                cosmosDatabase,
                cosmosCollection));

            return services;
        }
    }
}
