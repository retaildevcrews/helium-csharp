namespace KeyVault.Extensions
{
    /// <summary>
    /// Static helper methods for working with Key Vault
    /// </summary>
    public class KeyVaultHelper
    {
        /// <summary>
        /// Build the Key Vault URL from the name
        /// </summary>
        /// <param name="name">Key Vault Name</param>
        /// <returns>URL to Key Vault</returns>
        public static string BuildKeyVaultUrl(string name)
        {
            string kvUrl = name?.Trim();

            // name is required
            if (string.IsNullOrEmpty(kvUrl))
            {
                throw new System.ArgumentNullException("name", "Key Vault name is required");
            }

            // build the URL
            if (!kvUrl.StartsWith("https://"))
            {
                kvUrl = "https://" + kvUrl;
            }

            if (!kvUrl.EndsWith(".vault.azure.net/") && !kvUrl.EndsWith(".vault.azure.net"))
            {
                kvUrl += ".vault.azure.net/";
            }

            if (!kvUrl.EndsWith("/"))
            {
                kvUrl += "/";
            }

            return kvUrl;
        }
    }
}
