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
        /// Validate the keyvault name
        /// </summary>
        /// <param name="name">string</param>
        /// <returns>bool</returns>
        public static bool ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            name = name.Trim();

            if (name.Length < 3 || name.Length > 24)
            {
                return false;
            }

            return true;
        }
    }
}
