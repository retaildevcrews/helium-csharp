using Microsoft.Azure.KeyVault;
using System;

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
        Uri Uri { get; set; }
    }

    /// <summary>
    /// IKeyVaultConnection implementation
    /// </summary>
    public class KeyVaultConnection : IKeyVaultConnection
    {
        public IKeyVaultClient Client { get; set; }
        public Uri Uri { get; set; }
    }
}
