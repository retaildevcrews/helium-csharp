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
        /// <param name="address">Key Vault address</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddKeyVaultConnection(this IServiceCollection services, KeyVaultClient client, string address)
        {
            // add the KeyVaultConnection as a singleton
            services.AddSingleton<IKeyVaultConnection>(new KeyVaultConnection
            {
                Client = client,
                Uri = address
            });

            return services;
        }
    }
}
