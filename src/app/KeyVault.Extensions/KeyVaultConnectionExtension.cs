using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.DependencyInjection;

namespace KeyVault.Extensions
{
    public static class KeyVaultConnectionExtension
    {
        /// <summary>
        /// Adds the KeyVaultConnection to services via DI
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="client">KeyVaultClient</param>
        /// <param name="uri">Key Vault URI</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddKeyVaultConnection(this IServiceCollection services, KeyVaultClient client, string uri)
        {
            // add the KeyVaultConnection as a singleton
            services.AddSingleton<IKeyVaultConnection>(new KeyVaultConnection
            {
                Client = client,
                Uri = uri
            });

            return services;
        }
    }
}
