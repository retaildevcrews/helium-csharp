// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Polly.Retry;

namespace CSE.KeyRotation
{
    public interface IKeyRotation
    {
        AsyncRetryPolicy RetryCosmosPolicy { get; }
    }
}
