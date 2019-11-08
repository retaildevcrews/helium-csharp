using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeyVault.Extensions
{
    /// <summary>
    /// EventingKeyVaultSecretManager
    /// Implements: IKeyVaultSecretManager
    /// Raise a KeyVaultSecretChanged event when the underlying Key Vault secrets change
    /// </summary>
    public class EventingKeyVaultSecretManager : IKeyVaultSecretManager
    {
        // list of secrets
        private readonly SortedList<string, SecretItem> _secrets = new SortedList<string, SecretItem>();

        // lock
        private readonly object _lock = new object();

        // list of keys that changed in this batch
        private readonly List<string> _keysChanged = new List<string>();

        // delay time for change batching
        public int BatchDelayMs = 200;

        /// <summary>
        /// IKeyVaultSecretManager:GetKey
        /// Implementation is the same as DefaultKeyVaultSecretManager
        /// </summary>
        /// <param name="secret"></param>
        /// <returns>the key Name, replacing -- with : for IConfiguration support</returns>
        public string GetKey(SecretBundle secret)
        {
            // default implementation
            return secret.SecretIdentifier.Name.Replace("--", ":");
        }

        /// <summary>
        /// IKeyVaultSecretManager
        /// 
        /// Called when a secret is loaded from Key Vault
        /// 
        /// Track the secrets to raise the KeyVaultSecret changed event
        /// </summary>
        /// <param name="secret"></param>
        /// <returns>bool</returns>
        public bool Load(SecretItem secret)
        {
            if (_secrets.ContainsKey(secret.Identifier.Name))
            {
                var curr = _secrets[secret.Identifier.Name];

                // check for a change
                if (secret.Attributes.Created != curr.Attributes.Created ||
                    secret.Attributes.Updated != curr.Attributes.Updated)
                {
                    // update the current value
                    _secrets[secret.Identifier.Name] = secret;

                    lock (_lock)
                    {
                        _keysChanged.Add(secret.Identifier.Name);

                        _ = BatchAlerts();
                    }
                }
            }
            else
            {
                // track the secret
                _secrets.Add(secret.Identifier.Name, secret);
            }

            return true;
        }

        /// <summary>
        /// Batch the Key Vault changes together
        /// </summary>
        /// <returns>Task</returns>
        private async Task BatchAlerts()
        {
            // get the current count (assumes caller has a lock on _lock)
            int count = _keysChanged.Count;

            // wait for the batch to be built
            await Task.Delay(BatchDelayMs);

            // only raise the event on the final key change
            if (count == _keysChanged.Count)
            {
                lock (_lock)
                {
                    // raise the event
                    KeyVaultSecretChanged?.Invoke(this, new KeyVaultSecretChangedEventArgs { KeysChanged = _keysChanged });

                    // clear the tracking collection
                    _keysChanged.Clear();
                }
            }
        }

        /// <summary>
        /// KeyVaultSecretChanged event handler
        /// </summary>
        public EventHandler<KeyVaultSecretChangedEventArgs> KeyVaultSecretChanged;
    }
}
