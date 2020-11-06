// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.KeyVault;

namespace CSE.KeyVault
{
    /// <summary>
    /// IKeyVaultConnection implementation
    /// </summary>
    public class KeyVaultConnection : IKeyVaultConnection
    {
        public IKeyVaultClient Client { get; set; }
        public string Address { get; set; }
    }
}
