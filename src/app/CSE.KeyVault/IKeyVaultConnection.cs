// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.KeyVault;

namespace CSE.KeyVault
{
    /// <summary>
    /// IKeyVault Connection Interface
    ///
    /// Stores the KeyVaultClient and Uri
    /// </summary>
    public interface IKeyVaultConnection
    {
        /// <summary>
        /// Gets or sets KeyVault Client
        /// </summary>
        IKeyVaultClient Client { get; set; }

        /// <summary>
        /// Gets or sets Address (URL)
        /// </summary>
        string Address { get; set; }
    }
}
