// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.DependencyInjection;

namespace CSE.KeyVault
{
    /// <summary>
    /// Extension for DI
    /// </summary>
    public static class KeyVaultConnectionExtension
    {
        /// <summary>
        /// Adds the KeyVaultConnection to services via DI
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="client">KeyVaultClient</param>
        /// <param name="uri">Key Vault URI</param>
        /// <returns>The IServiceCollection</returns>
        public static IServiceCollection AddKeyVaultConnection(this IServiceCollection services, KeyVaultClient client, string uri)
        {
            // add the KeyVaultConnection as a singleton
            services.AddSingleton<IKeyVaultConnection>(new KeyVaultConnection
            {
                Client = client,
                Address = uri,
            });

            return services;
        }
    }
}
