using System;
using System.Collections.Generic;

namespace KeyVault.Extensions
{
    /// <summary>
    /// Key Vault Secret Changed Event Arguments
    /// </summary>
    public class KeyVaultSecretChangedEventArgs : EventArgs
    {
        // list of keys that changed
        public List<string> KeysChanged;
    }
}
