using Microsoft.Azure.KeyVault;
using System;

namespace CSE.KeyVault
{
    /// <summary>
    /// IKeyVault Connection Interface
    /// 
    /// Stores the KeyVaultClient and Uri
    /// </summary>
    public interface IKeyVaultConnection
    {
        IKeyVaultClient Client { get; set; }
        string Address { get; set; }
    }

    /// <summary>
    /// IKeyVaultConnection implementation
    /// </summary>
    public class KeyVaultConnection : IKeyVaultConnection
    {
        public IKeyVaultClient Client { get; set; }
        public string Address { get; set; }
    }
}
