// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.KeyVault
{
    /// <summary>
    /// Authentication Type Enum
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// Managed Identity
        /// </summary>
        MI,

        /// <summary>
        /// Azure CLI
        /// </summary>
        CLI,

        /// <summary>
        /// Visual Studio
        /// </summary>
        VS,
    }
}
