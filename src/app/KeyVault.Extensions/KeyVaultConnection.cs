using Microsoft.Azure.KeyVault;

namespace KeyVault.Extensions
{
    /// <summary>
    /// IKeyVault Connection Interface
    /// 
    /// Stores the KeyVaultClient and Uri
    /// </summary>
    public interface IKeyVaultConnection
    {
        IKeyVaultClient Client { get; set; }
        string Uri { get; set; }
    }

    /// <summary>
    /// IKeyVaultConnection implementation
    /// </summary>
    public class KeyVaultConnection : IKeyVaultConnection
    {
        public IKeyVaultClient Client { get; set; }
        public string Uri { get; set; }
    }
}
