using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using System;
using System.Collections.Generic;

namespace Helium
{
    /// <summary>
    /// We override the default key vault secret manager and raise an event when the
    ///   underlying Key Vault secret changes
    /// </summary>
    public class HeliumSecretManager : DefaultKeyVaultSecretManager
    {
        // list of secrets
        private readonly SortedList<string, SecretItem> _secrets = new SortedList<string, SecretItem>();

        public override bool Load(SecretItem secret)
        {
            // call the base method
            bool b = base.Load(secret);

            if (_secrets.ContainsKey(secret.Identifier.Name))
            {
                var curr = _secrets[secret.Identifier.Name];

                // check for a change
                if (secret.Attributes.Created != curr.Attributes.Created ||
                    secret.Attributes.Updated != curr.Attributes.Updated)
                {
                    // update the current value
                    _secrets[secret.Identifier.Name] = secret;

                    // raise the event
                    KeyVaultSecretChangedEventArgs e = new KeyVaultSecretChangedEventArgs { KeyName = secret.Identifier.Name };
                    KeyVaultSecretChanged?.Invoke(this, e);
                }
            }
            else
            {
                // track the secret
                _secrets.Add(secret.Identifier.Name, secret);
            }

            return b;
        }

        // event handler
        public EventHandler<KeyVaultSecretChangedEventArgs> KeyVaultSecretChanged;

        /// <summary>
        /// Key Vault Secret Changed Event Arguments
        /// </summary>
        public class KeyVaultSecretChangedEventArgs : EventArgs
        {
            // the key that changed
            public string KeyName;
        }
    }
}
