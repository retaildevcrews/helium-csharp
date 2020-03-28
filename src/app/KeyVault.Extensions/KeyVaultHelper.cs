using System;
using System.Collections.Generic;

namespace KeyVault.Extensions
{
    /// <summary>
    /// Static helper methods for working with Key Vault
    /// </summary>
    public sealed class KeyVaultHelper
    {
        /// <summary>
        /// Build the Key Vault URL from the name
        /// </summary>
        /// <param name="name">Key Vault Name</param>
        /// <returns>URL to Key Vault</returns>
        public static bool BuildKeyVaultConnectionString(string name, out string keyvaultConnection)
        {
            keyvaultConnection = name?.Trim();

            // name is required
            if (string.IsNullOrEmpty(keyvaultConnection))
            {
                return false;
            }

            // build the URL
            if (!keyvaultConnection.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                keyvaultConnection = "https://" + keyvaultConnection;
            }

            if (!keyvaultConnection.EndsWith(".vault.azure.net/", StringComparison.OrdinalIgnoreCase) && !keyvaultConnection.EndsWith(".vault.azure.net", StringComparison.OrdinalIgnoreCase))
            {
                keyvaultConnection += ".vault.azure.net/";
            }

            if (!keyvaultConnection.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                keyvaultConnection += "/";
            }

            return true;
        }

        /// <summary>
        /// Get the Key Vault config from the environment variable or command line
        /// </summary>
        /// <param name="authType">out Authentication Type</param>
        /// <returns>bool</returns>
        public static bool ValidateAuthType(string authType)
        {
            // valid authentication types
            List<string> validAuthTypes = new List<string> { "MSI", "CLI", "VS" };

            // validate authType
            if (string.IsNullOrWhiteSpace(authType) || !validAuthTypes.Contains(authType))
            {
                return false;
            }

            return true;
        }
    }
}
